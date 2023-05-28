using Animal_Hotel.Models.ViewModels.RegisterViewModels;

namespace Animal_Hotel.Services
{
    public interface IClientService
    {
        public Task<bool> RegisterClient(ClientRegisterModel model);
    }
}
