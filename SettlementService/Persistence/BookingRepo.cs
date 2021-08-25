using Microsoft.EntityFrameworkCore;
using SettlementService.Interfaces;
using SettlementService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SettlementService.Persistence
{
    public class BookingRepo : RepositoryBase, IBookingRepo
    {
        public BookingRepo(SettlementContext ctx) : base(ctx)
        {

        }

        public async Task<Booking> AddAsync(Booking booking)
        {
            _ctx.Bookings.Add(booking);

            await _ctx.SaveChangesAsync();

            return booking;
        }

        public async Task<List<Booking>> GetAsync()
        {
            return await _ctx.Bookings.ToListAsync();
        }

        public async Task<List<Booking>> GetAsync(DateTime timeSlotStart)
        {
            var timeSlotEnd = timeSlotStart.AddHours(1);

            // TODO dates and timezones

            var existing = await _ctx.Bookings
                .Where(b => 
                    (timeSlotStart <= b.BookingTime && b.BookingTime <= timeSlotEnd) ||
                    (timeSlotStart <= b.BookingTime.AddHours(1) && b.BookingTime.AddHours(1) <= timeSlotEnd)
                )
                .ToListAsync();

            return existing;
        }
    }
}
