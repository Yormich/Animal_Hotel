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
    }
}
