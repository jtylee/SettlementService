using Moq;
using NUnit.Framework;
using SettlementService.Dtos;
using SettlementService.Interfaces;
using SettlementService.Models;
using SettlementService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class BookingSvcTests
    {
        private IBookingSvc _sut;
        private Mock<IBookingRepo> _bookingRepoMock;

        [SetUp]
        public void Setup()
        {
            _bookingRepoMock = new Mock<IBookingRepo>();
            _sut = new BookingSvc(_bookingRepoMock.Object);

            _bookingRepoMock.Setup(m => m.AddAsync(It.IsAny<Booking>())).Returns<Booking>(x => Task.FromResult(x));
        }

        [Test]
        public async Task CreateBooking_Should_Create_Successfully()
        {
            var dto = new BookingDto
            {
                BookingTime = "09:00",
                Name = "abc",
            };

            var expectedBookingTime = CreateBookingTime(9, 0, 0);

            _bookingRepoMock.Setup(m => m.GetAsync(expectedBookingTime)).Returns(Task.FromResult(new List<Booking>()));

            var result = await _sut.CreateBookingAsync(dto);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<SuccessfBookingCreation>(result);
            Assert.AreEqual(expectedBookingTime, ((SuccessfBookingCreation)result).Booking.BookingTime);
        }

        [TestCase(9, 0, true)]
        [TestCase(16, 0, true)]
        [TestCase(9, 1, true)]
        [TestCase(15, 59, true)]
        [TestCase(8, 59, false)]
        [TestCase(16, 1, false)]
        [TestCase(0, 0, false)]
        [TestCase(23, 59, false)]
        public void IsWithinBusinessHours_Should_Return_Correctly(int hours, int minutes, bool expectedResult)
        {
            var earliestBooking = new TimeSpan(9, 0, 0);
            var latestBooking = new TimeSpan(16, 0, 0);

            var bookingTime = DateTime.Today.Add(new TimeSpan(hours, minutes, 00));

            var result = _sut.IsWithinBusinessHours(bookingTime, earliestBooking, latestBooking);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [TestCase("a")]
        [TestCase("123")]
        [TestCase("24:00")]
        [TestCase("99:99")]
        public void IsBookingTimeValid_Should_Identify_Invalid_Times(string time)
        {
            var (isValid, bookingTime) = _sut.IsBookingTimeValid(time);

            Assert.AreEqual(false, isValid);
            Assert.IsNull(bookingTime);
        }

        [TestCase("00:00")]
        [TestCase("00:01")]
        [TestCase("23:59")]
        [TestCase("09:30")]
        public void IsBookingTimeValid_Should_Identify_Valid_Times(string time)
        {
            var (isValid, bookingTime) = _sut.IsBookingTimeValid(time);

            Assert.AreEqual(true, isValid);
            Assert.AreEqual(time, bookingTime.Value.ToString("HH:mm"));
        }

        [TestCase(9, 0, 0, 4)]
        [TestCase(9, 0, 1, 4)]
        [TestCase(9, 0, 2, 4)]
        [TestCase(9, 0, 3, 4)]
        public async Task HasBookingSlotAvailableAsync_Should_Return_True_If_Max_Slots_Not_Reached(int hours, int min, int currentBookingsCount, int maxConcurrentBookings)
        {
            var bookingTime = CreateBookingTime(hours, min, 0);

            // Create the current bookings and seed the mocked repo method
            var bookings = Enumerable
                .Range(1, currentBookingsCount)
                .Select(x => new Booking
                {
                    Id = Guid.NewGuid(),
                    BookingTime = bookingTime,
                    Name = x.ToString(),
                })
                .ToList();

            _bookingRepoMock.Setup(m => m.GetAsync(bookingTime)).Returns(Task.FromResult(bookings));

            var result = await _sut.HasBookingSlotAvailableAsync(bookingTime, maxConcurrentBookings);

            Assert.AreEqual(true, result);
        }

        [TestCase(9, 0, 4, 4)]
        [TestCase(9, 0, 10, 4)]
        public async Task HasBookingSlotAvailableAsync_Should_Return_False_If_Max_Slots_Reached(int hours, int min, int currentBookingsCount, int maxConcurrentBookings)
        {
            var bookingTime = CreateBookingTime(hours, min, 0);

            // Create the current bookings and seed the mocked repo method
            var bookings = Enumerable
                .Range(1, currentBookingsCount)
                .Select(x => new Booking
                {
                    Id = Guid.NewGuid(),
                    BookingTime = bookingTime,
                    Name = x.ToString(),
                })
                .ToList();

            _bookingRepoMock.Setup(m => m.GetAsync(bookingTime)).Returns(Task.FromResult(bookings));

            var result = await _sut.HasBookingSlotAvailableAsync(bookingTime, maxConcurrentBookings);

            Assert.AreEqual(false, result);
        }

        [Test]
        public async Task HasBookingSlotAvailableAsync_Should_Return_False_If_Max_Slots_Reached_With_Overlapping_Times()
        {
            // 11:30
            var bookingTime = CreateBookingTime(11, 30, 0);

            var bookings = new List<Booking>
            {
                new Booking { BookingTime = CreateBookingTime(11, 0) },
                new Booking { BookingTime = CreateBookingTime(11, 0) },
                new Booking { BookingTime = CreateBookingTime(12, 0) },
                new Booking { BookingTime = CreateBookingTime(12, 0) },
            };

            _bookingRepoMock.Setup(m => m.GetAsync(bookingTime)).Returns(Task.FromResult(bookings));

            var result = await _sut.HasBookingSlotAvailableAsync(bookingTime, 4);

            Assert.AreEqual(false, result);
        }

        private DateTime CreateBookingTime(int hours, int min, int seconds = 0) => DateTime.Today.Add(new TimeSpan(hours, min, seconds));
    }
}
