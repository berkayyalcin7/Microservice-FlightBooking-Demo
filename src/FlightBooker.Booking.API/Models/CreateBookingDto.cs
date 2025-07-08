namespace FlightBooker.Booking.API.Models;
public record CreateBookingDto(Guid FlightId, string PassengerName, int SeatCount);