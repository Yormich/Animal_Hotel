using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Animal_Hotel.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AnimalHotelDbContext _db;

        public EmployeeService(AnimalHotelDbContext db)
        {
            _db = db;
        }

        public Task<EmployeeDataViewModel?> GetEmployeeById(long? employeeId)
        {
            string sql = "SELECT * FROM dbo.employee e" +
               " WHERE e.id = @employeeId";

            SqlParameter idParam = new("employeeId", employeeId);

            return _db.Employees.FromSqlRaw(sql, idParam)
                .Include(e => e.LoginInfo)
                .ThenInclude(uli => uli!.UserType)
                .Select(e => new EmployeeDataViewModel()
                {
                    UserId = e.LoginInfo!.Id,
                    SubUserId = e.Id,
                    Login = e.LoginInfo!.Email,
                    PhoneNumber = e.LoginInfo!.PhoneNumber,
                    UserType = e.LoginInfo!.UserType,
                    BirthDate = e.BirthDate,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Salary = e.Salary,
                    PhotoPath = e.PhotoPath,
                    Sex = e.Sex,
                    HiredSince = e.HiredSince,
                })
                .AsQueryable()
                .FirstOrDefaultAsync();
        }

        public Task DeleteEmployee(long employeeId)
        {
            string sql = "DELETE FROM dbo.employee" +
                " WHERE id = @employeeId";

            SqlParameter idParam = new("employeeId", employeeId);

            return _db.Database.ExecuteSqlRawAsync(sql, idParam);
        }

        public Task<IQueryable<EmployeeDataViewModel>> GetEmployees(int pageIndex, int pageSize, bool excludeManagers = true)
        {
            string sql = "SELECT e.* FROM dbo.employee e " +
                $"{(excludeManagers ? "INNER JOIN dbo.user_login_info uli ON uli.employee_id = e.id WHERE uli.user_type_id != 4" 
                    : string.Empty)}" + 
                " ORDER BY e.id" +
                " OFFSET @skipAmount ROWS" +
                " FETCH NEXT @toTake ROWS ONLY";

            SqlParameter skipParam = new("skipAmount", pageSize * (pageIndex - 1));
            SqlParameter toTakeParam = new("toTake", pageSize);

            return Task.Run(() => _db.Employees.FromSqlRaw(sql, skipParam, toTakeParam)
                .Include(e => e.LoginInfo)
                .ThenInclude(uli => uli!.UserType)
                .Select(e => new EmployeeDataViewModel()
                {
                    UserId = e.LoginInfo!.Id,
                    SubUserId = e.Id,
                    Login = e.LoginInfo!.Email,
                    PhoneNumber = e.LoginInfo!.PhoneNumber,
                    UserType = e.LoginInfo!.UserType,
                    BirthDate = e.BirthDate,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Salary = e.Salary,
                    PhotoPath = e.PhotoPath,
                    Sex = e.Sex,
                    HiredSince = e.HiredSince,
                })
                .AsQueryable());
        }

        public Task<int> GetEmployeesCount(bool excludeManagers = true)
        {
            string sql = "SELECT e.* FROM dbo.employee e ";
            string managersExclude = " INNER JOIN dbo.user_login_info uli ON uli.employee_id = e.id" +
                " WHERE uli.user_type_id <> 4";
            sql = excludeManagers ? $"{sql}{managersExclude}" : sql;

            return _db.Employees.FromSqlRaw(sql).AsQueryable().CountAsync();
        }
        public async Task<bool> UpdateEmployeeByManager(EmployeeDataViewModel employee)
        {
            string sql = "EXEC dbo.UpdateEmployeeByManager" +
                "   @LoginId = @uliId," +
                "   @EmployeeId = @employeeId," +
                "   @Salary = @salary," +
                "   @EmployeeTypeId = @userTypeId;";

            try
            {
                SqlParameter uliParam = new("uliId", employee.UserId);
                SqlParameter employeeParam = new("employeeId", employee.SubUserId);
                SqlParameter salaryParam = new("salary", employee.Salary);
                SqlParameter employeeTypeParam = new("userTypeId", employee.Position);

                await _db.Database.ExecuteSqlRawAsync(sql, uliParam, employeeParam, salaryParam, employeeTypeParam);

                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public async Task<bool> RegisterEmployee(EmployeeRegisterModel employee)
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
                SqlParameter email = new SqlParameter("login", employee.Login);
                SqlParameter password = new SqlParameter("password", employee.Password);
                SqlParameter phoneNumber = new SqlParameter("phone", employee.PhoneNumber);
                SqlParameter employeeType = new SqlParameter("employeeType", employee.EmployeeTypeId);
                SqlParameter first = new SqlParameter("firstname", employee.FirstName);
                SqlParameter last = new SqlParameter("lastname", employee.LastName);
                SqlParameter salary = new SqlParameter("salary", employee.Salary);
                SqlParameter sex = new SqlParameter("sex", employee.Sex);
                SqlParameter hiredSince = new SqlParameter("hiredSince", employee.HiredSince);
                SqlParameter photo = new SqlParameter("photo", employee.PhotoPath);
                SqlParameter birthDate = new SqlParameter("birthdate", employee.BirthDate);


                await _db.Database.ExecuteSqlRawAsync(sql, email, password, phoneNumber, employeeType, first,
                    last, salary, sex, hiredSince, photo, birthDate);
                return true;

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public Task<List<UserType>> GetEmployeePositions()
        {
            string sql = "SELECT * FROM dbo.user_type ut" +
                " WHERE ut.id <> 1";

            return _db.UserTypes.FromSqlRaw(sql).AsQueryable().ToListAsync();
        }

        public Task MakeEmployeeResponsibleForRoom(RoomEmployee roomEmployee)
        {
            string sql = "INSERT INTO dbo.room_employee(employee_id, room_id) VALUES(@employeeId, @roomId)";
            SqlParameter employeeParam = new("employeeId", roomEmployee.EmployeeId);
            SqlParameter roomParam = new("roomId", roomEmployee.RoomId);

            return _db.Database.ExecuteSqlRawAsync(sql, employeeParam, roomParam);
        }

        public Task RemoveEmployeeResponsibility(RoomEmployee roomEmployee)
        {
            string sql = "DELETE FROM dbo.room_employee" +
                " WHERE employee_id = @employeeId AND room_id = @roomId";
            SqlParameter employeeParam = new("employeeId", roomEmployee.EmployeeId);
            SqlParameter roomParam = new("roomId", roomEmployee.RoomId);

            return _db.Database.ExecuteSqlRawAsync(sql, employeeParam, roomParam);
        }
    }
}
