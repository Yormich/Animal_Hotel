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

        public bool HasClaimWithValue(string claimValue, string claimType)
        {
            return _claims.Any(c => string.Compare(c.Value, claimValue, StringComparison.CurrentCulture) == 0 &&
                string.Compare(c.Type, claimType, StringComparison.CurrentCulture) == 0);
        }

        public bool HasClaimWithValue(string claimValue)
        {
            return _claims.Any(c => string.Compare(c.Value, claimValue, StringComparison.CurrentCulture) == 0);
        }
    }
}
