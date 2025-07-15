using FlightBooker.Notification.Service;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;


var builder = Host.CreateApplicationBuilder(args);

// --- YENÝ ve ÝYÝLEÞTÝRÝLMÝÞ SERILOG YAPILANDIRMASI ---
builder.Services.AddSerilog(config =>
{
    // Host builder'ýn kendi yapýlandýrmasýný (appsettings.json'ý okumuþ halini) kullan
    config.ReadFrom.Configuration(builder.Configuration);
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        // Bu servis HTTP istekleri alýp göndermediði için enstrümantasyonlarý eklemiyoruz.
        // Bunun yerine, MassTransit'ten gelen izleri (trace) yakalamasý gerektiðini belirtiyoruz.
        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
        .AddConsoleExporter());


builder.Services.AddHostedService<Worker>();


builder.Services.AddMassTransit(config => {
    config.AddConsumer<BookingCreatedEventConsumer>(); // Consumer'ý tanýt

    //config.UsingRabbitMq((ctx, cfg) => {
    //    cfg.Host("localhost", "/", h => {
    //        h.Username("guest");
    //        h.Password("guest");
    //    });

    //    // Consumer için kuyruk ayarlarýný yap
    //    cfg.ReceiveEndpoint("booking-created-event-queue", e => {
    //        e.ConfigureConsumer<BookingCreatedEventConsumer>(ctx);
    //    });
    //});
    config.UsingRabbitMq((ctx, cfg) => {
        // appsettings.json dosyasýndaki "MassTransit" connection string'ini kullan
        cfg.Host(builder.Configuration.GetConnectionString("MassTransit"));

        // Consumer için kuyruk ayarlarýný yap
        cfg.ConfigureEndpoints(ctx);
    });
});

var host = builder.Build();


host.Run();
