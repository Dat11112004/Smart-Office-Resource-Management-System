using SORMS.API.DTOs;

namespace SORMS.API.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetNotificationsForResidentAsync(int residentId);
        Task<NotificationDto> CreateNotificationAsync(NotificationDto notificationDto);
        Task<bool> MarkAsReadAsync(int notificationId);
    }

}
