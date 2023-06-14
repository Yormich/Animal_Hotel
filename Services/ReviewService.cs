using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
#pragma warning disable IDE0090

namespace Animal_Hotel.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AnimalHotelDbContext _db;
        private readonly IMemoryCache _cache;

        public ReviewService(AnimalHotelDbContext db, IMemoryCache memoryCache)
        {
            _db = db;
            _cache = memoryCache;
        }

        public async Task<(bool, string? message)> CreateReview(Review review)
        {
            string sql = "EXEC dbo.CreateReview" +
                        "   @rating = @Rating, " +
                        "   @comment = @Comment," +
                        "   @writing_date = @writingDate," +
                        "   @client_id = @clientId;";

            SqlParameter ratingParam = new("Rating", review.Rating);
            SqlParameter commentParam = new("Comment", review.Comment);
            SqlParameter dateParam = new("writingDate", DateTime.Now);
            SqlParameter clientParam = new("clientId", review.ClientId);

            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, ratingParam, commentParam, dateParam, clientParam);
            }
            catch(SqlException se)
            {
                Console.WriteLine(se.Message);
                return new(false, se.Message);
            }

            return new(true, string.Empty);
        }

        public Task DeleteReview(long reviewId)
        {
            string sql = "DELETE FROM dbo.client_review" +
                " WHERE id = @reviewId";
            SqlParameter idParam = new("reviewId", reviewId);

            return _db.Database.ExecuteSqlRawAsync(sql, idParam);
        }

        public Task<Review?> GetClientReview(long clientId)
        {
            string sql = "SELECT * FROM dbo.client_review cr" +
                " WHERE cr.client_id = @clientId";
            SqlParameter idParam = new("clientId", clientId);

            return _db.Reviews.FromSqlRaw(sql, idParam)
                .Include(r => r.Client)
                .AsQueryable()
                .FirstOrDefaultAsync();
        }

        public Task UpdateReview(Review review)
        {
            string sql = "UPDATE dbo.client_review" +
                " SET rating = @rating, comment = @comment, " +
                " writing_date = CONVERT(DATETIME, @writingDate)" +
                " WHERE id = @reviewId";

            SqlParameter idParam = new("reviewId", review.Id);
            SqlParameter dateParam = new("writingDate", DateTime.Now);
            SqlParameter commentParam = new("comment", review.Comment);
            SqlParameter ratingParam = new("rating", review.Rating);

            return _db.Database.ExecuteSqlRawAsync(sql, idParam, dateParam, commentParam, ratingParam);
        }

        public async Task<List<Review>> GetLastReviews(int number)
        {
            string sql = $"SELECT TOP {number} * FROM dbo.client_review cr" +
                " ORDER BY cr.writing_date DESC, cr.rating DESC";

            _cache.TryGetValue($"GetLastReviews({number})", out List<Review>? reviews);

            if (reviews == null)
            {
                SqlParameter reviewAmount = new SqlParameter("reviewAmount", number);
                reviews = await _db.Reviews.FromSqlRaw(sql, reviewAmount)
                            .Include(r => r.Client)
                            .AsQueryable()
                            .ToListAsync();

                _cache.Set($"GetLastReviews({number})", reviews, 
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }

            return reviews;
        }
    }
}
