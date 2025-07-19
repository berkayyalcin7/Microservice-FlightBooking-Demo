// Konum: FlightBooker.Identity.API.Tests/IdentityApiIntegrationTests.cs
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FlightBooker.Identity.API.Tests;

public class IdentityApiIntegrationTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client;

    public IdentityApiIntegrationTests(IdentityApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldSucceed_WhenValidCredentialsAreProvided()
    {
        // --- ARRANGE (Hazýrlýk) ---
        var uniqueEmail = $"testuser_{Guid.NewGuid()}@example.com";
        var registerRequest = new RegisterDto(uniqueEmail, "Password123!");

        // --- ACT (Eylem) ---
        // Bellekteki API'mýzýn /api/auth/register endpoint'ine istek atýyoruz.
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // --- ASSERT (Doðrulama) ---

        // 1. Dönen HTTP durum kodunun baþarýlý olduðunu doðrula.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Cevabýn içeriðini kontrol et.
        var content = await response.Content.ReadFromJsonAsync<object>();
        content.Should().NotBeNull();
    }
}