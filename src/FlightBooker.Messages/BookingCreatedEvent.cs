namespace FlightBooker.Messages;
public record BookingCreatedEvent(Guid BookingId, string PassengerName, string ToEmail);