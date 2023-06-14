using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Models.ViewModels
{
    public class ReceptionistReport
    {
        public int Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int Period { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public string ClientPhone { get; set; } = string.Empty;

        public long AnimalId { get; set; }

        public long EnclosureId { get; set; }

        public Animal? Animal { get; set; }

        public AnimalEnclosure? Enclosure { get; set; }

        public long ContractId { get; set; }
    }
}
