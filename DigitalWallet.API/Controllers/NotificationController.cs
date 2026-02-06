using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Notification;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Returns a paginated list of all notifications for the authenticated user,
        /// ordered from newest to oldest.
        /// </summary>
        /// <param name="pageNumber">1-based page index (default 1).</param>
        /// <param name="pageSize">Items per page (default 20, max 100).</param>
        /// <returns>Collection of NotificationDto.</returns>
        /// <response code="200">Notifications retrieved.</response>
        /// <response code="400">Invalid pagination parameters.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageNumber < 1)
                return BadRequest(ApiResponse<IEnumerable<NotificationDto>>.ErrorResponse("Page number must be at least 1."));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<IEnumerable<NotificationDto>>.ErrorResponse("Page size must be between 1 and 100."));

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Fetching notifications for UserId: {UserId}, Page: {Page}, Size: {Size}",
                currentUserId, pageNumber, pageSize);

            var result = await _notificationService.GetUserNotificationsAsync(currentUserId, pageNumber, pageSize);
            return HandleResult(result);
        }

        /// <summary>
        /// Marks a single notification as read.
        /// </summary>
        /// <param name="notificationId">The notification to mark.</param>
        /// <returns>Boolean success flag.</returns>
        /// <response code="200">Notification marked as read.</response>
        /// <response code="400">Invalid notificationId.</response>
        [HttpPut("{notificationId:guid}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead([FromRoute] Guid notificationId)
        {
            if (notificationId == Guid.Empty)
                return BadRequest(ApiResponse<bool>.ErrorResponse("A valid Notification ID is required."));

            _logger.LogInformation("Marking notification {NotificationId} as read.", notificationId);

            var result = await _notificationService.MarkAsReadAsync(notificationId);
            return HandleResult(result, "Notification marked as read");
        }

        /// <summary>
        /// Returns the count of unread notifications for the authenticated user.
        /// Useful for badge counters on the mobile / web UI.
        /// </summary>
        /// <returns>Integer unread count.</returns>
        /// <response code="200">Count retrieved.</response>
        [HttpGet("unread-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Fetching unread notification count for UserId: {UserId}", currentUserId);

            var result = await _notificationService.GetUnreadCountAsync(currentUserId);
            return HandleResult(result);
        }
    }
}
