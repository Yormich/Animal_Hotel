using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using System;
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

        public Task<UserViewModel> FormUserByClaims()
        {
            return Task.Run(() => 
            {
                string[] fullName = this.GetClaimValue(ClaimTypes.Name).Split(' ',StringSplitOptions.RemoveEmptyEntries);
                string firstName = fullName[0], lastName = fullName[1];

                return new UserViewModel()
                {
                    UserId = Convert.ToInt64(this.GetClaimValue(ClaimTypes.PrimarySid)),
                    SubUserId = Convert.ToInt64(this.GetClaimValue(ClaimTypes.Sid)),
                    Login = this.GetClaimValue(ClaimTypes.Email),
                    PhoneNumber = this.GetClaimValue(ClaimTypes.MobilePhone),
                    BirthDate = Convert.ToDateTime(this.GetClaimValue(ClaimTypes.DateOfBirth)),
                    FirstName = firstName,
                    LastName = lastName,
                    PhotoPath = this.GetClaimValue("ProfileImagePath"),
                };
            });
        } 
    }
}
