using Microsoft.AspNetCore.Mvc;
using SettlementService.Dtos;
using SettlementService.Interfaces;
using SettlementService.Models;
using System.Threading.Tasks;

namespace SettlementService.Controllers
{
    // TODO Authentication
    [Route("api/v1/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingSvc _bookingSvc;

        public BookingsController(IBookingSvc bookingSvc)
        {
            _bookingSvc = bookingSvc;
        }

        /// <summary>
        /// Creates a booking
        /// </summary>
        /// <param name="dto">Booking object, contains the booking start time and representative name. Bookings are for an hour.</param>
        /// <response code="200">Booking created</response>
        /// <response code="400">Invalid booking time or booking time outside business hours (9am to 5pm)</response>
        /// <response code="409">No booking slots available for specified time</response>
        /// <response code="500">Unexpected error</response>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessfulBookingDto), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] BookingDto dto)
        {
            var result = await _bookingSvc.CreateBookingAsync(dto);

            switch (result)
            {
                case SuccessfBookingCreation success:
                    return Ok(new SuccessfulBookingDto { BookingId = success.Booking.Id });
                case InvalidBookingCreation invalid:
                    return BadRequest(new ApiError { Error = invalid.Error });
                case ConflictedBookingCreation conflict:
                    return Conflict();
                default:
                    return Problem();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _bookingSvc.GetBookingsAsync();

            return Ok(bookings);
        }
    }
}
