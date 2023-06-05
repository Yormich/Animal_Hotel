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

        public Task<AnimalEnclosure?> GetEnclosureById(long? enclosureId)
        {
            string sql = "SELECT * FROM dbo.animal_enclosure ae" +
                " WHERE ae.id = @enclosureId";
            SqlParameter enclosureParam = new("enclosureId", enclosureId);

            return _db.Enclosures.FromSqlRaw(sql, enclosureParam)
                .Include(ae => ae.EnclosureType)
                .Include(ae => ae.AnimalType)
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
    }
}
