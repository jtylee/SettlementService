using SettlementService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SettlementService.Interfaces
{
    public interface IBookingRepo
    {
        Task<Booking> AddAsync(Booking booking);
        Task<List<Booking>> GetAsync();
        Task<List<Booking>> GetAsync(DateTime timeSlotStart);
    }
}
