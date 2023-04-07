using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("enclosure_type")]
    public class EnclosureType
    {
        [Key]
        public short Id { get; set; }

        [Required]
        [StringLength(maximumLength:30, ErrorMessage = "Type length should be between 3 and 30 characters", MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        public decimal Surcharge { get; set; }

        public List<AnimalEnclosure> Enclosures { get; set; } = new();

        public EnclosureType(string name, decimal surcharge = 0.0m)
        {
            this.Name = name;
            this.Surcharge = surcharge;
        }
    }
}
