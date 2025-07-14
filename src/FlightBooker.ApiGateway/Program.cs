
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Threading.RateLimiting;

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



builder.Services.AddControllers();

// --- YEN� EKLENEN KOD BLOKLARI ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(policyName: "fixed", fixedWindow =>
    {
        // Politika: Her 10 saniyede en fazla 10 istek kabul et.
        fixedWindow.PermitLimit = 5;
        fixedWindow.Window = TimeSpan.FromSeconds(10);
        //fixedWindow.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        //fixedWindow.QueueLimit = 5; // S�n�r� a�an 5 iste�i s�raya al, fazlas�n� reddet.
    });
});


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRateLimiter();

// Gelen istekleri YARP'�n y�nlendirme haritas�na g�nder
app.MapReverseProxy().RequireRateLimiting("fixed");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
