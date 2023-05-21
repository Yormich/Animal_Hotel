using Animal_Hotel.Models.DatabaseModels;
using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class ClientDataViewModel : UserViewModel
    {
        [CreditCard(ErrorMessage = "Enter correct card number")]
        public string? CardNumber { get; set; }

        public PaginatedList<Animal>? Animals { get; set; }

        public Animal? ActiveAnimal { get; set; }

        public ClientDataViewModel(UserViewModel user) 
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

        public ClientDataViewModel() { }
    }
}
