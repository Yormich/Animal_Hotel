using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Animal_Hotel.Services
{
    public class BookingService : IBookingService
    {
        private readonly AnimalHotelDbContext _db;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly IAnimalService _animalService;
        private readonly IEnclosureService _enclosureService;

        public BookingService(AnimalHotelDbContext db, IHttpContextAccessor contextAccessor, IDbConnectionProvider connectionProvider,
            IAnimalService animalService, IEnclosureService enclosureService)
        {
            _db = db;
            _contextAccessor = contextAccessor;
            _connectionProvider = connectionProvider;
            _animalService = animalService;
            _enclosureService = enclosureService;
        }

        public async Task<(bool success, string? message)> CreateBooking(Booking newBooking)
        {
            string sql = "EXEC dbo.CreateBooking" +
                " @animal_id = @animalId," +
                " @start_date = @startDate," +
                " @date_of_leaving =  @dateOfLeaving," +
                " @enclosure_id = @enclosureId;";

            SqlParameter animalParam = new("animalId", newBooking.AnimalId);
            SqlParameter startDateParam = new("startDate", newBooking.StartDate);
            SqlParameter endDateParam = new("dateOfLeaving", newBooking.EndDate);
            SqlParameter enclosureParam = new("enclosureId", newBooking.EnclosureId);

            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, animalParam, startDateParam, endDateParam, enclosureParam);
            }
            catch(SqlException se)
            {
                Console.WriteLine(se.Message);
                return new(false, se.Message);
            }
            return new(true, string.Empty);
        }

        public Task DeleteBooking(long animalBookedId)
        {
            string sql = "DELETE FROM dbo.booking" +
                " WHERE animal_id = @animalId";

            SqlParameter animalParam = new("animalId", animalBookedId);

            return _db.Database.ExecuteSqlRawAsync(sql, animalParam);
        }

        public Task<Booking?> GetBookingById(long? animalId)
        {
            string sql = "SELECT b.* FROM dbo.booking b" +
                " WHERE animal_id = @animalId";

            SqlParameter animalParam = new("animalId", animalId);

            return _db.Bookings.FromSqlRaw(sql, animalParam)
                .Include(b => b.Animal)
                .ThenInclude(a => a!.Owner)
                .Include(b => b.Enclosure)
                .ThenInclude(e => e!.Room)
                .Include(b => b.Animal)
                .ThenInclude(a => a!.Owner)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ReceptionistReport>> GetBookingsByDate(DateTime targetDate)
        {
            string connectionString = _connectionProvider.GetConnection(_contextAccessor.HttpContext);
            string sql = "SELECT" +
                " (" +
                "   SELECT COUNT(bInner.animal_id) FROM dbo.booking bInner" +
                "   WHERE bInner.start_date <= @targetDate" +
                " ) AS bookings_amount, b.start_date, b.date_of_leaving, " +
                " DATEDIFF(DAY, b.start_date, b.date_of_leaving) AS rent_period_in_days," +
                " CONCAT(cl.first_name, ' ' , cl.last_name) AS client_name, uli.phone_number, a.id AS animal_id, b.enclosure_id FROM dbo.booking b" +
                " INNER JOIN dbo.animal a ON b.animal_id = a.id" +
                " INNER JOIN dbo.client cl ON a.owner_id = cl.id" +
                " INNER JOIN dbo.user_login_info uli ON uli.client_id = cl.id" +
                " WHERE b.start_date <= @targetDate";


            List<ReceptionistReport> reports = new();
            using (SqlConnection sqlConnection = new(connectionString))
            {
                SqlCommand command = new(sql, sqlConnection);
                command.Parameters.AddWithValue("@targetDate", targetDate);
                try
                {
                    await sqlConnection.OpenAsync();

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        int bookingsAmount = reader.GetInt32(0);
                        DateTime startDate = reader.GetDateTime(1);
                        DateTime endDate = reader.GetDateTime(2);
                        int rentPeriod = reader.GetInt32(3);
                        string clientName = reader.GetString(4);
                        string clientPhone = reader.GetString(5);
                        long animalId = reader.GetInt64(6);
                        long enclosureId = reader.GetInt64(7);

                        reports.Add(new ReceptionistReport()
                        {
                            Amount = bookingsAmount,
                            StartDate = startDate,
                            EndDate = endDate,
                            Period = rentPeriod,
                            ClientName = clientName,
                            ClientPhone = clientPhone,
                            AnimalId = animalId,
                            Animal = await _animalService.GetAnimalById(animalId),
                            Enclosure = await _enclosureService.GetEnclosureById(enclosureId),
                            EnclosureId = enclosureId,
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Server Error: " + ex.Message);
                }
            }

            return reports;
        }

        public Task<IQueryable<Booking>> GetClientBookingsByPage(long clientId, int pageIndex, int pageSize)
        {
            string sql = "SELECT b.* FROM dbo.booking b" +
                " INNER JOIN dbo.animal a ON a.id = b.animal_id" +
                " INNER JOIN dbo.client cl ON cl.id = a.owner_id" +
                " WHERE cl.id = @clientId" +
                " ORDER BY b.animal_id" +
                " OFFSET @toSkip ROWS" +
                " FETCH NEXT @pageSize ROWS ONLY";

            SqlParameter clientParam = new("clientId", clientId);
            SqlParameter skipParam = new("toSkip", pageSize * (pageIndex - 1));
            SqlParameter toTakeParam = new("pageSize", pageSize);

            return Task.Run(() => _db.Bookings
                                    .FromSqlRaw(sql, clientParam, skipParam, toTakeParam)
                                    .Include(b => b.Enclosure)
                                    .ThenInclude(e => e!.Room)
                                    .Include(b => b.Animal)
                                    .ThenInclude(a => a!.AnimalType)
                                    .AsQueryable());
        }

        public Task<int> GetClientBookingsCount(long clientId)
        {
            string sql = "SELECT b.* FROM dbo.booking b" +
                " INNER JOIN dbo.animal a ON a.id = b.animal_id" +
                " INNER JOIN dbo.client cl ON cl.id = a.owner_id" +
                " WHERE cl.id = @clientId";

            SqlParameter clientParam = new("clientId", clientId);

            return _db.Bookings.FromSqlRaw(sql, clientParam).CountAsync();
        }
    }
}
