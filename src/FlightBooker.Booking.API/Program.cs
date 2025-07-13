using FlightBooker.Booking.API.Services;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Polly.Extensions.Http;
using Polly;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
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


