using FlightBooker.Booking.API.Services;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Polly.Extensions.Http;
using Polly;
using System.Text;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using RabbitMQ.Client;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

// .NET'in varsay�lan loglamas�n� kald�r�p yerine Serilog'u ekle
builder.Host.UseSerilog();

// OpenTelemetry'yi ekle ve yap�land�r
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation() // Gelen HTTP isteklerini izler
        .AddHttpClientInstrumentation()   // Giden HTTP isteklerini izler
        .AddConsoleExporter());          // �zleri konsola basar

// 1. Controller'lar�n �al��mas� i�in gereken temel servisleri kaydeder.
builder.Services.AddControllers();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true, // �mza do�rulamas�n� zorunlu k�l
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// Son versiyon 9.0 g�ncellemesi ile kullan�m
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        //Uri = new Uri("amqp://guest:guest@localhost:5672"),
        Uri=new Uri("amqp://guest:guest@rabbitmq:5672"),
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
}).AddHealthChecks().AddRabbitMQ();


builder.Services.AddHttpClient("SearchService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:SearchAPI"]);
})
// GE��C� HTTP HATALARINI YAKALAYACAK B�R POL�T�KA EKLE
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());



builder.Services.AddScoped<BookingService>();

// MassTransit yap�land�rmas�.
builder.Services.AddMassTransit(config => {

    // BU YAPILANDIRMA NORMAL IIS DE �ALI�AN LOCALIMIZ

    //config.UsingRabbitMq((ctx, cfg) => {
    //    cfg.Host("localhost", "/", h => {
    //        h.Username("guest");
    //        h.Password("guest");
    //    });
    //});

    // DOCKER ���N YAPILANDIRMAMIZ

    config.UsingRabbitMq((ctx, cfg) => {
        // appsettings.json dosyas�ndaki "MassTransit" connection string'ini kullan
        cfg.Host(builder.Configuration.GetConnectionString("MassTransit"));

        // Consumer i�in kuyruk ayarlar�n� yap
        cfg.ConfigureEndpoints(ctx);
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// 3. EN �NEML� SATIR: Gelen istekleri Controller'lara y�nlendiren haritalamay� yapar.
app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // Cevab� detayl� JSON olarak formatla
});

app.Run();


// 1. TEKRAR DENEME POL�T�KASI
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    // Bir istek ba�ar�s�z oldu�unda (�rn: 5XX, 408), 3 kez tekrar dene.
    // Her deneme aras�nda �ssel olarak artan bir s�re bekle (1s, 2s, 4s).
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound) // 404'� de ge�ici hata say
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                // Her tekrar denemede konsola log bas
                Console.WriteLine($"--> Search.API'ye istek ba�ar�s�z oldu. Tekrar deneniyor... Deneme: {retryAttempt}, Bekleme: {timespan.TotalSeconds}s");
            });
}

// 2. DEVRE KES�C� POL�T�KASI
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    // E�er ard���k 5 istek ba�ar�s�z olursa, devreyi 30 saniyeli�ine kes.
    // Bu s�re boyunca hi�bir istek g�nderilmez, an�nda hata d�n�l�r.
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
            onBreak: (result, timespan) =>
            {
                Console.WriteLine("--> Devre kesici a��ld� (OnBreak)! 30 saniye boyunca istek g�nderilmeyecek.");
            },
            onReset: () =>
            {
                Console.WriteLine("--> Devre kesici kapand� (OnReset). �stekler tekrar g�nderiliyor.");
            });
}


