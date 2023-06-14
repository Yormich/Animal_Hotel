using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("animal_type")]
    public class AnimalType
    {
        [Key]
        public short Id { get; set; }

        [Required]
        [StringLength(maximumLength: 40, MinimumLength = 2, ErrorMessage = "Animal Type length should be between 2 and 40 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1,10000)]
        [Column("base_price")]
        public decimal BasePrice { get; set; }
        public List<Animal>? Animals { get; set; }
        public List<AnimalEnclosure>? Enclosures { get; set; }

        public AnimalType(string name, decimal basePrice)
        {
            this.Name = name;
            this.BasePrice = basePrice;
        }

        public AnimalType() { }
    }
}
