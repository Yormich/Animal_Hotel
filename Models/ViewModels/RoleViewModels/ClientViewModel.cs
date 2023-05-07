using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class ClientViewModel : UserViewModel
    {
        public long ClientId { get; set; }
        [CreditCard(ErrorMessage = "Enter correct card number")]
        public string? CardNumber { get; set; }
    }
}
