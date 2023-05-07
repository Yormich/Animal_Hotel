using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;

namespace Animal_Hotel.Services
{
    public interface IUserLoginInfoService
    {
        public Task<IQueryable<UserLoginInfo>> GetUserLoginsWithTypes();
        public Task<UserLoginInfo?> GetLoginWithRoleAndPersonalInfo(string email);
        public Task<UserLoginInfo?> GetUserLoginInfoWithRole(string email);

        public Task<string?> GetUserPhotoPathById(long userId);

        public Task<UserType> GetUserTypeByUserId(long userId);
        public Task<ClientViewModel?> GetClientDataById(long loginId);
        public Task<EmployeeViewModel?> GetEmployeeDataById(long loginId);

        public Task UpdatePasswordById(long userId, string password);

        public Task<bool> UpdateClient(ClientViewModel clientViewModel);

        public Task<bool> UpdateEmployeePersonalData(EmployeeViewModel employeeViewModel);
    }
}
