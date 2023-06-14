using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Animal_Hotel.Services
{
    public interface IEmployeeService
    {
        public Task<EmployeeDataViewModel?> GetEmployeeById(long? employeeId);

        public Task DeleteEmployee(long employeeId);

        public Task<bool> UpdateEmployeeByManager(EmployeeDataViewModel employee);

        public Task<bool> RegisterEmployee(EmployeeRegisterModel employee);

        public Task<IQueryable<EmployeeDataViewModel>> GetEmployees(int pageIndex, int pageSize, bool excludeManagers = true);

        public Task<int> GetEmployeesCount(bool excludeManagers = true);

        public Task<List<UserType>> GetEmployeePositions();

        public Task MakeEmployeeResponsibleForRoom(RoomEmployee roomEmployee);

        public Task RemoveEmployeeResponsibility(RoomEmployee roomEmployee);

        public Task<List<Employee>> GetSuitableForRoomEmployees(short roomId);

        public Task<List<Employee>> GetWatcherWithRelatedAnimalsCount(long? watcherId = null);
    }
}
