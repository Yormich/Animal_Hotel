using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Flags]
    public enum EnclosureStatus
    {
        [Display(Name = "Available")]
        None = 0,
        [Display(Name = "Booked")]
        HasBooking = 1,
        [Display(Name = "Occupied")]
        HasContract = 2
    }


    [Table("animal_enclosure")]
    public class AnimalEnclosure
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Range(0, 10)]
        [Column("max_animals")]
        public short MaxAnimals { get; set; }

        [Required]
        [Column("room_id")]
        [ForeignKey("Room")]
        public short RoomId { get; set; }

        public Room? Room { get; set; }

        [Required]
        [ForeignKey("EnclosureType")]
        [Column("enclosure_type_id")]
        public short EnclosureTypeId { get; set; }

        public EnclosureType? EnclosureType { get; set; }

        [Required]
        [Column("animal_type_id")]
        [ForeignKey("AnimalType")]
        public short AnimalTypeId { get; set; }

        public AnimalType? AnimalType { get; set; }

        [Column("price_per_day")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal PricePerDay { get; set; }

        [NotMapped]
        public EnclosureStatus EnclosureStatus { get; set; } = EnclosureStatus.None;

        public List<Booking>? Bookings { get; set; }

        public List<Contract>? Contracts { get; set; }

        public AnimalEnclosure(short maxAnimals, short roomId, short enclosureTypeId, short animalTypeId)
        {
            this.MaxAnimals = maxAnimals;
            this.RoomId = roomId;
            this.EnclosureTypeId = enclosureTypeId;
            this.AnimalTypeId = animalTypeId;
        }
    }
}
