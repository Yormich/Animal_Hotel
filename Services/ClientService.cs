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

        public ClientService(AnimalHotelDbContext db)
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
    }
}
