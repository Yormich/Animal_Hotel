using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IEnclosureService
    {
        public Task<List<(long enclosureId, bool hasActiveBookings, bool hasActiveContracts)>> GetEnclosuresStatus(short roomId);

        public Task<AnimalEnclosure?> GetEnclosureById(long? enclosureId);

        public Task CreateEnclosure(AnimalEnclosure newEnclosure);

        public Task DeleteEnclosure(long enclosureId);

        public Task<bool> IsEnclosureHasActiveContractOrBooking(long enclosureId);

        public Task UpdateEnclosure(AnimalEnclosure enclosure);

        public Task<List<EnclosureType>> GetEnclosureTypes();
    }
}
