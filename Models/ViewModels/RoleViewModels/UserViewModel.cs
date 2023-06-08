using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Services;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class UserViewModel
    {
        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        [Column("sub_user_id")]
        public long SubUserId { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Column("login")]
        [EmailAddress(ErrorMessage = "Enter correct email")]
        [StringLength(50, MinimumLength = 6)]
        public string Login { get; set; } = string.Empty;

        [Column("phone_number")]
        [Required(ErrorMessage = "This field is required")]
        [Phone(ErrorMessage = "Enter correct phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("password")]

        public byte[]? Password { get; set; }

        [NotMapped]
        public string UserPassword { get; set; } = string.Empty;


        [NotMapped]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$", ErrorMessage =
            "Password should contain at least 1 uppercase and lowercase letters, " +
            "1 digit with minimum length of 8 characters")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password length should be between 8 and 40 characters")]
        public string? RepeatedPassword { get; set; }

        [NotMapped]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$", ErrorMessage =
            "Password should contain at least 1 uppercase and lowercase letters, " +
            "1 digit with minimum length of 8 characters")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password length should be between 8 and 40 characters")]
        public string? NewPassword { get; set; }


        [Column("first_name")]
        [Required(ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        [Required(ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Column("photo_path")]
        public string? PhotoPath { get; set; } = string.Empty;

        [Column("birth_date")]
        [Required(ErrorMessage = "Please set your birth date")]
        public DateTime BirthDate { get; set; }

        public UserType? UserType { get; set; }

        [Column("user_type_id")]
        public short UserTypeId { get; set; }

        [NotMapped]
        public Dictionary<string, (string Controller, string Display)> Actions { get; set; } = new();

        [NotMapped]
        public string ActiveAction { get; set; } = string.Empty;

        [NotMapped]
        public bool IsInteractedWithModal { get; set; }

        public static async Task<UserViewModel> CreateUser(ClaimHelper claimHelper, IUserTypeService userTypeService, IMemoryCache cache)
        {
            var userActionsTask = UtilFuncs.CreateUserActionsList(claimHelper.GetClaimValue(ClaimTypes.Role), cache);
            var userTask = claimHelper.FormUserByClaims();
            var userTypeTask = userTypeService
                .GetUserTypeByUserId(Convert.ToInt64(claimHelper.GetClaimValue(ClaimTypes.PrimarySid)));

            UserViewModel user = await userTask;
            user.Actions = await userActionsTask;
            user.UserType = await userTypeTask;
            return user;
        }
    }
}
