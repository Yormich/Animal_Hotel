using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels;
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

        public async Task<(bool success, string? message)> DeleteEnclosure(long enclosureId)
        {
            string sql = "EXEC dbo.DeleteEnclosure" +
                "   @enclosure_id = @enclosureId;";

            SqlParameter enclosureParam = new("enclosureId", enclosureId);

            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, enclosureParam);
            }
            catch(SqlException se)
            {
                Console.WriteLine(se.Message);
                return new(false, se.Message);
            }

            return new(true, string.Empty);
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

        public async Task<List<EnclosureStatusViewModel>> GetEnclosuresStatus(short roomId)
        {
            string connectionString = _connectionProvider.GetConnection(_contextAccessor.HttpContext);
            string query = " SELECT es.* FROM dbo.enclosures_statuses es" +
                " INNER JOIN dbo.animal_enclosure ae ON ae.id = es.id" +
                " WHERE ae.room_id = @roomId";

            List<EnclosureStatusViewModel> enclosureStatuses = new();
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
                        enclosureStatuses.Add(new() {
                            Id = enclosureId, 
                            HasBookings = hasBookings,
                            HasActiveContracts = hasContracts 
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Server Error: " + ex.Message);
                }
            }
            return enclosureStatuses.OrderBy(s => s.Id).ToList();
        }

        public Task<List<EnclosureType>> GetEnclosureTypes()
        {
            string sql = "SELECT * FROM dbo.enclosure_type";

            return _db.EnclosureTypes.FromSqlRaw(sql).AsQueryable().ToListAsync();
        }

        public Task<List<AnimalEnclosure>> GetSuitableEnclosures(long animalId)
        {
            string sql = "SELECT ae.* FROM dbo.animal_enclosure ae" +
                " WHERE ae.id NOT IN " +
                " (" +
                "   SELECT aeInner.id FROM dbo.occupied_enclosures aeInner" +
                "   LEFT JOIN dbo.booking b ON b.enclosure_id = aeInner.id" +
                "   WHERE b.animal_id IS NULL OR b.animal_id IS NOT NULL AND b.animal_id <> @animalId" +
                " ) " +
                " AND ae.animal_type_id = " +
                " (" +
                "   SELECT at.id FROM dbo.animal_type at" +
                "   INNER JOIN dbo.animal a ON a.type_id = at.id" +
                "   WHERE a.id = @animalId)";

            SqlParameter animalParam = new("animalId", animalId);

            return _db.Enclosures.FromSqlRaw(sql, animalParam)
                .Include(e => e.EnclosureType)
                .ToListAsync();
        }

        public async Task<(bool success, string? message)> UpdateEnclosure(AnimalEnclosure enclosure)
        {
            string sql = "EXEC dbo.UpdateEnclosure" +
                "   @enclosure_id = @enclosureId," +
                "   @max_animals = @maxAnimals," +
                "   @animal_type_id = @animalTypeId," +
                "   @enclosure_type_id = @enclosureType";

            SqlParameter idParam = new("enclosureId", enclosure.Id);
            SqlParameter enclosureParam = new("enclosureType", enclosure.EnclosureTypeId);
            SqlParameter animalTypeParam = new("animalTypeId", enclosure.AnimalTypeId);
            SqlParameter maxAnimalsId = new("maxAnimals", enclosure.MaxAnimals);

            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, idParam, enclosureParam, animalTypeParam, maxAnimalsId);
            }
            catch(SqlException se)
            {
                Console.WriteLine(se.Message);
                return new(false, se.Message);
            }

            return new(true, string.Empty);
        }
    }
}
