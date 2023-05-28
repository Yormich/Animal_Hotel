using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;

namespace Animal_Hotel.Services
{
    public interface IEmployeeService
    {
        public Task<EmployeeDataViewModel?> GetEmployeeById(long? employeeId);

        public Task DeleteEmployee(long employeeId);

        public Task<bool> UpdateEmployeeByManager(EmployeeDataViewModel employee);

        public Task<bool> RegisterEmployee(EmployeeDataViewModel employee);

        public Task<IQueryable<EmployeeDataViewModel>> GetEmployees(int pageIndex, int pageSize, bool excludeManagers = true);

        public Task<int> GetEmployeesCount(bool excludeManagers = true);
    }
}
