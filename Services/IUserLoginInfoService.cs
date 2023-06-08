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

        public Task<ClientDataViewModel?> GetClientDataById(long loginId);
        public Task<EmployeeDataViewModel?> GetEmployeeDataById(long loginId);

        public Task UpdatePasswordById(long userId, string password);

        public Task<(bool isExists, string errorMessage)> IsUserWithPhoneAndEmailExists(string email, string phone, long? requester = null);

        //TODO: split this methods across services
        public Task<bool> UpdateClient(ClientDataViewModel clientViewModel);

        public Task<bool> UpdateEmployeePersonalData(EmployeeDataViewModel employeeViewModel);
    }
}
