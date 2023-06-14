using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("contract")]
    public class Contract
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("date_of_leaving")]
        public DateTime EndDate { get; set; }

        [Column("check_out_date")]
        public DateTime? CheckOutDate { get; set; }

        [Required]
        [Column("animal_id")]
        [ForeignKey("Animal")]
        public long AnimalId { get; set; }

        public Animal? Animal { get; set; }

        [Required]
        [Column("enclosure_id")]
        [ForeignKey("Enclosure")]
        public long EnclosureId { get; set; }

        public AnimalEnclosure? Enclosure { get; set; }

        [Column("full_price")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal FullPrice { get; set; }

        [Column("actually_paid")]
        public decimal? ActuallyPaid { get; set; }

        public Contract(DateTime startDate, DateTime endDate, long animalId, long enclosureId, decimal? actuallyPaid = null)
        {   
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.AnimalId = animalId;
            this.EnclosureId = enclosureId;
            this.ActuallyPaid = actuallyPaid;
        }

        public Contract() { }
    }
}
