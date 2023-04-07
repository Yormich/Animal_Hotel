using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("user_type")]
    public class UserType
    {
        [Key]
        public short Id { get; set; }

        [Required]
        [Column("Name")]
        [StringLength(maximumLength: 20, ErrorMessage = "User type length should be betwwen 4 and 20 characters", MinimumLength = 4)]
        public string Name { get; set; }

        public List<UserLoginInfo> UserLoginInfos { get; set; } = new();

        public RoomType? RoomType { get; set; }

        public UserType(string name)
        {
            this.Name = name;
        }
    }
}
