using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class EmployeeBaseViewModel : UserViewModel
    {
        public PaginatedList<Request>? Requests { get; set; }

        public Request? ActiveRequest { get; set; }

        public EmployeeBaseViewModel(UserViewModel user)
        {
            this.UserId = user.UserId;
            this.SubUserId = user.SubUserId;
            this.Login = user.Login;
            this.PhoneNumber = user.PhoneNumber;
            this.UserType = user.UserType;
            this.BirthDate = user.BirthDate;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.PhotoPath = user.PhotoPath;
            this.Actions = user.Actions;
        }

        public EmployeeBaseViewModel() { }
    }
}
