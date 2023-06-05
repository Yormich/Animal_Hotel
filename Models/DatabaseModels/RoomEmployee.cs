using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("room_employee")]
    public class RoomEmployee
    {
        [Column("room_id")]
        [ForeignKey("Room")]
        public short RoomId { get; set; }

        public Room? Room { get; set; }
        
        [Column("employee_id")]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public RoomEmployee(short roomId, long employeeId)
        {
            this.RoomId = roomId;
            this.EmployeeId = employeeId;
        }

        public RoomEmployee() { }
    }
}
