using FlightBooker.Search.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightBooker.Search.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        // Gerçek bir projede bu veriler veritabanından gelir.
        // Şimdilik sahte verileri bellekte statik bir liste olarak tutuyoruz.
        private static readonly List<FlightDto> _flights =
        [
            new (Guid.NewGuid(), "Türk Hava Yolları", "IST", "ESB", DateTime.UtcNow.AddHours(3), DateTime.UtcNow.AddHours(4), 1250.00m),
        new (Guid.NewGuid(), "Pegasus", "SAW", "ADB", DateTime.UtcNow.AddHours(5), DateTime.UtcNow.AddHours(6), 980.50m),
        new (Guid.NewGuid(), "AnadoluJet", "ESB", "AYT", DateTime.UtcNow.AddHours(8), DateTime.UtcNow.AddHours(9), 1100.00m),
        new (Guid.NewGuid(), "SunExpress", "ADB", "DLM", DateTime.UtcNow.AddHours(12), DateTime.UtcNow.AddHours(13), 850.00m)
        ];

        [HttpGet]
        public IActionResult GetAvailableFlights()
        {
            // Tüm sahte uçuş listesini döndürür.
            return Ok(_flights);
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetFlightById(Guid id)
        {
            var flight = _flights.FirstOrDefault(f => f.Id == id);
            if (flight == null)
            {
                return NotFound(); // Uçuş bulunamazsa 404 dön
            }
            return Ok(flight);
        }
    }
}