using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class HotelManagerViewModel : EmployeeBaseViewModel
    {
        public PaginatedList<EmployeeDataViewModel>? Employees { get; set; }

        public EmployeeDataViewModel? ActiveEmployee { get; set; }

        public EmployeeRegisterModel? NewEmployee { get; set; }

        public List<RequestStatus>? RequestStatuses { get; set; }

        public HotelManagerViewModel(UserViewModel user) : base(user)
        {
        }

        public HotelManagerViewModel() { }
    }
}
