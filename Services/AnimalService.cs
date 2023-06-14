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
                .Include(a => a.Owner)
                .ThenInclude(c => c!.LoginInfo)
                .AsQueryable()
                .FirstOrDefaultAsync();
        }

        public async Task<(bool success, string? message)> DeleteAnimalById(long animalId)
        {
            string sql = "EXEC dbo.DeleteAnimal" +
                "   @animal_id = @animalId;";

            SqlParameter idParam = new("animalId", animalId);

            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, idParam);
            }
            catch(SqlException se)
            {
                Console.WriteLine(se.Message);
                return new(false, se.Message);
            }

            return new(true, string.Empty);
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

        public Task<List<Animal>> GetAnimalsByWatcherId(long watcherId)
        {
            string sql = "SELECT DISTINCT a.* FROM dbo.animal a" +
                " INNER JOIN dbo.contract c ON c.animal_id = a.id" +
                " INNER JOIN dbo.animal_enclosure ae ON ae.id = c.enclosure_id" +
                " INNER JOIN dbo.room r ON r.id = ae.room_id" +
                " INNER JOIN dbo.room_employee re ON re.room_id = r.id"  +
                " INNER JOIN dbo.employee e ON e.id = re.employee_id" +
                " WHERE e.id = @employeeId AND c.check_out_date IS NULL";

            SqlParameter watcherParam = new("employeeId", watcherId);

            return _db.Animals.FromSqlRaw(sql, watcherParam)
                .Include(a => a.AnimalType)
                .Include(a => a.Owner)
                .AsQueryable()
                .ToListAsync();
        }

        public Task<List<Animal>> GetSuitableAnimals(long enclosureId, long clientId)
        {
            string sql = " SELECT a.* FROM dbo.animal a" +
                " INNER JOIN dbo.animal_type at ON at.id = a.type_id" +
                " WHERE a.owner_id = @clientId AND at.id = " +
                " (" +
                "   SELECT atInner.id FROM dbo.animal_enclosure ae" +
                "   INNER JOIN dbo.animal_type atInner ON atInner.id = ae.animal_type_id" +
                "   WHERE ae.id = @enclosureId" +
                " )";

            SqlParameter enclosureParam = new("enclosureId", enclosureId);
            SqlParameter clientParam = new("clientId", clientId);

            return _db.Animals.FromSqlRaw(sql, enclosureParam, clientParam)
                .Include(a => a.AnimalType)
                .ToListAsync();
        }

        public Task<List<Animal>> GetAvailableClientAnimals(long clientId)
        {
            string sql = "SELECT a.* FROM dbo.animal a " +
                " WHERE a.owner_id = @clientId AND a.id NOT IN " +
                " (" +
                "   SELECT DISTINCT aInner.id FROM dbo.animal aInner" +
                "   INNER JOIN dbo.contract c ON c.animal_id = aInner.id" +
                "   WHERE aInner.owner_id = 1 AND  c.check_out_date IS NULL" +
                " )";

            SqlParameter clientParam = new("clientId", clientId);

            return _db.Animals.FromSqlRaw(sql, clientParam)
                .Include(a => a.AnimalType)
                .AsQueryable()
                .ToListAsync();
        }
    }
}
