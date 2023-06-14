using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels;
using Microsoft.Identity.Client;

namespace Animal_Hotel.Services
{
    public interface IBookingService
    {
        public Task<(bool success, string? message)> CreateBooking(Booking newBooking);

        public Task DeleteBooking(long animalBookedId);

        public Task<IQueryable<Booking>> GetClientBookingsByPage(long clientId, int pageIndex, int pageSize);

        public Task<int> GetClientBookingsCount(long clientId);

        public Task<List<ReceptionistReport>> GetBookingsByDate(DateTime targetDate);

        public Task<Booking?> GetBookingById(long? animalId);
    }
}
