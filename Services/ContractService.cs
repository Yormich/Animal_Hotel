using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Animal_Hotel.Services
{
    public class ContractService : IContractService
    {
        private readonly AnimalHotelDbContext _db;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDbConnectionProvider _connectionProvider;

        public ContractService(AnimalHotelDbContext context, IHttpContextAccessor contextAccessor, IDbConnectionProvider connectionProvider)
        {
            _db = context;
            _contextAccessor = contextAccessor;
            _connectionProvider = connectionProvider;
        }

        public async Task<(bool success, string? message)> CreateContract(Contract newContract)
        {
            string sql = "EXEC dbo.CreateContract" +
                " @start_date = @startDate," +
                " @date_of_leaving = @leavingDate," +
                " @animal_id = @animalId," +
                " @enclosure_id = @enclosureId;";

            SqlParameter sDateParam = new("startDate", newContract.StartDate);
            SqlParameter eDateParam = new("leavingDate", newContract.EndDate);
            SqlParameter animalParam = new("animalId", newContract.AnimalId);
            SqlParameter enclosureParam = new("enclosureId", newContract.EnclosureId);

            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, sDateParam, eDateParam, animalParam, enclosureParam);
            }
            catch (SqlException se)
            {
                Console.WriteLine(se.Message);
                return new(false, se.Message);
            }
            return new(true, string.Empty);
        }

        public async Task<(bool success, string? message)> DeleteContract(long contractId)
        {
            string sql = "EXEC dbo.DeleteContract" +
                "   @contract_id = @contractId;";

            SqlParameter contractParam = new("contractId", contractId);
            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, contractParam);
            }
            catch(SqlException se)
            {
                Console.WriteLine(se.Message);
                return new(false, se.Message);
            }
            return new(true, string.Empty);
        }

        public Task UpdateContract(Contract updatedContract)
        {
            string sql = "UPDATE dbo.contract" +
                " SET check_out_date = @checkoutDate, actually_paid = @actuallyPaid" +
                " WHERE id = @contractId";

            SqlParameter contractParam = new("contractId", updatedContract);
            SqlParameter checkoutDateParam = new("checkoutDate", updatedContract.CheckOutDate);
            SqlParameter paidParam = new("actuallyPaid", updatedContract.ActuallyPaid);

            return _db.Database.ExecuteSqlRawAsync(sql, contractParam, checkoutDateParam, paidParam);
        }

        public Task<bool> DoesClientHasFinishedContract(long clientId)
        {
            string sql = "SELECT DISTINCT cl.* FROM dbo.client cl" +
                " INNER JOIN dbo.animal a ON a.owner_id = cl.id" +
                " INNER JOIN dbo.contract c ON c.animal_id = a.id" +
                " WHERE cl.id = @clientId AND c.check_out_date IS NOT NULL";

            SqlParameter clientParam = new("clientId", clientId);

            return _db.Clients.FromSqlRaw(sql, clientParam)
                .AsQueryable()
                .AnyAsync();
        }

        public async Task<List<(int year, int month, string type, int count)>> GetAnimalsContractsByPeriod(DateTime startDate, DateTime endDate)
        {
            string connectionString = _connectionProvider.GetConnection(_contextAccessor.HttpContext);
            string sql = "SELECT YEAR(c.start_date) year, MONTH(c.start_date) AS month, " +
                " at.name AS animal_type, COUNT(DISTINCT c.animal_id) AS animals_count" +
                " FROM dbo.contract c" +
                " INNER JOIN dbo.animal a ON a.id = c.animal_id" +
                " INNER JOIN dbo.animal_type at ON at.id = a.type_id" +
                " WHERE (c.start_date BETWEEN @startDate AND @endDate)" +
                " GROUP BY YEAR(c.start_date), MONTH(c.start_date), at.name" +
                " ORDER BY year, month";

            List<(int year, int month, string type, int count)> animalsContracts= new();
            using(SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand command = new(sql, sqlConnection);

                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);
                try
                {
                    await sqlConnection.OpenAsync();
                    
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        int year = reader.GetInt32(0);
                        int month = reader.GetInt32(1);
                        string type = reader.GetString(2);
                        int count = reader.GetInt32(3);
                        animalsContracts.Add(new(year, month, type, count));
                    }
                }
                catch(SqlException se)
                {
                    Console.WriteLine("Server Error: " + se.Message);
                }
            }

            return animalsContracts;
        }

        public async Task<List<(int year, int month, decimal monthIncome)>> GetIncomeByPeriod(DateTime startDate, DateTime endDate)
        {
            string connectionString = _connectionProvider.GetConnection(_contextAccessor.HttpContext);
            string sql = "SELECT YEAR(c.check_out_date) AS contract_year, MONTH(c.check_out_date) AS contract_month, " +
                " SUM(c.actually_paid) AS month_income " +
                " FROM dbo.contract c" +
                " WHERE c.check_out_date IS NOT NULL AND (c.check_out_date BETWEEN @startDate AND '@endDate)" +
                " GROUP BY YEAR(c.check_out_date), MONTH(c.check_out_date)" +
                " ORDER BY contract_year, contract_month";

            List<(int year, int month, decimal monthIncome)> contractsIncome = new();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand command = new(sql, sqlConnection);

                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);
                try
                {
                    await sqlConnection.OpenAsync();

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        int year = reader.GetInt32(0);
                        int month = reader.GetInt32(1);
                        decimal income = reader.GetDecimal(2);
                        contractsIncome.Add(new(year, month, income));
                    }
                }
                catch (SqlException se)
                {
                    Console.WriteLine("Server Error: " + se.Message);
                }
            }

            return contractsIncome;
        }

        public async Task<List<ReceptionistReport>> GetContractsByDate(DateTime targetDate)
        {
            string connectionString = _connectionProvider.GetConnection(_contextAccessor.HttpContext);
            string sql = "SELECT" +
                " (" +
                "   SELECT COUNT (cInner.id) FROM dbo.contract cInner " +
                "   WHERE cInner.date_of_leaving <= @targetDate AND cInner.check_out_date IS NULL" +
                " ) AS contracts_amount, c.start_date, DATEDIFF(DAY, c.start_date, c.date_of_leaving) AS rent_period_in_days," +
                " a.*, CONCAT(cl.first_name, ' ' , cl.last_name) AS client_name, uli.phone_number, c.id" +
                " FROM dbo.contract c " +
                " INNER JOIN dbo.animal a ON a.id = c.animal_id" +
                " INNER JOIN dbo.animal_type at ON at.id = a.type_id" +
                " INNER JOIN dbo.client cl ON cl.id = a.owner_id" +
                " INNER JOIN dbo.user_login_info uli ON uli.client_id = cl.id" +
                " WHERE c.date_of_leaving <= @targetDate AND c.check_out_date IS NULL";


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
                        reports.Add(new ReceptionistReport()
                        {
                            Amount = reader.GetInt32(0),
                            StartDate = reader.GetDateTime(1),
                            Period = reader.GetInt32(2),
                            ClientName = reader.GetString(12),
                            ClientPhone = reader.GetString(13),
                            AnimalId = reader.GetInt64(3),
                            Animal = new()
                            {
                                Id = reader.GetInt64(3),
                                Name = reader.GetString(4),
                                Age = reader.GetInt16(5),
                                Sex = reader.GetString(6)[0],
                                Weight = reader.GetDouble(7),
                                Preferences = reader.GetString(8),
                                OwnerId = reader.GetInt64(10),
                                TypeId = reader.GetInt16(11),
                                AnimalType = new()
                                {
                                    Id = reader.GetInt16(11),
                                }
                            },
                            ContractId = reader.GetInt64(14),
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

        public Task<Contract?> GetContractById(long contractId)
        {
            string sql = "SELECT c.* FROM dbo.contract c" +
                " WHERE c.id = @contractId";

            SqlParameter contractParam = new("contractId", contractId);

            return _db.Contracts.FromSqlRaw(sql, contractParam)
                .FirstOrDefaultAsync();
        }
    }
}
