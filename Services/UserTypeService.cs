using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Animal_Hotel.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly AnimalHotelDbContext _db;

        public UserTypeService(AnimalHotelDbContext db)
        {
            _db = db;
        }

        public Task<UserType> GetUserTypeByUserId(long userId)
        {
            string sql = "SELECT ut.id, ut.name FROM dbo.user_login_info uli" +
                " INNER JOIN dbo.user_type ut ON uli.user_type_id = ut.id" +
                " WHERE uli.id = @userId";
            SqlParameter idParam = new SqlParameter("userId", userId);

            return _db.UserTypes.FromSqlRaw(sql, idParam).FirstAsync();
        }
    }
}
