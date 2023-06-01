using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("room")]
    public class Room
    {
        [Key]
        public short Id { get; set; }

        [Required]
        [Column("photo_name")]
        [StringLength(maximumLength:60, ErrorMessage = "Photo file name length should be less than 60 characters")]
        public string PhotoPath { get; set; }

        [Required]
        [Column("room_type_id")]
        [ForeignKey("RoomType")]
        public short RoomTypeId { get; set; }

        public RoomType? RoomType { get; set; }

        public List<Employee> Employees { get; set; } = new();

        public List<RoomEmployee> RoomEmployees { get; set; } = new();

        public List<AnimalEnclosure> Enclosures { get; set; } = new();

        [Required]
        [Column("unable_to_book")]
        public bool UnableToBook { get; set; }

        public Room(string photoPath, short roomTypeId)
        {
            this.PhotoPath = photoPath;
            this.RoomTypeId = roomTypeId;
        }
    }
}
