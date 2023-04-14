using Animal_Hotel.Models.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Animal_Hotel.Services
{
    public class UserRegisterService : IUserRegisterService
    {

        private readonly AnimalHotelDbContext _db;
        public UserRegisterService(AnimalHotelDbContext db) 
        {
            _db = db;
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

        public async Task<bool> RegisterEmployee(EmployeeRegisterModel model)
        {
            string sql = "EXEC RegisterEmployee" +
                " @Login = @login," +
                "@Password = @password," +
                "@PhoneNumber = @phone," +
                "@EmployeeTypeId = @employeeType," +
                "@FirstName = @firstname," +
                "@LastName = @lastname," +
                "@Salary = @salary," +
                "@Sex = @sex," +
                "@HiredSince = @hiredSince," +
                "@PhotoPath = @photo," +
                "@BirthDate = @birthdate;";

            try
            {
                SqlParameter email = new SqlParameter("email", model.Login);
                SqlParameter password = new SqlParameter("password", model.Password);
                SqlParameter phoneNumber = new SqlParameter("phone", model.PhoneNumber);
                SqlParameter employeeType = new SqlParameter("employeeType", model.EmployeeTypeId);
                SqlParameter first = new SqlParameter("firstname", model.FirstName);
                SqlParameter last = new SqlParameter("lastname", model.LastName);
                SqlParameter salary = new SqlParameter("salary", model.Salary);
                SqlParameter sex = new SqlParameter("sex", model.Sex);
                SqlParameter hiredSince = new SqlParameter("hiredSince", model.HiredSince);
                SqlParameter photo = new SqlParameter("photopath", model.PhotoPath);
                SqlParameter birthDate = new SqlParameter("birthdate", model.BirthDate);


                await _db.Database.ExecuteSqlRawAsync(sql, email, password, phoneNumber, employeeType, first,
                    last, salary, sex, hiredSince, photo, birthDate);
                return true;

            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
