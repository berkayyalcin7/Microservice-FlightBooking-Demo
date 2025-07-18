using FlightBooker.Booking.API.Services;
using FluentAssertions;

namespace FlightBooker.Booking.API.Tests
{
    public class BookingPriceCalculatorTests
    {

        private readonly BookingPriceCalculator _calculator;

        public BookingPriceCalculatorTests()
        {
            // Her testten önce hesaplayýcýdan yeni bir örnek oluþturulur.
            _calculator = new BookingPriceCalculator();
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectPrice_ForSingleSeat()
        {
            // Arrange (Hazýrlýk): Test için gerekli olan verileri ve nesneleri hazýrlarýz.
            var basePrice = 1000m;
            var seatCount = 1;

            // Act (Eylem): Hesaplama iþlemini gerçekleþtiririz.
            var result = _calculator.Calculate(basePrice, seatCount);

            // Assert (Doðrulama): Sonucun beklediðimiz gibi olup olmadýðýný kontrol ederiz.
            result.Should().Be(1000); // Sonuç 1000'e eþit OLMALI.
        }

        [Fact]
        public void Calculate_ShouldApplyDiscount_ForMultipleSeats()
        {
            // Arrange
            var basePrice = 1000m;
            var seatCount = 2;
            var expectedPrice = 1800m; // 2 * 1000 = 2000, %10 indirimle 1800

            // Act
            var result = _calculator.Calculate(basePrice, seatCount);

            // Assert
            result.Should().Be(expectedPrice); // Sonuç 1800'e eþit OLMALI.
        }

        [Theory] // Bu, ayný testi farklý verilerle çalýþtýrmamýzý saðlayan bir xUnit özelliðidir.
        [InlineData(0, 5)]  // basePrice=0, seatCount=5
        [InlineData(1000, 0)] // basePrice=1000, seatCount=0
        [InlineData(-100, 5)] // basePrice=-100, seatCount=5
        public void Calculate_ShouldReturnZero_ForInvalidInputs(decimal basePrice, int seatCount)
        {
            // Arrange - Veriler parametreden geliyor.

            // Act
            var result = _calculator.Calculate(basePrice, seatCount);

            // Assert
            result.Should().Be(0); // Sonuç 0 OLMALI.
        }


    }
}