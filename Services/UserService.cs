using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using System.Text;
using System.Security.Claims;
#pragma warning disable IDE0090

namespace Animal_Hotel.Services
{
    public class UserService : IUserLoginInfoService
    {
        private readonly AnimalHotelDbContext _db;
        private readonly ClaimHelper _claimHelper;

        public UserService(AnimalHotelDbContext db, ClaimHelper claimHelper)
        {
            _db = db;
            _claimHelper = claimHelper;
        }

        public Task<UserLoginInfo?> GetUserLoginInfoWithRole(string email)
        {
            string sql = "SELECT * FROM dbo.user_login_info uli" +
                " WHERE login = @email";
            SqlParameter param = new SqlParameter("email", email);
            return _db.LoginInfos.FromSqlRaw(sql, param)
                .Include(l => l.UserType)
                .SingleOrDefaultAsync();
        }

        public Task<UserLoginInfo?> GetLoginWithRoleAndPersonalInfo(string email)
        {
            string sql = "SELECT * FROM dbo.user_login_info uli" +
                " WHERE login = @email";
            SqlParameter param = new SqlParameter("email", email);
            return _db.LoginInfos.FromSqlRaw(sql, param)
                .Include(l => l.UserType)
                .Include(l => l.Employee)
                .Include(l => l.Client)
                .SingleOrDefaultAsync();
        }

        public async Task<ClientDataViewModel?> GetClientDataById(long loginId)
        {
            string sql = "SELECT * FROM dbo.client_info ci" +
                " WHERE ci.user_id = @id";

            SqlParameter idParam = new SqlParameter("id", loginId);

            var client = await _db.ClientViewModels.FromSqlRaw(sql, idParam)
                .Include(uli => uli.UserType)
                .FirstOrDefaultAsync();

            if (client != null)
            {
                client.UserPassword = new StringBuilder().GetString(client!.Password!);
            }

            return client;
        }

        public Task<string?> GetUserPhotoPathById(long userId)
        {
            string sql = "SELECT * FROM dbo.user_login_info uli" +
                " WHERE uli.id = @userId";
            SqlParameter idParam = new SqlParameter("userId", userId);

            var loginInfos = _db.LoginInfos.FromSqlRaw(sql, idParam);
            bool isClient = _claimHelper.HasClaimWithValue("Client", ClaimTypes.Role);
            if (isClient)
            {
                loginInfos.Include(uli => uli.Client);
            }
            else
            {
                loginInfos.Include(uli => uli.Employee);
            }

            return loginInfos.Select(uli => isClient ? uli.Client!.PhotoPath : uli.Employee!.PhotoPath).FirstAsync();
        }

        public async Task<EmployeeDataViewModel?> GetEmployeeDataById(long loginId)
        {
            string sql = "SELECT * FROM dbo.employee_info ei" +
                " WHERE ei.user_id = @id";

            SqlParameter idParam = new SqlParameter("id", loginId);

            var employee = await _db.EmployeeViewModels.FromSqlRaw(sql, idParam)
                .Include(uli => uli.UserType)
                .FirstOrDefaultAsync();

            if (employee != null)
            {
                employee.UserPassword = new StringBuilder().GetString(employee!.Password!);
            }

            return employee;
        }

        public Task<IQueryable<UserLoginInfo>> GetUserLoginsWithTypes()
        {
            string sql = "SELECT * FROM dbo.user_login_info uli";

            return Task.Run(() => _db.Database.SqlQueryRaw<UserLoginInfo>(sql)
            .Include(l => l.UserType)
            .AsQueryable());
        }

        public async Task<bool> UpdateClient(ClientDataViewModel clientViewModel)
        {
            string sql = "EXEC UpdateClient" +
                " @UserId = @userId," +
                "@Login = @email," +
                "@PhoneNumber = @phone," +
                "@ClientId = @clientId," +
                "@FirstName = @firstName," +
                "@LastName = @lastName," +
                "@PhotoPath = @photoPath," +
                "@CardNumber = @cardNumber," +
                "@BirthDate = @birthDate;";

            try
            {
                SqlParameter idParam = new("userId", clientViewModel.UserId);
                SqlParameter loginParam = new("email", clientViewModel.Login);
                SqlParameter phoneParam = new SqlParameter("phone", clientViewModel.PhoneNumber);
                SqlParameter clientIdParam = new("clientId", clientViewModel.SubUserId);
                SqlParameter firstNameParam = new("firstName", clientViewModel.FirstName);
                SqlParameter lastNameParam = new("lastName", clientViewModel.LastName);
                SqlParameter photoPathParam = new("photoPath", string.IsNullOrEmpty(clientViewModel.PhotoPath) ? null : 
                    clientViewModel.PhotoPath);
                SqlParameter cardNumberParam = new("cardNumber", string.IsNullOrEmpty(clientViewModel.CardNumber) ? null :
                    clientViewModel.CardNumber);
                SqlParameter birthDateParam = new("birthDate", clientViewModel.BirthDate);

                await _db.Database.ExecuteSqlRawAsync(sql, idParam, loginParam, phoneParam, clientIdParam, firstNameParam,
                    lastNameParam, photoPathParam, cardNumberParam, birthDateParam);
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public async Task<bool> UpdateEmployeePersonalData(EmployeeDataViewModel employeeViewModel)
        {
            string sql = "EXEC UpdateEmployeePersonalData" +
                " @UserId = @userid," +
                "@Login = @email," +
                "@PhoneNumber = @phonenumber," +
                "@EmployeeId = @employeeid," +
                "@FirstName = @firstname," +
                "@LastName = @lastname," +
                "@Sex = @sex," +
                "@PhotoPath =  @photopath," +
                "@BirthDate =  @birthdate;";
            try
            {
                SqlParameter idParam = new("userid", employeeViewModel.UserId);
                SqlParameter loginParam = new("email", employeeViewModel.Login);
                SqlParameter phoneParam = new SqlParameter("phonenumber", employeeViewModel.PhoneNumber);
                SqlParameter employeeIdParam = new("employeeid", employeeViewModel.SubUserId);
                SqlParameter firstNameParam = new("firstname", employeeViewModel.FirstName);
                SqlParameter lastNameParam = new("lastname", employeeViewModel.LastName);
                SqlParameter sexParam = new("sex", employeeViewModel.Sex);
                SqlParameter photoPathParam = new("photopath", employeeViewModel.PhotoPath);
                SqlParameter birthDateParam = new("birthdate", employeeViewModel.BirthDate);

                await _db.Database.ExecuteSqlRawAsync(sql, idParam, loginParam, phoneParam, employeeIdParam, firstNameParam, 
                    lastNameParam, sexParam, photoPathParam, birthDateParam);
                return true;
            }
            catch(SqlException)
            {
                return false;
            }
        }

        public async Task UpdatePasswordById(long userId, string password)
        {
            string sql = "UPDATE dbo.user_login_info" +
                " SET password = CONVERT(VARBINARY, @password)" +
                "WHERE id = @userId";

            SqlParameter passwordParam = new SqlParameter("password", System.Data.SqlDbType.VarChar);
            passwordParam.Value = password;
            SqlParameter idParam = new SqlParameter("userId", userId);

            await _db.Database.ExecuteSqlRawAsync(sql, passwordParam, idParam);
        }

        public async Task<(bool isExists, string errorMessage)> IsUserWithPhoneAndEmailExists(string email, string phone, long? requester = null)
        {
            (bool IsExists, string errorMessage) result = (false, string.Empty);
            string sql = "SELECT * FROM dbo.user_login_info uli" +
                " WHERE uli.id <> @requester AND (uli.login = @email OR uli.phone_number = @phoneNumber)";

            SqlParameter emailParam = new("email", email);
            SqlParameter phoneParam = new("phoneNumber", phone);
            SqlParameter reqParam = new("requester", requester);

            if (await _db.LoginInfos.FromSqlRaw(sql, emailParam, phoneParam, reqParam).AsQueryable().AnyAsync())
            {
                result = (true, "User with such email or phone number already exists");
            }

            return result;
        }
    }
}
