// Konum: FlightBooker.Booking.API.Tests/BookingServiceTests.cs
using FlightBooker.Booking.API.Models;
using FlightBooker.Booking.API.Services;
using FlightBooker.Messages;
using FluentAssertions;
using MassTransit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace FlightBooker.Booking.API.Tests;

public class BookingServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        // Her testten önce, bağımlılıklarımızın sahte kopyalarını oluşturuyoruz.
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        // Test edeceğimiz BookingService'i, bu sahte nesneleri kullanarak oluşturuyoruz.
        // .Object diyerek mock nesnesinin taklit ettiği asıl nesneyi alırız.
        _bookingService = new BookingService(_httpClientFactoryMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_Should_Succeed_And_PublishEvent_WhenFlightExists()
    {
        // --- ARRANGE (Hazırlık) ---

        // 1. Sahte bir uçuş ID'si ve rezervasyon isteği oluşturuyoruz.
        var flightId = Guid.NewGuid();
        var createBookingRequest = new CreateBookingDto(flightId, "Test Passenger", 1);

        // 2. Search.API'den dönecek sahte başarılı cevabı hazırlıyoruz.
        var fakeFlight = new FlightDto(flightId, "Test Airline", "IST", "ESB", 1500m);
        var fakeHttpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(fakeFlight))
        };

        // 3. IHttpClientFactory'nin davranışını programlıyoruz.
        // Bu, Moq'un en güçlü olduğu yerdir.
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            // Search.API'ye herhangi bir GET isteği yapıldığında...
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            // ... bizim hazırladığımız sahte cevabı dön.
            .ReturnsAsync(fakeHttpResponse);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            // HttpClient'ın BaseAddress'ini ayarlamak önemlidir.
            BaseAddress = new Uri("http://fake-search-api")
        };

        // HttpClientFactory çağrıldığında bizim sahte HttpClient'ımızı döndürmesini sağlıyoruz.
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);


        // --- ACT (Eylem) ---

        // Test edeceğimiz asıl metodu çağırıyoruz.
        var result = await _bookingService.CreateBookingAsync(createBookingRequest);


        // --- ASSERT (Doğrulama) ---

        // 1. Metodun sonucunun 'true' olduğunu doğruluyoruz.
        result.Should().BeTrue();

        // 2. En önemli kontrol: `IPublishEndpoint`'in `Publish` metodunun,
        //    içinde `BookingCreatedEvent` tipinde bir mesajla,
        //    tam olarak 1 kez çağrıldığını doğruluyoruz.
        _publishEndpointMock.Verify(p => p.Publish(
                It.IsAny<BookingCreatedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}