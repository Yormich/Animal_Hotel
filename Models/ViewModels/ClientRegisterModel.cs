using System.ComponentModel.DataAnnotations;

namespace Animal_Hotel.Models.ViewModels
{
    public class ClientRegisterModel : RegisterViewModel
    {
        [CreditCard(ErrorMessage = "Enter correct card number")]
        public string? CardNumber { get; set; }
        [StringLength(40, MinimumLength = 5)]
        public string? PhotoPath { get; set; }
    }
}
