using Animal_Hotel.Models.ViewModels;

namespace Animal_Hotel.Services
{
    public interface IUserRegisterService
    {
        public Task<bool> RegisterClient(ClientRegisterModel model);

        public Task<bool> RegisterEmployee(EmployeeRegisterModel model);
    }
}
