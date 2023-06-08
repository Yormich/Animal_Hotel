using Animal_Hotel.Models.DatabaseModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class EmployeeDataViewModel : UserViewModel
    {

        [Column("salary")]
        [Range(0, 100000, ErrorMessage = "Salary should be between 0 and 100000")]
        public decimal Salary { get; set; }

        [Column("hired_since")]
        public DateTime HiredSince { get; set; }

        [Required]
        [Column("sex")]
        public char Sex { get; set; }

        [NotMapped]
        public short Position { get; set; }

        public EmployeeDataViewModel() 
        {

        }
    }
}
