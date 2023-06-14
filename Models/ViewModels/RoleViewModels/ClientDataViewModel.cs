using Animal_Hotel.Models.DatabaseModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.ViewModels.RoleViewModels
{
    public class ClientDataViewModel : UserViewModel
    {
        [Column("card_number")]
        [CreditCard(ErrorMessage = "Enter correct card number")]
        public string? CardNumber { get; set; }

        [NotMapped]
        public PaginatedList<Animal>? Animals { get; set; }

        [NotMapped]
        public IQueryable<AnimalType>? AnimalTypes { get; set; }

        [NotMapped]
        public Animal? ActiveAnimal { get; set; }

        [NotMapped]
        public Review? HotelReview { get; set; }

        [NotMapped]
        public bool HasFinishedContracts { get; set; }

        [NotMapped]
        public Booking? ActiveBooking { get; set; }

        [NotMapped]
        public PaginatedList<Booking>? Bookings { get; set; }


        [NotMapped]
        public Room? ActiveRoom { get; set; }

        [NotMapped]
        public AnimalEnclosure? ActiveEnclosure { get; set; }


        [Column("registered_since")]
        public DateTime RegisteredSince { get; set; }

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
