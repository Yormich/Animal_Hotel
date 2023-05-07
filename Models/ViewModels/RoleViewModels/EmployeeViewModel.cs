using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class EmployeeViewModel : UserViewModel
    {
        public long EmployeeId { get; set; }

        [Range(0, 100000, ErrorMessage = "Salary should be between 0 and 100000")]
        public decimal Salary { get; set; }

        public DateTime HiredSince { get; set; }

        [Required]
        public char Sex { get; set; }
    }
}
