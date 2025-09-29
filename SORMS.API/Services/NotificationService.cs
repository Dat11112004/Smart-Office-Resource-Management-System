namespace SORMS.API.Services
{
    using Microsoft.EntityFrameworkCore;
    using SORMS.API.Data;
    using SORMS.API.DTOs;
    using SORMS.API.Interfaces;
    using SORMS.API.Models;

    public class NotificationService : INotificationService
    {
        private readonly SormsDbContext _context;

        public NotificationService(SormsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsForResidentAsync(int residentId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.ResidentId == residentId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                IsRead = n.IsRead,
                ResidentId = n.ResidentId
            });
        }

        public async Task<NotificationDto> CreateNotificationAsync(NotificationDto notificationDto)
        {
            var notification = new Notification
            {
                Message = notificationDto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                ResidentId = notificationDto.ResidentId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            notificationDto.Id = notification.Id;
            notificationDto.IsRead = false;
            return notificationDto;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null || notification.IsRead)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
