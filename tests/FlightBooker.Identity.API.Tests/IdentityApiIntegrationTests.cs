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
        // --- ARRANGE (Haz�rl�k) ---
        var uniqueEmail = $"testuser_{Guid.NewGuid()}@example.com";
        var registerRequest = new RegisterDto(uniqueEmail, "Password123!");

        // --- ACT (Eylem) ---
        // Bellekteki API'm�z�n /api/auth/register endpoint'ine istek at�yoruz.
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // --- ASSERT (Do�rulama) ---

        // 1. D�nen HTTP durum kodunun ba�ar�l� oldu�unu do�rula.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Cevab�n i�eri�ini kontrol et.
        var content = await response.Content.ReadFromJsonAsync<object>();
        content.Should().NotBeNull();
    }
}