using Animal_Hotel.Models.DatabaseModels;
using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels
{
    public class HomePageViewModel
    {
        [Required(ErrorMessage = "Please enter login to proceed")]
        [EmailAddress(ErrorMessage = "Please enter valid email")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter password to proceed")]
        [MinLength(8, ErrorMessage = "Password should contain at least 8 characters")]
        public string Password { get; set; } = string.Empty;
        public string ToView { get; set; } = string.Empty;

        public bool IsInteractedWithModal { get; set; } = false;

        public List<Review> Reviews { get; set; } = new();

        public PaginatedList<Room>? Rooms { get; set; }

    }
}
