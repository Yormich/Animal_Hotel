using System.Security.Claims;

namespace Animal_Hotel.Services
{
    public class ClaimHelper
    {
        private readonly IEnumerable<Claim> _claims;

        public ClaimHelper(IHttpContextAccessor contextAccessor) 
        {
            _claims = contextAccessor.HttpContext!.User.Claims;
        }

        public string GetClaimValue(string claimType)
        {
            return _claims.First(c => string.Compare(c.Type, claimType, StringComparison.CurrentCulture) == 0).Value;
        }
    }
}
