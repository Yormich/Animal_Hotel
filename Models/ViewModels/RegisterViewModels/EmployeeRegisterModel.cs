using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels.RegisterViewModels
{
    public class EmployeeRegisterModel : RegisterViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        public short EmployeeTypeId { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Range(0, 100000, ErrorMessage = "Salary should be between 0 and 100000")]
        public decimal Salary { get; set; }

        public DateTime HiredSince { get; set; } = DateTime.Now;

        public string? PhotoPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        public char Sex { get; set; }
    }
}
