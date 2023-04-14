using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IUserLoginInfoService
    {
        public Task<IQueryable<UserLoginInfo>> GetUserLoginsWithTypes();

        public Task<UserLoginInfo?> GetLoginWithRoleAndPersonalInfo(string email);

        public Task<UserLoginInfo?> GetUserLoginInfoWithRole(string email);
    }
}
