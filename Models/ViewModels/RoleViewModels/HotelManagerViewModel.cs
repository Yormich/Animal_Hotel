using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class HotelManagerViewModel : EmployeeBaseViewModel
    {
        public PaginatedList<EmployeeDataViewModel>? Employees { get; set; }

        public EmployeeDataViewModel? ActiveEmployee { get; set; }

        public HotelManagerViewModel(UserViewModel user) : base(user)
        {
        }
    }
}
