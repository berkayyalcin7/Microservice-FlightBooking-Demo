using FlightBooker.Booking.API.Models;
using FlightBooker.Booking.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightBooker.Booking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController(BookingService bookingService) : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("Hello from Booking.API!");

        [HttpPost]
        public async Task<IActionResult> Post(CreateBookingDto bookingRequest)
        {
            var isSuccess = await bookingService.CreateBookingAsync(bookingRequest);

            if (isSuccess)
            {
                return Ok(new { Message = "Rezervasyon başarıyla oluşturuldu." });
            }

            return BadRequest("Uçuş bulunamadığı veya stokta yer olmadığı için rezervasyon oluşturulamadı.");
        }
    }


}
