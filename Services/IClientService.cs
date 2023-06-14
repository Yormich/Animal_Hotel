using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;

namespace Animal_Hotel.Services
{
    public interface IClientService
    {
        public Task<bool> RegisterClient(ClientRegisterModel model);

        public Task<List<(int year, int month, int clientsCount)>> RegisteredActiveClients(DateTime startDate, DateTime endDate);

        public Task<IQueryable<Client>> GetClientsByPageIndex(int pageIndex, int pageSize);

        public Task<int> GetClientCount();

        public Task<Client?> GetClientById(long? clientId);
    }
}
