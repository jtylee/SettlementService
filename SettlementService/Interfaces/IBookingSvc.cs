using SettlementService.Dtos;
using SettlementService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SettlementService.Interfaces
{
    public interface IBookingSvc
    {
        Task<BookingCreationResult> CreateBookingAsync(BookingDto dto);
        Task<List<Booking>> GetBookingsAsync();
        bool IsWithinBusinessHours(DateTime time, TimeSpan earliestBooking, TimeSpan latestBooking);
        (bool isValid, DateTime? bookingTime) IsBookingTimeValid(string time);
        Task<bool> HasBookingSlotAvailableAsync(DateTime time, int maxConcurrentBookings);
    }
}
