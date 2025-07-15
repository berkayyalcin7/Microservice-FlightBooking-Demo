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

// .NET'in varsayýlan loglamasýný kaldýrýp yerine Serilog'u ekle
builder.Host.UseSerilog();

// OpenTelemetry'yi ekle ve yapýlandýr
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation() // Gelen HTTP isteklerini izler
        .AddHttpClientInstrumentation()   // Giden HTTP isteklerini izler
        .AddConsoleExporter());          // Ýzleri konsola basar

// 1. Controller'larýn çalýþmasý için gereken temel servisleri kaydeder.
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
            ValidateIssuerSigningKey = true, // Ýmza doðrulamasýný zorunlu kýl
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// Son versiyon 9.0 güncellemesi ile kullaným
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
// GEÇÝCÝ HTTP HATALARINI YAKALAYACAK BÝR POLÝTÝKA EKLE
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());



builder.Services.AddScoped<BookingService>();

// MassTransit yapýlandýrmasý.
builder.Services.AddMassTransit(config => {

    // BU YAPILANDIRMA NORMAL IIS DE ÇALIÞAN LOCALIMIZ

    //config.UsingRabbitMq((ctx, cfg) => {
    //    cfg.Host("localhost", "/", h => {
    //        h.Username("guest");
    //        h.Password("guest");
    //    });
    //});

    // DOCKER ÝÇÝN YAPILANDIRMAMIZ

    config.UsingRabbitMq((ctx, cfg) => {
        // appsettings.json dosyasýndaki "MassTransit" connection string'ini kullan
        cfg.Host(builder.Configuration.GetConnectionString("MassTransit"));

        // Consumer için kuyruk ayarlarýný yap
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

// 3. EN ÖNEMLÝ SATIR: Gelen istekleri Controller'lara yönlendiren haritalamayý yapar.
app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // Cevabý detaylý JSON olarak formatla
});

app.Run();


// 1. TEKRAR DENEME POLÝTÝKASI
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    // Bir istek baþarýsýz olduðunda (örn: 5XX, 408), 3 kez tekrar dene.
    // Her deneme arasýnda üssel olarak artan bir süre bekle (1s, 2s, 4s).
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound) // 404'ü de geçici hata say
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                // Her tekrar denemede konsola log bas
                Console.WriteLine($"--> Search.API'ye istek baþarýsýz oldu. Tekrar deneniyor... Deneme: {retryAttempt}, Bekleme: {timespan.TotalSeconds}s");
            });
}

// 2. DEVRE KESÝCÝ POLÝTÝKASI
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    // Eðer ardýþýk 5 istek baþarýsýz olursa, devreyi 30 saniyeliðine kes.
    // Bu süre boyunca hiçbir istek gönderilmez, anýnda hata dönülür.
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
            onBreak: (result, timespan) =>
            {
                Console.WriteLine("--> Devre kesici açýldý (OnBreak)! 30 saniye boyunca istek gönderilmeyecek.");
            },
            onReset: () =>
            {
                Console.WriteLine("--> Devre kesici kapandý (OnReset). Ýstekler tekrar gönderiliyor.");
            });
}


