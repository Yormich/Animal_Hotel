using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("client_review")]
    public class Review
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Range(1, 10)]
        public short Rating { get; set; }

        [Required]
        [StringLength(maximumLength: 300, MinimumLength = 6, ErrorMessage = "Please write a comment (length should be between 6 and 300 characters)")]
        public string Comment { get; set; } = string.Empty;

        [Required]
        [Column("writing_date")]
        [DataType(DataType.Date)]
        public DateTime WritingDate { get; set; }

        [Required]
        [Column("client_id")]
        [ForeignKey("Client")]
        public long ClientId { get; set; }

        public Client? Client { get; set; }

        public Review(short rating, string comment, long clientId)
        {
            this.Rating = rating;
            this.Comment = comment;
            this.ClientId = clientId;
        }

        public Review() { }
    }
}
