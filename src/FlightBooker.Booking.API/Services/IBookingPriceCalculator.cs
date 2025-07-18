namespace FlightBooker.Booking.API.Services
{
    public interface IBookingPriceCalculator
    {
        decimal Calculate(decimal basePrice, int seatCount);
    }
}
