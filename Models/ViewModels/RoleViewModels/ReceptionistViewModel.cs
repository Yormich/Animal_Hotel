using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class ReceptionistViewModel : EmployeeBaseViewModel
    {
        public List<ReceptionistReport>? BaseReports { get; set; } 

        public DateTime TargetDate { get; set; }

        public List<Contract>? Contracts { get; set; }

        public Contract? ActiveContract { get; set; }


        public PaginatedList<Client>? Clients { get; set; }

        public Client? ActiveClient { get; set; }

        public List<Animal>? Animals { get; set; }

        public List<AnimalEnclosure>? Enclosures { get; set; }

        public Animal? ActiveAnimal { get; set; }

        public long SelectedAnimalId { get; set; }

        public List<AnimalType>? AnimalTypes { get; set; }

        public ReceptionistViewModel(UserViewModel user) : base(user)
        {
        }

        public ReceptionistViewModel() { }
    }
}
