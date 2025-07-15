using FlightBooker.Notification.Service;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;


var builder = Host.CreateApplicationBuilder(args);

// --- YEN� ve �Y�LE�T�R�LM�� SERILOG YAPILANDIRMASI ---
builder.Services.AddSerilog(config =>
{
    // Host builder'�n kendi yap�land�rmas�n� (appsettings.json'� okumu� halini) kullan
    config.ReadFrom.Configuration(builder.Configuration);
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        // Bu servis HTTP istekleri al�p g�ndermedi�i i�in enstr�mantasyonlar� eklemiyoruz.
        // Bunun yerine, MassTransit'ten gelen izleri (trace) yakalamas� gerekti�ini belirtiyoruz.
        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
        .AddConsoleExporter());


builder.Services.AddHostedService<Worker>();


builder.Services.AddMassTransit(config => {
    config.AddConsumer<BookingCreatedEventConsumer>(); // Consumer'� tan�t

    //config.UsingRabbitMq((ctx, cfg) => {
    //    cfg.Host("localhost", "/", h => {
    //        h.Username("guest");
    //        h.Password("guest");
    //    });

    //    // Consumer i�in kuyruk ayarlar�n� yap
    //    cfg.ReceiveEndpoint("booking-created-event-queue", e => {
    //        e.ConfigureConsumer<BookingCreatedEventConsumer>(ctx);
    //    });
    //});
    config.UsingRabbitMq((ctx, cfg) => {
        // appsettings.json dosyas�ndaki "MassTransit" connection string'ini kullan
        cfg.Host(builder.Configuration.GetConnectionString("MassTransit"));

        // Consumer i�in kuyruk ayarlar�n� yap
        cfg.ConfigureEndpoints(ctx);
    });
});

var host = builder.Build();


host.Run();
