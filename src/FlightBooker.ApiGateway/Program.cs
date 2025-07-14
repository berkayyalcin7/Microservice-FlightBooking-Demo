
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

// .NET'in varsayýlan loglamasýný kaldýrýp yerine Serilog'u ekle
builder.Host.UseSerilog();

// OpenTelemetry'yi ekle ve yapýlandýr
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation() // Gelen HTTP isteklerini izler
        .AddHttpClientInstrumentation()   // Giden HTTP isteklerini izler
        .AddConsoleExporter());          // Ýzleri konsola basar



builder.Services.AddControllers();

// --- YENÝ EKLENEN KOD BLOKLARI ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(policyName: "fixed", fixedWindow =>
    {
        // Politika: Her 10 saniyede en fazla 10 istek kabul et.
        fixedWindow.PermitLimit = 5;
        fixedWindow.Window = TimeSpan.FromSeconds(10);
        //fixedWindow.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        //fixedWindow.QueueLimit = 5; // Sýnýrý aþan 5 isteði sýraya al, fazlasýný reddet.
    });
});


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRateLimiter();

// Gelen istekleri YARP'ýn yönlendirme haritasýna gönder
app.MapReverseProxy().RequireRateLimiting("fixed");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
