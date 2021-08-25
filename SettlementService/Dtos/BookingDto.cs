using System.ComponentModel.DataAnnotations;

namespace SettlementService.Dtos
{
    public class BookingDto
    {
        /// <summary>
        /// Start time of Booking
        /// </summary>
        /// <example>09:00</example>
        [Required]
        [MaxLength(5)]
        public string BookingTime { get; set; }

        /// <summary>
        /// Name of representative placing booking
        /// </summary>
        /// <example>James</example>
        [Required]
        public string Name { get; set; }
    }
}
