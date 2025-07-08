using FlightBooker.Notification.Service;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();


builder.Services.AddMassTransit(config => {
    config.AddConsumer<BookingCreatedEventConsumer>(); // Consumer'� tan�t

    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        // Consumer i�in kuyruk ayarlar�n� yap
        cfg.ReceiveEndpoint("booking-created-event-queue", e => {
            e.ConfigureConsumer<BookingCreatedEventConsumer>(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
