using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IReviewService
    {
        public Task<IQueryable<Review>> GetLastReviews(int number);
    }
}
