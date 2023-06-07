using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class HotelManagerViewModel : EmployeeBaseViewModel
    {
        public PaginatedList<EmployeeDataViewModel>? Employees { get; set; }

        public PaginatedList<Room>? Rooms { get; set; }

        public Room? ActiveRoom { get; set; }
        public EmployeeDataViewModel? ActiveEmployee { get; set; }

        public AnimalEnclosure? ActiveEnclosure { get; set; }
        
        public List<RoomType>? RoomTypes { get; set; }
 

        public EmployeeRegisterModel? NewEmployee { get; set; }

        public List<RequestStatus>? RequestStatuses { get; set; }

        public List<Employee>? SuitableEmployees { get; set; }

        public RoomEmployee? NewRoomEmployee { get; set; }

        public List<AnimalType>? AnimalTypes { get; set; }

        public List<EnclosureType>? EnclosureTypes { get; set; }

        public HotelManagerViewModel(UserViewModel user) : base(user)
        {
        }

        public HotelManagerViewModel() { }
    }
}
