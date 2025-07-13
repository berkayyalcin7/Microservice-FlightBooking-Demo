using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

builder.Services.AddControllers();



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

// Yetkilendirme (ileride kullanacaðýz)
app.UseAuthorization();

// 3. EN ÖNEMLÝ SATIR: Gelen istekleri Controller'lara yönlendiren haritalamayý yapar.
app.MapControllers();


app.Run();
