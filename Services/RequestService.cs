using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Animal_Hotel.Services
{
    public class RequestService : IRequestService
    {
        private readonly AnimalHotelDbContext _db;

        public RequestService(AnimalHotelDbContext db)
        {
            _db = db;
        }

        public Task<IQueryable<Request>> GetRequestsByPageIndex(long employeeId, int pageIndex, int pageSize)
        {
            string sql = "SELECT * FROM dbo.request r" +
                " WHERE r.employee_id = @employeeId" +
                " ORDER BY r.id" +
                " OFFSET @skipAmount ROWS" +
                " FETCH NEXT @toTake ROWS ONLY";

            return Task.Run(() =>
            {
                SqlParameter skipParam = new("skipAmount", pageSize * (pageIndex - 1));
                SqlParameter toTakeParam = new("toTake", pageSize);
                SqlParameter employeeIdParam = new("employeeId", employeeId);

                return _db.Requests.FromSqlRaw(sql, skipParam, toTakeParam, employeeIdParam)
                    .Include(r => r.Status)
                    .AsQueryable();
            });
        }

        public Task<IQueryable<Request>> GetEmployeeRequestsById(long employeeId)
        {
            string sql = "SELECT * FROM dbo.request r" +
                " WHERE r.employee_id = @employeeId";
            SqlParameter idParam = new SqlParameter("employeeId", employeeId);

            return Task.Run(() => _db.Requests.FromSqlRaw(sql, idParam)
                    .Include(r => r.Status).AsQueryable());
        }

        public Task<int> GetEmployeeRequestsCount(long employeeId)
        {
            string sql = "SELECT * FROM dbo.request r" +
                " WHERE r.employee_id = @employeeId";
            SqlParameter idParam = new SqlParameter("employeeId", employeeId);


            return _db.Requests.FromSqlRaw(sql, idParam).AsQueryable().CountAsync();
        }

        public Task DeleteRequest(long requestId)
        {
            string sql = "DELETE FROM dbo.request" +
                " WHERE id = @requestId";
            SqlParameter requestParam = new SqlParameter("requestId", requestId);
            
            return _db.Database.ExecuteSqlRawAsync(sql, requestParam);
        }

        public Task UpdateRequest(Request request)
        {
            string sql = "UPDATE dbo.request" +
                " SET request_text = @text, writing_date = GETDATE(), status_id = 1" +
                " WHERE id = @requestId";
            SqlParameter idParam = new SqlParameter("requestId", request.Id);
            SqlParameter textParam = new SqlParameter("text", request.Text);

            return _db.Database.ExecuteSqlRawAsync(sql, idParam, textParam);
        }
            
        public Task AddRequest(Request request)
        {
            string sql = "INSERT INTO dbo.request(request_text, writing_date, status_id, employee_id)" +
                " VALUES(@requestText, DEFAULT, @statusId, @employeeId)";
            SqlParameter textParam = new SqlParameter("requestText", request.Text);
            SqlParameter employeeParam = new SqlParameter("employeeId", request.EmployeeId);
            SqlParameter statusParam = new SqlParameter("statusId", request.StatusId);

            return _db.Database.ExecuteSqlRawAsync(sql, textParam, employeeParam, statusParam);
        }

        public Task<Request?> GetRequestById(long? requestId)
        {
            string sql = "SELECT * FROM dbo.request r" +
                         " WHERE r.id = @requestId";
            SqlParameter requestParam = new SqlParameter("requestId", requestId);

            return _db.Requests.FromSqlRaw(sql, requestParam).Include(r => r.Status)
                .Include(r => r.Writer)
                .AsQueryable()
                .FirstOrDefaultAsync();
        }

        public Task<RequestStatus?> GetRequestStatusByStatus(string status)
        {
            string sql = "SELECT * FROM dbo.request_status rs" +
                " WHERE rs.status LIKE @status";

            SqlParameter statusParam = new SqlParameter("status", status);
            return _db.RequestStatuses.FromSqlRaw(sql, statusParam).FirstOrDefaultAsync();
        }

        public Task<IQueryable<Request>> GetRequestsForManager(int pageIndex, int pageSize)
        {
            string sql = "SELECT * FROM dbo.request r" +
                " ORDER BY r.status_id, r.writing_date" +
                " OFFSET @skipAmount ROWS" +
                " FETCH NEXT @toTake ROWS ONLY";

            SqlParameter skipParam = new("skipAmount", pageSize * (pageIndex - 1));
            SqlParameter toTakeParam = new("toTake", pageSize);

            return Task.Run(() => _db.Requests.FromSqlRaw(sql, skipParam, toTakeParam)
                .Include(r => r.Writer)
                .Include(r => r.Status)
                .AsQueryable());
        }

        public Task<int> GetAllRequestsCount()
        {
            return _db.Requests.CountAsync();
        }

        public Task<List<RequestStatus>> GetRequestStatusesForUpdate()
        {
            string sql = "SELECT * FROM dbo.request_status rs";
            return _db.RequestStatuses.FromSqlRaw(sql).ToListAsync();
        }

        public Task UpdateRequestStatus(long requestId, short statusId)
        {
            string sql = "UPDATE dbo.request" +
                " SET status_id = @statusId" +
                " WHERE id = @requestId";

            SqlParameter requestParam = new("requestId", requestId);
            SqlParameter statusParam = new("statusId", statusId);

            return _db.Database.ExecuteSqlRawAsync(sql, requestParam, statusParam);
        }
    }
}
