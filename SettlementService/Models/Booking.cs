using System;

namespace SettlementService.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public DateTime BookingTime { get; set; }
        public string Name { get; set; }
    }
}
