namespace Animal_Hotel.Services
{
    public interface IContractService
    {
        public Task<bool> DoesClientHasFinishedContract(long clientId);
    }
}
