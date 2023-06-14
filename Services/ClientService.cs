using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace Animal_Hotel.Services
{
    public class ClientService : IClientService
    {
        private readonly AnimalHotelDbContext _db;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDbConnectionProvider _connectionProvider;


        public ClientService(AnimalHotelDbContext db, IHttpContextAccessor contextAccessor, IDbConnectionProvider connectionProvider)
        {
            _db = db;
            _contextAccessor = contextAccessor;
            _connectionProvider = connectionProvider;
        }

        public Task<Client?> GetClientById(long? clientId)
        {
            string sql = "SELECT cl.* FROM dbo.client cl" +
                " WHERE cl.id = @clientId";

            SqlParameter clientParam = new("clientId", clientId);
           
            return _db.Clients.FromSqlRaw(sql, clientParam)
                .Include(cl => cl.LoginInfo!)
                .Include(cl => cl.Animals!)
                .ThenInclude(a => a!.AnimalType)
                .FirstOrDefaultAsync();
        }

        public Task<int> GetClientCount()
        {
            return _db.Clients.CountAsync();
        }

        public Task<IQueryable<Client>> GetClientsByPageIndex(int pageIndex, int pageSize)
        {
            string sql = "SELECT * FROM dbo.client c" +
                " ORDER BY c.registered_since DESC" +
                " OFFSET @toSkip ROWS" +
                " FETCH NEXT @pageSize ROWS ONLY";

            SqlParameter skipParam = new("toSkip", pageSize * (pageIndex - 1));
            SqlParameter takeParam = new("pageSize", pageSize);

            return Task.Run(() => _db.Clients.FromSqlRaw(sql, skipParam, takeParam)
                .AsQueryable());
        }

        public async Task<bool> RegisterClient(ClientRegisterModel model)
        {
            //using sql parameters with procedures to prevent SQL injection
            string sql = "EXEC RegisterClient" +
                " @Login = @email," +
                "@Password = @password," +
                "@PhoneNumber = @phone," +
                "@FirstName = @firstname," +
                "@LastName = @lastname," +
                "@PhotoPath = @photopath," +
                "@CardNumber = @cardnumber," +
                "@BirthDate = @birthdate;";

            try
            {
                SqlParameter email = new SqlParameter("email", model.Login);
                SqlParameter password = new SqlParameter("password", model.Password);
                SqlParameter phoneNumber = new SqlParameter("phone", model.PhoneNumber);
                SqlParameter first = new SqlParameter("firstname", model.FirstName);
                SqlParameter last = new SqlParameter("lastname", model.LastName);
                SqlParameter photo = new SqlParameter("photopath", string.IsNullOrEmpty(model.PhotoPath) ? "UnsetClient.png" : model.PhotoPath);
                SqlParameter card = new SqlParameter("cardnumber", string.IsNullOrEmpty(model.CardNumber) ? string.Empty : model.CardNumber);
                SqlParameter birthDate = new SqlParameter("birthdate", model.BirthDate);

                await _db.Database.ExecuteSqlRawAsync(sql, email, password, phoneNumber, first, last, photo, card, birthDate);
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public async Task<List<(int year, int month, int clientsCount)>> RegisteredActiveClients(DateTime startDate, DateTime endDate)
        {
            string connectionString = _connectionProvider.GetConnection(_contextAccessor.HttpContext);
            string sql = "SELECT YEAR(cl.registered_since) AS year, MONTH(cl.registered_since) AS month, COUNT(DISTINCT cl.id) " +
                " FROM dbo.client cl" +
                " INNER JOIN dbo.animal a ON a.owner_id = cl.id" +
                " INNER JOIN dbo.contract c ON c.animal_id = cl.id" +
                " WHERE (cl.registered_since BETWEEN @startDate AND @endDate) AND c.check_out_date IS NOT NULL" +
                " GROUP BY YEAR(cl.registered_since), MONTH(cl.registered_since)" +
                " ORDER BY year, month";


            List<(int year, int month, int clientsCount)> newClients = new();
            using (SqlConnection sqlConnection = new(connectionString))
            {
                SqlCommand command = new(sql, sqlConnection);
                try
                {
                    await sqlConnection.OpenAsync();

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                       int year = reader.GetInt32(0);
                       int month = reader.GetInt32(1);
                       int clientAmount = reader.GetInt32(2);
                        newClients.Add(new(year, month, clientAmount));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Server Error: " + ex.Message);
                }
            }

            return newClients;
        }
    }
}
