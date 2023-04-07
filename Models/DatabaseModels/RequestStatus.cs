using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{

    [Table("request_status")]
    public class RequestStatus
    {
        [Key]
        public short Id { get; set; }

        [Required]
        [StringLength(maximumLength:30, ErrorMessage = "Status length should be between 4 and 30 characters", MinimumLength = 4)]
        public string Status { get; set; }

        public List<Request> Requests { get; set; } = new();

        public RequestStatus(string status)
        {
            this.Status = status;
        }
    }
}
