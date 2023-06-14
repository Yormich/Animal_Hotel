using Animal_Hotel.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Animal_Hotel.Services
{
    public interface IAnimalService
    {
        public Task<IQueryable<Animal>> GetClientAnimals(long clientId, int pageIndex, int pageSize, bool withOwners = false);


        public Task<Animal?> GetAnimalById(long? animalId);

        public Task<(bool success, string? message)> DeleteAnimalById(long animalId);

        public Task UpdateAnimal(Animal animal);

        public Task<long> CreateAnimal(Animal animal);

        public Task<IQueryable<AnimalType>> GetAnimalTypes();

        public Task<AnimalType> GetAnimalTypeById(short typeId);

        public Task<List<Animal>> GetAnimalsByWatcherId(long watcherId);

        public Task<int> GetClientAnimalsCount(long clientId);

        public Task<List<Animal>> GetSuitableAnimals(long enclosureId, long clientId);

        public Task<List<Animal>> GetAvailableClientAnimals(long clientId);
    }
}
