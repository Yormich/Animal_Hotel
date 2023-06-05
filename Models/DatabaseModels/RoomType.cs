using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace Animal_Hotel.Models.DatabaseModels
{

    [Table("room_type")]
    public class RoomType
    {
        [Key]
        public short Id { get; set; }

        [Required]
        [StringLength(maximumLength: 40, ErrorMessage = "Type should not be empty")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Column("preferred_user_type_id")]
        [ForeignKey("PreferredEmployeeType")]
        public short PreferredEmployeeTypeId{ get; set; }

        public UserType? PreferredEmployeeType { get; set; }

        public List<Room>? Rooms { get; set; }

        public RoomType(string type, short preferredEmployeeTypeId)
        {
            this.Type = type;
            this.PreferredEmployeeTypeId = preferredEmployeeTypeId;
        }

        public RoomType() { }
    }
}
