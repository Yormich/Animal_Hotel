using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("user_login_info")]
    public class UserLoginInfo
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Column("login")]
        [EmailAddress(ErrorMessage = "Please enter correct email")]
        [StringLength(maximumLength: 50, ErrorMessage = "Login length should be between 6 and 50 characters", MinimumLength = 6)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public byte[] Password { get; set; } = Array.Empty<byte>(); 

        [Required]
        [Column("phone_number")]
        [Phone(ErrorMessage = "Please enter correct phone number")]
        [StringLength(maximumLength: 20, ErrorMessage = "Phone number should contain from 10 to 20 characters", MinimumLength = 10)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Column("user_type_id")]
        [ForeignKey("UserType")]
        public short UserTypeId { get; set; }

        public UserType UserType { get; set; } = null!;

        [ForeignKey("Client")]
        [Column("client_id")]
        public long? ClientId { get; set; }

        public Client? Client { get; set; }

        [ForeignKey("Employee")]
        [Column("employee_id")]
        public long? EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public UserLoginInfo(string email, byte[] password, string phoneNumber, short userTypeId,
            long? clientId = null, long? employeeId = null)
        {
            this.Email = email;
            this.Password = password;
            this.PhoneNumber = phoneNumber;
            this.UserTypeId = userTypeId;
            this.ClientId = clientId;
            this.EmployeeId = employeeId;
        }

        public UserLoginInfo()
        {
        
        }
    }
}
