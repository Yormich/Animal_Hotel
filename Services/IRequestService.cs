using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;

namespace Animal_Hotel.Services
{
    public interface IRequestService
    {
        public Task<int> GetEmployeeRequestsCount(long employeeId);

        public Task<int> GetAllRequestsCount();

        public Task<IQueryable<Request>> GetEmployeeRequestsById(long employeeId);

        public Task<IQueryable<Request>> GetRequestsByPageIndex(long employeeId, int pageIndex, int pageSize);

        public Task<IQueryable<Request>> GetRequestsForManager(int pageIndex, int pageSize);

        public Task<Request?> GetRequestById(long? requestId);

        public Task<RequestStatus?> GetRequestStatusByStatus(string status);

        public Task DeleteRequest(long requestId);

        public Task UpdateRequest(Request request);

        public Task AddRequest(Request request);

        public Task<List<RequestStatus>> GetRequestStatusesForUpdate();

        public Task UpdateRequestStatus(long requestId, short statusId);
    }
}
