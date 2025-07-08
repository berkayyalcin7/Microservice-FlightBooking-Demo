using FlightBooker.Booking.API.Models;
using FlightBooker.Messages;
using MassTransit;
using System.Text.Json;

namespace FlightBooker.Booking.API.Services
{
    public class BookingService(IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint)
    {
        public async Task<bool> CreateBookingAsync(CreateBookingDto bookingRequest)
        {
            // 1. IHttpClientFactory'den isimlendirdiğimiz istemciyi al.
            var httpClient = httpClientFactory.CreateClient("SearchService");

            try
            {
                // 2. Search.API'ye senkron bir istek atarak uçuş detaylarını al.
                var response = await httpClient.GetAsync($"/api/search/{bookingRequest.FlightId}");

                if (!response.IsSuccessStatusCode)
                {
                    // Uçuş bulunamadıysa veya Search.API'de bir sorun varsa rezervasyon yapma.
                    Console.WriteLine($"Uçuş bulunamadı veya Search.API cevap vermiyor. Status: {response.StatusCode}");
                    return false;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var flight = await response.Content.ReadFromJsonAsync<FlightDto>(options);

                Console.WriteLine($"Uçuş bulundu: {flight.Airline} - {flight.Origin} -> {flight.Destination}, Fiyat: {flight.Price}");

                // 3. Burada normalde veritabanına yeni bir rezervasyon kaydı atılır.
                // Şimdilik sadece başarılı olduğunu konsola yazdırıyoruz.
                Console.WriteLine($"Rezervasyon oluşturuldu: Yolcu: {bookingRequest.PassengerName}, Uçuş ID: {bookingRequest.FlightId}, Koltuk: {bookingRequest.SeatCount}");


                var bookingId = Guid.NewGuid(); // Gerçekte veritabanından gelen ID olur
                // Olayı yayınla
                await publishEndpoint.Publish(new BookingCreatedEvent(
                    bookingId,
                    bookingRequest.PassengerName,
                    "test@example.com")); // E-posta adresini şimdilik sabit verdik

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Servisler arası iletişimde bir hata oluştu: {ex.Message}");
                return false;
            }
        }
    }
}
