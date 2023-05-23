using Microsoft.AspNetCore.Routing.Constraints;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("animal")]
    public class Animal
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(maximumLength: 40, MinimumLength = 2, ErrorMessage = "Name length should be between 2 and 40 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 50, ErrorMessage = "Animal age should be between 1 and 50")]
        public short Age { get; set; }

        [Required]
        public char Sex { get; set; }

        [Required]
        [Range(0.1d, 30d, ErrorMessage = "Animal Weight should be between 100g and 30kg")]
        public double Weight { get; set; }

        [StringLength(400, MinimumLength = 6, ErrorMessage = "Animal Preferences length should be between 6 and 400 characters")]
        public string? Preferences { get; set; }

        [Column("photo_name")]
        [StringLength(maximumLength: 40, MinimumLength = 5, ErrorMessage = "Photo file Name length should be between 5 and 40 characters")]
        public string? PhotoPath { get; set; }

        [Required]
        [Column("owner_id")]
        [ForeignKey("Owner")]
        public long OwnerId { get; set; }
        public Client? Owner { get; set; }

        [Required]
        [Column("type_id")]
        [ForeignKey("AnimalType")]
        public short TypeId { get; set; } 
        public AnimalType? AnimalType { get; set; }

        public Booking? Booking { get; set; }

        public List<Contract> Contracts { get; set; } = new();

        public Animal(string name, short age, char sex, double weight, long ownerId, short typeId, string? preferences = null,
            string? photoPath = null)
        {
            this.Name = name;
            this.Age = age;
            this.Sex = sex;
            this.Weight = weight;
            this.OwnerId = ownerId;
            this.TypeId = typeId;
            this.Preferences = preferences;
            this.PhotoPath = photoPath;
        }

        public Animal() { }
    }
}
