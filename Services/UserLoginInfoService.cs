using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Cryptography;

namespace Animal_Hotel.Services
{
    public class UserLoginInfoService : IUserLoginInfoService
    {
        private readonly AnimalHotelDbContext _db;

        public UserLoginInfoService(AnimalHotelDbContext db)
        {
            _db = db;
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

        //TODO: maybe rework with already created views
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

        public Task<IQueryable<UserLoginInfo>> GetUserLoginsWithTypes()
        {
            string sql = "SELECT * FROM dbo.user_login_info uli";

            return Task.Run(() => _db.Database.SqlQueryRaw<UserLoginInfo>(sql)
            .Include(l => l.UserType)
            .AsQueryable());
        }

        
    }
}
