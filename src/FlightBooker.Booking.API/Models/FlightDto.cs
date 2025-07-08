namespace FlightBooker.Booking.API.Models
{
    public record FlightDto(Guid Id, string Airline, string Origin, string Destination, decimal Price);
}
