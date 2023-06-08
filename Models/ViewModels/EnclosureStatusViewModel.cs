using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.ViewModels
{
    public class EnclosureStatusViewModel
    {
        public long Id { get; set; }

        [Column("has_bookings")]
        public bool HasBookings { get; set; }

        [Column("has_active_contracts")]
        public bool HasActiveContracts { get; set; }
    }
}
