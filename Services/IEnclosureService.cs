using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels;

namespace Animal_Hotel.Services
{
    public interface IEnclosureService
    {
        public Task<List<EnclosureStatusViewModel>> GetEnclosuresStatus(short roomId);

        public Task<AnimalEnclosure?> GetEnclosureById(long? enclosureId);

        public Task CreateEnclosure(AnimalEnclosure newEnclosure);

        public Task<(bool success, string? message)> DeleteEnclosure(long enclosureId);

        public Task<(bool success, string? message)> UpdateEnclosure(AnimalEnclosure enclosure);

        public Task<List<EnclosureType>> GetEnclosureTypes();

        public Task<List<AnimalEnclosure>> GetSuitableEnclosures(long animalId);
    }
}
