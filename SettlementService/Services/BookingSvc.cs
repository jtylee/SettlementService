using SettlementService.Dtos;
using SettlementService.Interfaces;
using SettlementService.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace SettlementService.Services
{
    public class BookingSvc : IBookingSvc
    {
        private readonly IBookingRepo _bookingRepo;
        private readonly TimeSpan _earliestBooking;
        private readonly TimeSpan _latestBooking;
        private readonly int _maxConcurrentBookings;

        public BookingSvc(IBookingRepo bookingRepo)
        {
            _bookingRepo = bookingRepo;

            // TODO move to config and handle any missing values
            _earliestBooking = new TimeSpan(9, 0, 0);
            _latestBooking = new TimeSpan(16, 0, 0);
            _maxConcurrentBookings = 4;
        }

        /// <summary>
        /// Creates a booking if validation is successful.
        /// Duplicate booking time and name combinations are considered separate bookings.
        /// </summary>
        /// <param name="dto">BookingDto object</param>
        /// <returns>BookingCreationResult</returns>
        public async Task<BookingCreationResult> CreateBookingAsync(BookingDto dto)
        {
            var bookingFormatValidation = IsBookingTimeValid(dto.BookingTime);

            if (!bookingFormatValidation.isValid)
            {
                return new InvalidBookingCreation
                {
                    Error = $"{nameof(dto.BookingTime)} is invalid.",
                };
            }

            var bookingTime = bookingFormatValidation.bookingTime.GetValueOrDefault();

            if (!IsWithinBusinessHours(bookingTime, _earliestBooking, _latestBooking))
            {
                return new InvalidBookingCreation
                {
                    Error = $"{nameof(dto.BookingTime)} needs to be within business hours",
                };
            }

            if (!await HasBookingSlotAvailableAsync(bookingTime, _maxConcurrentBookings))
            {
                return new ConflictedBookingCreation();
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                BookingTime = bookingTime,
                Name = dto.Name,
            };

            await _bookingRepo.AddAsync(booking);

            return new SuccessfBookingCreation
            {
                Booking = booking,
            };
        }

        /// <summary>
        /// Returns a list of all existing bookings
        /// </summary>
        /// <returns>List of Booking objects</returns>
        public async Task<List<Booking>> GetBookingsAsync()
        {
            return await _bookingRepo.GetAsync();
        }

        /// <summary>
        /// Checks that the specified time lies between the specified min and max booking times
        /// </summary>
        /// <param name="time">Booking time to check</param>
        /// <param name="earliestPossibleBooking">Earliest possible booking time</param>
        /// <param name="latestPossibleBooking">Latest possible booking time</param>
        /// <returns>
        /// true - specified time is within specified range (inclusive)
        /// false - specified time is outside specified range
        /// </returns>
        public bool IsWithinBusinessHours(DateTime time, TimeSpan earliestPossibleBooking, TimeSpan latestPossibleBooking)
        {
            return DateTime.Today.Add(earliestPossibleBooking) <= time
                && time <= DateTime.Today.Add(latestPossibleBooking);
        }

        /// <summary>
        /// Checks if the specified string is a valid time representation
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public (bool isValid, DateTime? bookingTime) IsBookingTimeValid(string time)
        {
            var isParsable = DateTime.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);

            return (isParsable, isParsable ? result : null);
        }

        /// <summary>
        /// Checks if any booking slots are available for specified time
        /// </summary>
        /// <param name="time">Booking start time</param>
        /// <param name="maxConcurrentBookings">Max number of booking slots</param>
        /// <returns></returns>
        public async Task<bool> HasBookingSlotAvailableAsync(DateTime time, int maxConcurrentBookings)
        {
            var slotsFilled = await _bookingRepo.GetAsync(time);

            return slotsFilled.Count < maxConcurrentBookings;
        }
    }
}
