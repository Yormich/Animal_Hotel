using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Animal_Hotel.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly AnimalHotelDbContext _db;
        public AnimalService(AnimalHotelDbContext db)
        {
            _db = db;
        }

        public Task<IQueryable<Animal>> GetClientAnimals(long clientId, int pageIndex, int pageSize, bool withOwners = false)
        {
            string sql = "SELECT * FROM dbo.animal a" +
                " WHERE a.owner_id = @clientId" +
                " ORDER BY a.id" +
                " OFFSET @skipAmount ROWS" +
                " FETCH NEXT @pageSize ROWS ONLY";

            return Task.Run(() =>
            {
                SqlParameter idParam = new SqlParameter("clientId", clientId);
                SqlParameter skipParam = new SqlParameter("skipAmount", pageSize * (pageIndex - 1));
                SqlParameter takeParam = new SqlParameter("pageSize", pageSize);

                var animals = _db.Animals.FromSqlRaw(sql, idParam, skipParam, takeParam)
                    .Include(a => a.AnimalType);

                if (withOwners)
                {
                    animals.Include(a => a.Owner);
                }

                return animals.AsQueryable();
            });
        }

        public Task<Animal?> GetAnimalById(long? animalId)
        {
            string sql = "SELECT * FROM dbo.animal a" +
                " WHERE a.id = @animalId";
            SqlParameter animalParam = new("animalId", animalId);

            return _db.Animals.FromSqlRaw(sql, animalParam)
                .Include(a => a.AnimalType)
                .AsQueryable()
                .FirstOrDefaultAsync();
        }

        public Task DeleteAnimalById(long animalId)
        {
            string sql = "DELETE FROM dbo.animal" +
                " WHERE id = @animalId";

            SqlParameter idParam = new("animalId", animalId);

            return _db.Database.ExecuteSqlRawAsync(sql, idParam);
        }

        public Task UpdateAnimal(Animal animal)
        {
            string sql = "UPDATE dbo.animal" +
                " SET name = @name, age = @age, weight = @weight, preferences = @preferences, photo_name = @photoName" +
                " WHERE id = @animalId";

            SqlParameter idparam = new("animalId", animal.Id);
            SqlParameter nameParam = new("name", animal.Name);
            SqlParameter ageParam = new("age", animal.Age);
            SqlParameter weightParam = new("weight", animal.Weight);
            SqlParameter preferencesParam = new("preferences", animal.Preferences);
            SqlParameter photoParam = new("photoName", animal.PhotoPath);

            return _db.Database.ExecuteSqlRawAsync(sql, idparam, nameParam, ageParam, weightParam, preferencesParam, photoParam);
        }

        public async Task<long> CreateAnimal(Animal animal)
        {
            _db.Animals.Add(animal);
            await _db.SaveChangesAsync();

            return animal.Id;
        }

        public Task<bool> AnimalHasActiveContractOrBooking(long animalId)
        {
            string sql = "SELECT DISTINCT a.* FROM dbo.animal a\r\nINNER JOIN dbo.contract c ON c.animal_id = a.id" +
                " WHERE a.id = @animalId AND (c.check_out_date IS NULL OR a.id IN " +
                "(" +
                "   SELECT aInner.id FROM dbo.animal aInner" +
                "   INNER JOIN dbo.booking b ON b.animal_id = aInner.id" +
                "   WHERE aInner.id = @animalId" +
                "))";

            SqlParameter animalIdParam = new("animalId", animalId);

            return _db.Animals.FromSqlRaw(sql, animalIdParam)
                .AsQueryable()
                .AnyAsync();
        }

        public Task<IQueryable<AnimalType>> GetAnimalTypes()
        {
            string sql = "SELECT * FROM dbo.animal_type at";
            return Task.Run(() => _db.AnimalTypes.FromSqlRaw(sql).AsQueryable());
        }

        public Task<AnimalType> GetAnimalTypeById(short typeId)
        {
            string sql = "SELECT * FROM dbo.animal_type at" +
                         " WHERE at.id = @typeId";

            SqlParameter idParam = new("typeId", typeId);
            return _db.AnimalTypes.FromSqlRaw(sql, idParam)
                .AsQueryable()
                .FirstAsync();
        }

        public Task<int> GetClientAnimalsCount(long clientId)
        {
            string sql = "SELECT * FROM dbo.animal a" +
                " WHERE a.owner_id = @clientId";

            SqlParameter clientParam = new("clientId", clientId);

            return _db.Animals.FromSqlRaw(sql, clientParam).CountAsync();
        }
    }
}
