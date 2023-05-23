using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IAnimalService
    {
        public Task<IQueryable<Animal>> GetClientAnimals(long clientId, int pageIndex, int pageSize, bool withOwners = false);


        public Task<Animal?> GetAnimalById(long? animalId);

        public Task DeleteAnimalById(long animalId);

        public Task UpdateAnimal(Animal animal);

        public Task<long> CreateAnimal(Animal animal);

        public Task<bool> AnimalHasActiveContractOrBooking(long animalId);

        public Task<IQueryable<AnimalType>> GetAnimalTypes();

        public Task<AnimalType> GetAnimalTypeById(short typeId);

        public Task<int> GetClientAnimalsCount(long clientId);
    }
}
