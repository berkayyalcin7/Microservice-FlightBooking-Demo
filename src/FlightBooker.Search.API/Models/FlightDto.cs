namespace FlightBooker.Search.API.Models
{
    public record FlightDto(
    Guid Id,
    string Airline,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal Price
    );
}
