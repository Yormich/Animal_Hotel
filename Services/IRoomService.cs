using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IRoomService
    {
        public Task<int> GetRoomsCountAsync(bool withClosedToBook = false);

        public Task<Room> GetRoomBaseInfoById(short roomId);

        public Task<List<Room>> GetRoomsByPageIndex(int pageIndex, int pageSize, bool withClosedToBook = false);

        public Task<List<Room>> GetManagerRoomsByPageIndex(int pageIndex, int pageSize, bool withClosedToBook = true);

        public Task<bool> IsRoomHasAnyActiveContractsOrBookings(short roomId);

        public Task UpdateRoom(Room updatedRoom);

        public Task<short> CreateRoom(Room newRoom);

        public Task<Room> GetManagerRoomInfo(short roomId);

        public Task RemoveRoom(short roomId);

        public Task<List<RoomType>> GetRoomTypes();

        public Task RemoveNotPreferrableEmployees(short roomId);
    }
}
