using SORMS.API.DTOs;

namespace SORMS.API.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync();
        Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(); // ✅ Thêm dòng này
        Task<RoomDto> GetRoomByIdAsync(int id);
        Task<RoomDto> CreateRoomAsync(RoomDto roomDto);
        Task<bool> UpdateRoomAsync(int id, RoomDto roomDto);
        Task<bool> DeleteRoomAsync(int id);
    }
}
