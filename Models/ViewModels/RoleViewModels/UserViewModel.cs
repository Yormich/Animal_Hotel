using Animal_Hotel.Models.DatabaseModels;
using System.ComponentModel.DataAnnotations;


namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class UserViewModel
    {
        [Required]
        public long UserId { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [EmailAddress(ErrorMessage = "Enter correct email")]
        [StringLength(50, MinimumLength = 6)]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [Phone(ErrorMessage = "Enter correct phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;

        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$", ErrorMessage =
            "Password should contain at least 1 uppercase and lowercase letters, " +
            "1 digit with minimum length of 8 characters")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password length should be between 8 and 40 characters")]
        public string? RepeatedPassword { get; set; }

        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$", ErrorMessage =
            "Password should contain at least 1 uppercase and lowercase letters, " +
            "1 digit with minimum length of 8 characters")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password length should be between 8 and 40 characters")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        public string? PhotoPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please set your birth date")]
        public DateTime BirthDate { get; set; }

        public UserType? UserType { get; set; }

        public Dictionary<string, (string Controller, string Display)> Actions { get; set; } = new();
    }
}
