using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;

namespace FlightBooker.Search.API.Tests
{
    // API'den d�necek cevab� deserialize edebilmek i�in DTO'yu burada da tan�ml�yoruz.
    public record FlightDto(Guid Id,
        string Airline,
        string Origin,
        string Destination,
        DateTime DepartureTime,
        DateTime ArrivalTime,
        decimal Price);

    // IClassFixture, test s�n�f�m�z boyunca WebApplicationFactory'nin tek bir �rne�ini payla�mam�z� sa�lar.
    public class SearchApiIntegrationTests : IClassFixture<WebApplicationFactory<FlightBooker.Search.API.Program>>
    {
        private readonly HttpClient _client;

        public SearchApiIntegrationTests(WebApplicationFactory<FlightBooker.Search.API.Program> factory)
        {
            // Test i�in kullan�lacak HttpClient'� fabrika �zerinden olu�turuyoruz.
            // Bu istemci, istekleri a�a ��kmadan do�rudan bellek i�i sunucuya g�nderir.
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAvailableFlights_Should_ReturnSuccessStatusCode_And_FlightList()
        {
            // --- ARRANGE (Haz�rl�k) ---
            // Bu testte haz�rl�k basit, sadece istemciyi olu�turmak yeterli.

            // --- ACT (Eylem) ---
            // /api/search endpoint'ine ger�ek bir GET iste�i at�yoruz.
            var response = await _client.GetAsync("/api/search");


            // --- ASSERT (Do�rulama) ---

            // 1. D�nen HTTP durum kodunun ba�ar�l� (200 OK) oldu�unu do�rula.
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // 2. D�nen cevab�n i�eri�ini oku ve bir listeye d�n��t�r.
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var flights = JsonSerializer.Deserialize<List<FlightDto>>(content, options);

            // 3. D�nen listenin bo� olmad���n� ve i�inde en az bir eleman oldu�unu do�rula.
            flights.Should().NotBeNull();
            flights.Should().HaveCountGreaterThan(0);

            // 4. Listenin i�indeki bir u�a��n beklenen bir de�ere sahip oldu�unu kontrol et.
            flights.Should().Contain(f => f.Airline == "Türk Hava Yolları");
        }
    }
}