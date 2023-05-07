using Animal_Hotel.Models.DatabaseModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
#pragma warning disable IDE0090

namespace Animal_Hotel.Services
{
    public class RoomService : IRoomService
    {
        private readonly AnimalHotelDbContext _db;
        private readonly IMemoryCache _cache;
        public RoomService(AnimalHotelDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<int> GetRoomsCountAsync()
        {
            _cache.TryGetValue("GetRoomsCountAsync", out int? count);

            if (count == null)
            {
                count = await _db.Rooms.CountAsync();

                _cache.Set($"GetRoomsCountAsync", count,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(20)));
            }

            return (int)count;
        }

        public async Task<IQueryable<Room>> GetRoomsByPageIndex(int pageIndex, int pageSize)
        {
            _cache.TryGetValue($"RoomsByPage-{pageIndex * pageSize}", out IQueryable<Room>? rooms);

            if (rooms == null)
            {
                string sql = "SELECT * FROM dbo.room r" +
                " ORDER BY r.id" +
                " OFFSET @skipAmount ROWS" +
                " FETCH NEXT @toTake ROWS ONLY";
                SqlParameter skipAmount = new SqlParameter("skipAmount", pageSize * (pageIndex - 1));
                SqlParameter toTake = new SqlParameter("toTake", pageSize);

                rooms = await Task.Run(() => _db.Rooms.FromSqlRaw(sql, skipAmount, toTake)
                    .Include(r => r.RoomType)
                    .Include(r => r.Enclosures)
                    .AsQueryable());

                _cache.Set($"RoomsByPage-{pageIndex * pageSize}", await rooms.ToListAsync(), 
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(20)));
            }

            return rooms;
        }

        public Task<IQueryable<Room>> GetRoomsWithEnclosures(short id)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<Room>> GetRoomWithEnclosure(short id)
        {
            throw new NotImplementedException();
        }
    }
}
