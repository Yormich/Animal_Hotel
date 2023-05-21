using Animal_Hotel.Models.DatabaseModels;
using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class EmployeeDataViewModel : UserViewModel
    {

        [Range(0, 100000, ErrorMessage = "Salary should be between 0 and 100000")]
        public decimal Salary { get; set; }

        public DateTime HiredSince { get; set; }

        [Required]
        public char Sex { get; set; }

        public PaginatedList<Request>? Requests { get; set; } 

        public EmployeeDataViewModel() 
        {

        }
    }
}
