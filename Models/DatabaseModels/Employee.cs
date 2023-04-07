using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        public long Id { get; set; }


        [Required]
        [Column("first_name")]
        [StringLength(maximumLength: 50, MinimumLength = 2, ErrorMessage = "First name length should be between 2 and 50 characters")]
        public string FirstName { get; set; }

        [Required]
        [Column("last_name")]
        [StringLength(maximumLength: 50, MinimumLength = 2, ErrorMessage = "Last name length should be between 2 and 50 characters")]
        public string LastName { get; set; }

        [Required]
        [Range(0, 100_000)]
        public decimal Salary { get; set; }

        [Required]
        [Column("sex")]
        public char Sex { get; set; }

        [Required]
        [Column("birth_date")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required]
        [Column("hired_since")]
        [DataType(DataType.Date)]
        public DateTime HiredSince { get; set; }

        public UserLoginInfo LoginInfo { get; set; } = null!;

        [Required]
        [Column("photo_name")]
        [StringLength(maximumLength: 40, MinimumLength = 5, ErrorMessage = "Photo path length should be between 5 and 40 characters")]
        public string PhotoPath { get; set; }

        public List<Room> Rooms { get; set; } = new();
        public List<RoomEmployee> RoomEmployees { get; set; } = new();

        public List<Request> Requests { get; set; } = new();

        public Employee(string firstName, string lastName, decimal salary, char sex, DateTime birthDate, 
            DateTime hiredSince, string photoPath)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Salary = salary;
            this.Sex = sex;
            this.BirthDate = birthDate;
            this.HiredSince = hiredSince;
            this.PhotoPath = photoPath;
        }
    }
}
