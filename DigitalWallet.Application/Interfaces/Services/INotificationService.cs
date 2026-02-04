using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Notification;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<ServiceResult<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(
            Guid userId, int pageNumber = 1, int pageSize = 20);
        Task<ServiceResult<bool>> MarkAsReadAsync(Guid notificationId);
        Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId);
    }
}