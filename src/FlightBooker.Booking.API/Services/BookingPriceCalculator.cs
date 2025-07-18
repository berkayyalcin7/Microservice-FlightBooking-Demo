namespace FlightBooker.Booking.API.Services
{
    public class BookingPriceCalculator : IBookingPriceCalculator
    {
        // Basit bir iş kuralı: 2 veya daha fazla koltuk alınırsa %10 indirim uygula.
        public decimal Calculate(decimal basePrice, int seatCount)
        {
            if (basePrice <= 0 || seatCount <= 0)
            {
                return 0;
            }

            var totalPrice = basePrice * seatCount;

            if (seatCount >= 2)
            {
                totalPrice *= 0.90m; // %10 indirim
            }

            return totalPrice;
        }
    }
}
