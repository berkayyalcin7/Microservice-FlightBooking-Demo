using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;

namespace FlightBooker.Search.API.Tests
{
    // API'den dönecek cevabý deserialize edebilmek için DTO'yu burada da tanýmlýyoruz.
    public record FlightDto(Guid Id,
        string Airline,
        string Origin,
        string Destination,
        DateTime DepartureTime,
        DateTime ArrivalTime,
        decimal Price);

    // IClassFixture, test sýnýfýmýz boyunca WebApplicationFactory'nin tek bir örneðini paylaþmamýzý saðlar.
    public class SearchApiIntegrationTests : IClassFixture<WebApplicationFactory<FlightBooker.Search.API.Program>>
    {
        private readonly HttpClient _client;

        public SearchApiIntegrationTests(WebApplicationFactory<FlightBooker.Search.API.Program> factory)
        {
            // Test için kullanýlacak HttpClient'ý fabrika üzerinden oluþturuyoruz.
            // Bu istemci, istekleri aða çýkmadan doðrudan bellek içi sunucuya gönderir.
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAvailableFlights_Should_ReturnSuccessStatusCode_And_FlightList()
        {
            // --- ARRANGE (Hazýrlýk) ---
            // Bu testte hazýrlýk basit, sadece istemciyi oluþturmak yeterli.

            // --- ACT (Eylem) ---
            // /api/search endpoint'ine gerçek bir GET isteði atýyoruz.
            var response = await _client.GetAsync("/api/search");


            // --- ASSERT (Doðrulama) ---

            // 1. Dönen HTTP durum kodunun baþarýlý (200 OK) olduðunu doðrula.
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // 2. Dönen cevabýn içeriðini oku ve bir listeye dönüþtür.
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var flights = JsonSerializer.Deserialize<List<FlightDto>>(content, options);

            // 3. Dönen listenin boþ olmadýðýný ve içinde en az bir eleman olduðunu doðrula.
            flights.Should().NotBeNull();
            flights.Should().HaveCountGreaterThan(0);

            // 4. Listenin içindeki bir uçaðýn beklenen bir deðere sahip olduðunu kontrol et.
            flights.Should().Contain(f => f.Airline == "Türk Hava Yollarý");
        }
    }
}