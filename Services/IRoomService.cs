using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IRoomService
    {
        public Task<int> GetRoomsCountAsync();
        public Task<IQueryable<Room>> GetRoomWithEnclosure(short id);

        public Task<IQueryable<Room>> GetRoomsWithEnclosures(short id);

        public Task<IQueryable<Room>> GetRoomsByPageIndex(int pageIndex, int pageSize);
    }
}
