using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IAnimalService
    {
        public Task<IQueryable<Animal>> GetClientAnimals(long clientId, int pageIndex, int pageSize, bool withOwners = false);
    }
}
