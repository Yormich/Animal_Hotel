using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IReviewService
    {
        public Task<List<Review>> GetLastReviews(int number);

        public Task<Review?> GetClientReview(long clientId);

        public Task DeleteReview(long reviewId);

        public Task UpdateReview(Review review);

        public Task CreateReview(Review review);
    }
}
