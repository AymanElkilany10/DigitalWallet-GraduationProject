using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Notification;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;

namespace DigitalWallet.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(
            Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(
                    userId, pageNumber, pageSize);
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
                return ServiceResult<IEnumerable<NotificationDto>>.Success(notificationDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<NotificationDto>>.Failure(
                    $"Error retrieving notifications: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResult<bool>.Success(true, "Notification marked as read");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure(
                    $"Error marking notification as read: {ex.Message}");
            }
        }

        public async Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var count = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Failure(
                    $"Error getting unread count: {ex.Message}");
            }
        }
    }
}