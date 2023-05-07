using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

        public async Task<List<Review>> GetLastReviews(int number)
        {
            string sql = "SELECT TOP 5 * FROM dbo.client_review cr" +
                " ORDER BY cr.rating DESC, cr.writing_date DESC";

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
