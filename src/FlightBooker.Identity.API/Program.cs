using FlightBooker.Identity.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Serilog;

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


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// --- YEN� EKLENEN KOD BLOKU ---
// Uygulama ba�lang�c�nda veritaban� ba�lant�s�n� ve migration'lar� uygula
// Polly ile tekrar deneme politikas� ekliyoruz.
var retryPolicy = Policy
    .Handle<SqlException>()
    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        (ex, time) =>
        {
            // Her tekrar denemede log bas
            Console.WriteLine($"--> Veritaban� ba�lant�s� kurulamad�. {time.TotalSeconds} saniye sonra tekrar denenecek... Hata: {ex.Message}");
        });



builder.Services.AddAuthorization();

// ---- YEN� EKLENEN CORS SERV�S� ----
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin() // Geli�tirme i�in t�m kaynaklara izin ver
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// ASP.NET Core Identity servislerini ekle ve JWT kullanaca��n� belirt
// AddIdentity yerine AddIdentityCore kullanabiliriz, ��nk� UI istemiyoruz.
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Politikay� kullanarak migration'lar� uygula
retryPolicy.Execute(() =>
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("--> Veritaban� migration'lar� ba�ar�yla uyguland�.");
    }
});


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

//// Sihirli sat�r: /register, /login gibi endpoint'leri olu�turur
//app.MapIdentityApi<IdentityUser>();

app.MapControllers();

app.Run();

namespace FlightBooker.Identity.API
{
    public partial class Program { }
}