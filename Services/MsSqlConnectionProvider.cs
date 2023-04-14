using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Animal_Hotel.Services
{
    public class MsSqlConnectionProvider : IDbConnectionProvider
    {
        private readonly IConfiguration configuration;

        public MsSqlConnectionProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetConnection(HttpContext? context)
        {
            string role = context?.User.FindFirst(ClaimTypes.Role)?.Value ?? "AuthManager";

            string connectionString = configuration[$"MsSqlConnectionStrings:{role}"]!;
            return connectionString;
        }
    }
}
