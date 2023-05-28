using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Animal_Hotel.Services
{
    public class ContractService : IContractService
    {
        private readonly AnimalHotelDbContext _db;

        public ContractService(AnimalHotelDbContext context)
        {
            _db = context;
        }

        public Task<bool> DoesClientHasFinishedContract(long clientId)
        {
            string sql = "SELECT DISTINCT cl.* FROM dbo.client cl" +
                " INNER JOIN dbo.animal a ON a.owner_id = cl.id" +
                " INNER JOIN dbo.contract c ON c.animal_id = a.id" +
                " WHERE cl.id = @clientId AND c.check_out_date IS NOT NULL";

            SqlParameter clientParam = new("clientId", clientId);

            return _db.Clients.FromSqlRaw(sql, clientParam)
                .AsQueryable()
                .AnyAsync();
        }
    }
}
