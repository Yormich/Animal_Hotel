using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public interface IRoomService
    {
        public Task<int> GetRoomsCountAsync(bool withClosedToBook = false);

        public Task<Room> GetRoomBaseInfoById(short roomId);

        public Task<List<Room>> GetRoomsByPageIndex(int pageIndex, int pageSize, bool withClosedToBook = false);

        public Task<List<Room>> GetManagerRoomsByPageIndex(int pageIndex, int pageSize, bool withClosedToBook = true);

        public Task<(bool success, string? message)> UpdateRoom(Room updatedRoom);

        public Task<short> CreateRoom(Room newRoom);

        public Task UpdateRoomPhoto(short roomId, string photoPath);

        public Task<Room> GetManagerRoomInfo(short roomId);

        public Task<(bool success, string? message)> RemoveRoom(short roomId);

        public Task<List<RoomType>> GetRoomTypes();
    }
}
