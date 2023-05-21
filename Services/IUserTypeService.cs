using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IUserTypeService
    {
        public Task<UserType> GetUserTypeByUserId(long userId);
    }
}
