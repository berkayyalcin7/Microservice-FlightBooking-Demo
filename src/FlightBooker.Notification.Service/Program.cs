using FlightBooker.Notification.Service;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();


builder.Services.AddMassTransit(config => {
    config.AddConsumer<BookingCreatedEventConsumer>(); // Consumer'ý tanýt

    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        // Consumer için kuyruk ayarlarýný yap
        cfg.ReceiveEndpoint("booking-created-event-queue", e => {
            e.ConfigureConsumer<BookingCreatedEventConsumer>(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
