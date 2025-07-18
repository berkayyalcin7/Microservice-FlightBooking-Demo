using FlightBooker.Booking.API.Services;
using FluentAssertions;

namespace FlightBooker.Booking.API.Tests
{
    public class BookingPriceCalculatorTests
    {

        private readonly BookingPriceCalculator _calculator;

        public BookingPriceCalculatorTests()
        {
            // Her testten �nce hesaplay�c�dan yeni bir �rnek olu�turulur.
            _calculator = new BookingPriceCalculator();
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectPrice_ForSingleSeat()
        {
            // Arrange (Haz�rl�k): Test i�in gerekli olan verileri ve nesneleri haz�rlar�z.
            var basePrice = 1000m;
            var seatCount = 1;

            // Act (Eylem): Hesaplama i�lemini ger�ekle�tiririz.
            var result = _calculator.Calculate(basePrice, seatCount);

            // Assert (Do�rulama): Sonucun bekledi�imiz gibi olup olmad���n� kontrol ederiz.
            result.Should().Be(1000); // Sonu� 1000'e e�it OLMALI.
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
            result.Should().Be(expectedPrice); // Sonu� 1800'e e�it OLMALI.
        }

        [Theory] // Bu, ayn� testi farkl� verilerle �al��t�rmam�z� sa�layan bir xUnit �zelli�idir.
        [InlineData(0, 5)]  // basePrice=0, seatCount=5
        [InlineData(1000, 0)] // basePrice=1000, seatCount=0
        [InlineData(-100, 5)] // basePrice=-100, seatCount=5
        public void Calculate_ShouldReturnZero_ForInvalidInputs(decimal basePrice, int seatCount)
        {
            // Arrange - Veriler parametreden geliyor.

            // Act
            var result = _calculator.Calculate(basePrice, seatCount);

            // Assert
            result.Should().Be(0); // Sonu� 0 OLMALI.
        }


    }
}