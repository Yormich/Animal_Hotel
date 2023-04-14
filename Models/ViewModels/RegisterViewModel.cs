using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels
{
    //by default, we register as clients employee creation is available only for manager
    //i'm using inheritance rather than interface realization because we don't need to overwrite data validation attributes
    public abstract class RegisterViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        [EmailAddress(ErrorMessage = "Enter correct email")]
        [StringLength(50, MinimumLength = 6)]
        public string Login { get; set; } = string.Empty;


        [Required(ErrorMessage ="This field is required")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$", ErrorMessage = 
            "Password should contain at least 1 uppercase and lowercase letters, " +
            "1 digit with minimum length of 8 characters")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password length should be between 8 and 40 characters")]       
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords should match")]
        public string? RepeatedPassword { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Phone(ErrorMessage = "Enter correct phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please set your birth date")]
        public DateTime BirthDate { get; set; }
    }
}
