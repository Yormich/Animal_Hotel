using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("room")]
    public class Room
    {
        [Key]
        public short Id { get; set; }

        [Column("photo_name")]
        [StringLength(maximumLength:60, ErrorMessage = "Photo file name length should be less than 60 characters")]
        public string PhotoPath { get; set; } = string.Empty;

        [Required]
        [Column("room_type_id")]
        [ForeignKey("RoomType")]
        public short RoomTypeId { get; set; }

        public RoomType? RoomType { get; set; }

        public List<Employee>? Employees { get; set; }

        public List<RoomEmployee>? RoomEmployees { get; set; }

        public List<AnimalEnclosure>? Enclosures { get; set; }

        [Required]
        [Column("unable_to_book")]
        public bool UnableToBook { get; set; }


        [NotMapped]
        public int ResponsibleEmployeesCount { get; set; }

        [NotMapped]
        public int AvailableEnclosuresAmount { get; set; }


        public Room(string photoPath, short roomTypeId)
        {
            this.PhotoPath = photoPath;
            this.RoomTypeId = roomTypeId;
        }

        public Room() { }
    }
}
