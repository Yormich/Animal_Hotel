using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels;

namespace Animal_Hotel.Services
{
    public interface IContractService
    {
        public Task<bool> DoesClientHasFinishedContract(long clientId);

        public Task<List<(int year, int month, string type, int count)>> GetAnimalsContractsByPeriod(DateTime startDate, DateTime endDate);

        public Task<List<(int year, int month, decimal monthIncome)>> GetIncomeByPeriod(DateTime startDate, DateTime endDate);

        public Task<(bool success, string? message)> CreateContract(Contract newContract);

        public Task<(bool success, string? message)> DeleteContract(long contractId);

        public Task UpdateContract(Contract updatedContract);

        public Task<List<ReceptionistReport>> GetContractsByDate(DateTime targetDate);

        public Task<Contract?> GetContractById(long contractId);
    }
}
