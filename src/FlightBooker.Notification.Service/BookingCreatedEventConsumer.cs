using FlightBooker.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightBooker.Notification.Service
{
    public class BookingCreatedEventConsumer(ILogger<BookingCreatedEventConsumer> logger)
     : IConsumer<BookingCreatedEvent>
    {
        public Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var eventMessage = context.Message;
            logger.LogInformation("Yeni rezervasyon olayı alındı: BookingId={BookingId}. " +
                                  "'{PassengerName}' adlı yolcuya '{ToEmail}' adresine onay maili gönderiliyor...",
                                  eventMessage.BookingId,
                                  eventMessage.PassengerName,
                                  eventMessage.ToEmail);

            // Burada gerçek e-posta gönderme kodu olurdu...

            return Task.CompletedTask;
        }
    }
}
