using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("client")]
    public class Client
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
        [Column("birth_date")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Column("card_number")]
        [StringLength(maximumLength: 20, MinimumLength = 16)]
        [CreditCard(ErrorMessage = "Please enter valid credit card")]
        public string? CardNumber { get; set; }

        [Column("photo_name")]
        [StringLength(maximumLength:40, MinimumLength = 5, ErrorMessage = "Photo path length should be between 5 and 40 characters")]
        public string? PhotoPath { get; set; }

        public Review? Review { get; set; }
        public UserLoginInfo? LoginInfo { get; set; }

        public List<Animal> Animals { get; set; } = new();

        public Client(string firstName, string lastName, DateTime birthDate, string? cardNumber = null, string? photoPath = null)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.BirthDate = birthDate;
            this.CardNumber = cardNumber;
            this.PhotoPath = photoPath;
        }
    }
}
