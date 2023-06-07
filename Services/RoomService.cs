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
        private readonly IEnclosureService _enclosureService;


        public RoomService(AnimalHotelDbContext db, IMemoryCache cache, IEnclosureService enclosureService)
        {
            _db = db;
            _cache = cache;
            _enclosureService = enclosureService;
        }

        public async Task<int> GetRoomsCountAsync(bool withClosedToBook = false)
        {
            _cache.TryGetValue($"GetRoomsCountAsync{withClosedToBook}", out int? count);

            if (count == null)
            {
                count = await (withClosedToBook ? _db.Rooms : _db.Rooms.Where(r => !r.UnableToBook)).CountAsync();
                _cache.Set($"GetRoomsCountAsync", count,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(20)));
            }
            return (int)count;
        }

        public async Task<List<Room>> GetRoomsByPageIndex(int pageIndex, int pageSize, bool withClosedToBook = false)
        {
            string sql = "SELECT * FROM dbo.room r " +
            (withClosedToBook ? string.Empty : " WHERE r.unable_to_book = 0 ") +
            " ORDER BY r.id" +
            " OFFSET @skipAmount ROWS" +
            " FETCH NEXT @toTake ROWS ONLY";
            SqlParameter skipAmount = new SqlParameter("skipAmount", pageSize * (pageIndex - 1));
            SqlParameter toTake = new SqlParameter("toTake", pageSize);

            List<Room> rooms = await _db.Rooms.FromSqlRaw(sql, skipAmount, toTake)
                .Include(r => r.RoomType)
                .Include(r => r.Enclosures)
                .AsQueryable()
                .ToListAsync();



            return await Task.Run(() =>
            {
                rooms.ForEach(r =>
                {
                    r.AvailableEnclosuresAmount = Task.Run(() => this.GetAvailableEnclosuresForRoom(r.Id).CountAsync()).Result;
                });
                return rooms;
            });
        }

        public async Task<List<Room>> GetManagerRoomsByPageIndex(int pageIndex, int pageSize, bool withClosedToBook = true)
        {
            string sql = "SELECT * FROM dbo.room r " +
                (withClosedToBook ? string.Empty : " WHERE r.unable_to_book = 0 ") +
                " ORDER BY r.id" +
                " OFFSET @skipAmount ROWS" +
                " FETCH NEXT @toTake ROWS ONLY";

            SqlParameter skipAmountParam = new("skipAmount", pageSize * (pageIndex - 1));
            SqlParameter toTakeParam = new("toTake", pageSize);

            var rooms = await _db.Rooms.FromSqlRaw(sql, skipAmountParam, toTakeParam)
                .Include(r => r.RoomType)
                .AsQueryable()
                .ToListAsync();


            return await Task.Run(() =>
            {
                rooms.ForEach(r =>
                {
                    r.ResponsibleEmployeesCount = Task.Run(() => this.GetResponsibleForRoomEmployees(r.Id).CountAsync()).Result;

                    r.AvailableEnclosuresAmount = Task.Run(() => this.GetAvailableEnclosuresForRoom(r.Id).CountAsync()).Result;
                });
                return rooms;
            });
        }

        public Task UpdateRoom(Room updatedRoom)
        {
            string sql = "UPDATE dbo.room" +
                " SET room_type_id = @roomType, photo_name = @photoName, unable_to_book = @isUnableToBook" +
                " WHERE id = @roomId";

            SqlParameter idParam = new("roomId", updatedRoom.Id);
            SqlParameter typeParam = new("roomType", updatedRoom.RoomTypeId);
            SqlParameter photoParam = new("photoName", updatedRoom.PhotoPath);
            SqlParameter isUnableToBookParam = new("isUnableToBook", updatedRoom.UnableToBook);

            return _db.Database.ExecuteSqlRawAsync(sql, idParam, typeParam, photoParam, isUnableToBookParam);
        }

        public async Task<short> CreateRoom(Room newRoom)
        {
            await _db.Rooms.AddAsync(newRoom);
            await _db.SaveChangesAsync();
            return newRoom.Id;
        }

        public Task RemoveRoom(short roomId)
        {
            string sql = "DELETE FROM dbo.room" +
                " WHERE id = @roomId";

            SqlParameter idParam = new SqlParameter("roomId", roomId);

            return _db.Database.ExecuteSqlRawAsync(sql, idParam);
        }

        private IQueryable<Employee> GetResponsibleForRoomEmployees(short roomId)
        {
            string sql = "SELECT e.* FROM dbo.employee e" +
                        " INNER JOIN dbo.room_employee re ON re.employee_id = e.id" +
                        " INNER JOIN dbo.room r ON r.id = re.room_id" +
                        " WHERE r.id = @roomId";

            SqlParameter roomParam = new("roomId", roomId);

            return _db.Employees.FromSqlRaw(sql, roomParam).AsQueryable();
        }

        private IQueryable<AnimalEnclosure> GetAvailableEnclosuresForRoom(short roomId)
        {
            string sql = "SELECT ae.* FROM dbo.animal_enclosure ae" +
                " INNER JOIN dbo.room r ON r.id = ae.room_id" +
                " WHERE r.id = @roomId AND ae.id NOT IN " +
                "(" +
                "    SELECT DISTINCT aeInner.id FROM animal_enclosure aeInner" +
                "    LEFT JOIN dbo.booking b ON b.enclosure_id = aeInner.id" +
                "    LEFT JOIN dbo.contract c ON c.enclosure_id = aeInner.id" +
                "    WHERE aeInner.room_id = @roomId AND (b.enclosure_id IS NOT NULL OR (c.id IS NOT NULL AND c.check_out_date IS NULL))" +
                ")";

            SqlParameter roomParam = new("roomId", roomId);

            return _db.Enclosures.FromSqlRaw(sql, roomParam).AsQueryable();
        }

        public Task<bool> IsRoomHasAnyActiveContractsOrBookings(short roomId)
        {
            string sql = "SELECT DISTINCT ae.* FROM dbo.room r" +
                " INNER JOIN dbo.animal_enclosure ae ON ae.room_id = r.id" +
                " LEFT JOIN dbo.booking b ON b.enclosure_id = ae.id" +
                " LEFT JOIN dbo.contract c ON c.enclosure_id = ae.id" +
                " WHERE r.id = @roomId AND (b.enclosure_id IS NOT NULL OR (c.id IS NOT NULL AND c.check_out_date IS NULL))";

            SqlParameter roomParam = new("roomId", roomId);

            return _db.Rooms.FromSqlRaw(sql, roomParam).AnyAsync();
        }

        public Task<List<RoomType>> GetRoomTypes()
        {
            string sql = "SELECT * FROM dbo.room_type rt";

            return _db.RoomTypes.FromSqlRaw(sql).ToListAsync();
        }

        public async Task<Room> GetManagerRoomInfo(short roomId)
        {
            Room room = await _db.Rooms.Where(r => r.Id == roomId)
                .Include(r => r.RoomType)
                .Include(r => r.Employees!)
                .ThenInclude(e => e.LoginInfo)
                .ThenInclude(e => e!.UserType)
                .Include(r => r.Enclosures)
                .AsQueryable()
                .FirstAsync();

            var enclosuresStatuses = await _enclosureService.GetEnclosuresStatus(roomId);

            await SetEnclosuresStatuses(room.Enclosures!, enclosuresStatuses);

            return room;
        }

        public Task<Room> GetRoomBaseInfoById(short roomId)
        {
            string sql = "SELECT * FROM dbo.room r" +
                " WHERE r.id = @roomId";
            SqlParameter roomParam = new("roomId", roomId);

            return _db.Rooms.FromSqlRaw(sql, roomParam)
                .Include(r => r.RoomType)
                .AsQueryable()
                .FirstAsync();
        }

        private Task SetEnclosuresStatuses(List<AnimalEnclosure> enclosures,
             List<(long enclosureId, bool hasActiveBookings, bool hasActiveContracts)> enclosuresStatuses)
        {
            return Task.Run(() =>
            {
                var enclosuresSorted = enclosures.OrderBy(ae => ae.Id).ToList();

                for (int i = 0; i < enclosuresSorted.Count; i++)
                {
                    bool hasContract = enclosuresStatuses[i].hasActiveContracts, hasBooking = enclosuresStatuses[i].hasActiveBookings;

                    enclosuresSorted[i].EnclosureStatus = Enum.Parse<EnclosureStatus>(Math.Max(Convert.ToInt32(hasBooking) * 1,
                        Convert.ToInt32(hasContract) * 2).ToString());
                }
            });
        }

        public Task RemoveNotPreferrableEmployees(short roomId)
        {
            string sql = "DELETE FROM dbo.room_employee" +
                " WHERE room_id = @roomId AND employee_id IN" +
                " (" +
                "   SELECT DISTINCT e.id FROM dbo.employee e" +
                "   INNER JOIN dbo.user_login_info uli ON uli.employee_id = e.id" +
                "   INNER JOIN dbo.user_type ut ON ut.id = uli.user_type_id" +
                "   INNER JOIN dbo.room_employee re ON re.employee_id = e.id" +
                "   INNER JOIN dbo.room r ON r.id = re.room_id" +
                "   INNER JOIN dbo.room_type rt ON r.room_type_id = rt.id" +
                "   WHERE room_id = @roomId AND ut.id <> rt.preferred_user_type_id" +
                " )";

            SqlParameter roomParam = new("roomId", roomId);

            return _db.Database.ExecuteSqlRawAsync(sql, roomParam);
        }
    }
}
