using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Animal_Hotel.Services
{
    public class EnclosureService : IEnclosureService
    {
        private readonly AnimalHotelDbContext _db;
        private readonly IDbConnectionProvider _connectionProvider; 
        private readonly IHttpContextAccessor _contextAccessor;

        public EnclosureService(AnimalHotelDbContext db, IDbConnectionProvider dbConnectionProvider, IHttpContextAccessor contextAccessor)
        {
            _db = db;
            _connectionProvider = dbConnectionProvider;
            _contextAccessor = contextAccessor;
        }

        public Task CreateEnclosure(AnimalEnclosure newEnclosure)
        {
            string sql = "INSERT INTO dbo.animal_enclosure(max_animals, animal_type_id, room_id, enclosure_type_id)" +
                " VALUES(@maxAnimals, @animalType, @roomId, @enclosureType)";

            SqlParameter animalsParam = new("maxAnimals", newEnclosure.MaxAnimals);
            SqlParameter animalTypeParam = new("animalType", newEnclosure.AnimalTypeId);
            SqlParameter enclosureTypeParam = new("enclosureType", newEnclosure.EnclosureTypeId);
            SqlParameter roomParam = new("roomId", newEnclosure.RoomId);

            return _db.Database.ExecuteSqlRawAsync(sql, animalsParam, animalTypeParam, enclosureTypeParam, roomParam);
        }

        public Task DeleteEnclosure(long enclosureId)
        {
            string sql = "DELETE FROM dbo.animal_enclosure" +
                " WHERE id = @enclosureId";

            SqlParameter enclosureParam = new("enclosureId", enclosureId);

            return _db.Database.ExecuteSqlRawAsync(sql, enclosureParam);
        }

        public Task<AnimalEnclosure?> GetEnclosureById(long? enclosureId)
        {
            string sql = "SELECT * FROM dbo.animal_enclosure ae" +
                " WHERE ae.id = @enclosureId";
            SqlParameter enclosureParam = new("enclosureId", enclosureId);

            return _db.Enclosures.FromSqlRaw(sql, enclosureParam)
                .Include(ae => ae.EnclosureType)
                .Include(ae => ae.AnimalType)
                .AsQueryable()
                .FirstOrDefaultAsync();
        }

        public async Task<List<(long enclosureId, bool hasActiveBookings, bool hasActiveContracts)>> GetEnclosuresStatus(short roomId)
        {
            string connectionString = _connectionProvider.GetConnection(_contextAccessor.HttpContext);
            string query = "SELECT ae.id," +
                " (" +
                "   SELECT CONVERT(BIT, COUNT(bInner.animal_id)) FROM dbo.booking bInner" +
                "   WHERE bInner.enclosure_id = ae.id" +
                " ) AS has_bookings, " +
                " (" +
                "   SELECT CONVERT(BIT, COUNT(cInner.id)) FROM dbo.contract cInner" +
                "   WHERE cInner.enclosure_id = ae.id AND cInner.check_out_date IS NULL" +
                " ) AS has_active_contracts" +
                " FROM dbo.animal_enclosure ae" +
                " WHERE ae.room_id = @roomId" +
                " GROUP BY ae.id;";

            List<(long enclosureId, bool hasActiveBookings, bool hasActiveContracts)> enclosureStatuses = new();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand command = new(query, sqlConnection);

                command.Parameters.AddWithValue("@roomId", roomId);
                try
                {
                    await sqlConnection.OpenAsync();

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        long enclosureId = reader.GetInt64(0);
                        bool hasBookings = reader.GetBoolean(1);
                        bool hasContracts = reader.GetBoolean(2);
                        enclosureStatuses.Add(new(enclosureId, hasBookings, hasContracts));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Server Error: " + ex.Message);
                }
            }
            return enclosureStatuses.OrderBy(s => s.enclosureId).ToList();
        }

        public Task<List<EnclosureType>> GetEnclosureTypes()
        {
            string sql = "SELECT * FROM dbo.enclosure_type";

            return _db.EnclosureTypes.FromSqlRaw(sql).AsQueryable().ToListAsync();
        }

        public Task<bool> IsEnclosureHasActiveContractOrBooking(long enclosureId)
        {
            string sql = "SELECT DISTINCT ae.* FROM dbo.animal_enclosure ae" +
                " LEFT JOIN dbo.contract c ON c.enclosure_id = ae.id" +
                " LEFT JOIN dbo.booking b ON b.enclosure_id = ae.id" +
                " WHERE ae.id = @enclosureId AND (b.enclosure_id IS NOT NULL OR (c.id IS NOT NULL AND c.check_out_date IS NULL))";

            SqlParameter enclosureParam = new("enclosureId", enclosureId);

            return _db.Enclosures.FromSqlRaw(sql, enclosureParam).AnyAsync();
        }

        public Task UpdateEnclosure(AnimalEnclosure enclosure)
        {
            string sql = "UPDATE dbo.animal_enclosure" +
                " SET max_animals = @maxAnimalsId, animal_type_id = @animalTypeId, enclosure_type_id = @enclosureType" +
                " WHERE id = @enclosureId";

            SqlParameter idParam = new("enclosureId", enclosure.Id);
            SqlParameter enclosureParam = new("enclosureType", enclosure.EnclosureTypeId);
            SqlParameter animalTypeParam = new("animalTypeId", enclosure.AnimalTypeId);
            SqlParameter maxAnimalsId = new("maxAnimalsId", enclosure.MaxAnimals);

            return _db.Database.ExecuteSqlRawAsync(sql, idParam, enclosureParam, animalTypeParam, maxAnimalsId);
        }
    }
}
