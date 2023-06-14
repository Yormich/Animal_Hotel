using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Services;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class AnimalWatcherViewModel : EmployeeBaseViewModel
    {
        public PaginatedList<Animal>? Animals { get; set; }

        public Animal? ActiveAnimal { get; set; }

        public List<Room>? Rooms { get; set; }

        public AnimalWatcherViewModel(UserViewModel user) : base(user)
        {
        }

        public AnimalWatcherViewModel() { }
    }
}
