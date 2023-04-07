using Microsoft.AspNetCore.Http;

namespace Animal_Hotel.Services
{
    public interface IDbConnectionProvider
    {
        public string GetConnection(HttpContext? context);
    }
}
