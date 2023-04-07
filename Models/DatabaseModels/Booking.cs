using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("booking")]
    public class Booking
    {
        [Key]
        [Required]
        [Column("animal_id")]
        [ForeignKey("Animal")]
        public long AnimalId { get; set; }
        public Animal Animal { get; set; } = null!;

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("date_of_leaving")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("enclosure_id")]
        [ForeignKey("Enclosure")]
        public long EnclosureId { get; set; }

        public AnimalEnclosure Enclosure { get; set; } = null!;

        public Booking(long animalId, long enclosureId, DateTime startDate, DateTime endDate)
        {
            this.AnimalId = animalId;
            this.EnclosureId = enclosureId;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }
    }
}
