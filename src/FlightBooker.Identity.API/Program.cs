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

// .NET'in varsayýlan loglamasýný kaldýrýp yerine Serilog'u ekle
builder.Host.UseSerilog();

// OpenTelemetry'yi ekle ve yapýlandýr
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation() // Gelen HTTP isteklerini izler
        .AddHttpClientInstrumentation()   // Giden HTTP isteklerini izler
        .AddConsoleExporter());          // Ýzleri konsola basar


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// --- YENÝ EKLENEN KOD BLOKU ---
// Uygulama baþlangýcýnda veritabaný baðlantýsýný ve migration'larý uygula
// Polly ile tekrar deneme politikasý ekliyoruz.
var retryPolicy = Policy
    .Handle<SqlException>()
    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        (ex, time) =>
        {
            // Her tekrar denemede log bas
            Console.WriteLine($"--> Veritabaný baðlantýsý kurulamadý. {time.TotalSeconds} saniye sonra tekrar denenecek... Hata: {ex.Message}");
        });



builder.Services.AddAuthorization();

// ---- YENÝ EKLENEN CORS SERVÝSÝ ----
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin() // Geliþtirme için tüm kaynaklara izin ver
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// ASP.NET Core Identity servislerini ekle ve JWT kullanacaðýný belirt
// AddIdentity yerine AddIdentityCore kullanabiliriz, çünkü UI istemiyoruz.
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


// Politikayý kullanarak migration'larý uygula
retryPolicy.Execute(() =>
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("--> Veritabaný migration'larý baþarýyla uygulandý.");
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

//// Sihirli satýr: /register, /login gibi endpoint'leri oluþturur
//app.MapIdentityApi<IdentityUser>();

app.MapControllers();

app.Run();

namespace FlightBooker.Identity.API
{
    public partial class Program { }
}