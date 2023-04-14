using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels
{
    public class EmployeeRegisterModel : RegisterViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        public short EmployeeTypeId { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public decimal Salary { get; set; }

        public DateTime HiredSince { get; set; } = DateTime.Now;

        public string PhotoPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        public char Sex { get; set; }
    }
}
