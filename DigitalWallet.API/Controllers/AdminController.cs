using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Admin-only endpoints.  Access is restricted via the "Admin" authorization policy
    /// defined in ServiceCollectionExtensions.  A non-admin token will receive 403.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Returns all registered users across the platform.
        /// Intended for the admin dashboard.
        /// </summary>
        /// <returns>Collection of UserManagementDto.</returns>
        /// <response code="200">User list retrieved.</response>
        /// <response code="403">Caller does not have admin privileges.</response>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserManagementDto>>>> GetAllUsers()
        {
            _logger.LogInformation("Admin: Fetching all users.");

            var result = await _adminService.GetAllUsersAsync();

            if (!result.IsSuccess)
            {
                _logger.LogError("Admin GetAllUsers failed. Errors: {Errors}",
                    string.Join(", ", result.Errors ?? Array.Empty<string>()));
                return BadRequest(ApiResponse<IEnumerable<UserManagementDto>>.ErrorResponse(
                    result.Errors ?? new List<string> { "Failed to retrieve users" }));
            }

            return Ok(ApiResponse<IEnumerable<UserManagementDto>>.SuccessResponse(result.Data!));
        }

        /// <summary>
        /// Returns the most recent fraud-log entries (last 24 hours by default, controlled in the service).
        /// </summary>
        /// <returns>Collection of FraudLogDto.</returns>
        /// <response code="200">Fraud logs retrieved.</response>
        /// <response code="403">Caller does not have admin privileges.</response>
        [HttpGet("fraud-logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<FraudLogDto>>>> GetFraudLogs()
        {
            _logger.LogInformation("Admin: Fetching recent fraud logs.");

            var result = await _adminService.GetFraudLogsAsync();

            if (!result.IsSuccess)
            {
                _logger.LogError("Admin GetFraudLogs failed. Errors: {Errors}",
                    string.Join(", ", result.Errors ?? Array.Empty<string>()));
                return BadRequest(ApiResponse<IEnumerable<FraudLogDto>>.ErrorResponse(
                    result.Errors ?? new List<string> { "Failed to retrieve fraud logs" }));
            }

            return Ok(ApiResponse<IEnumerable<FraudLogDto>>.SuccessResponse(result.Data!));
        }

        /// <summary>
        /// Health-check / connectivity probe for the admin panel.
        /// Returns basic server metadata; useful for monitoring dashboards.
        /// </summary>
        /// <returns>Object with timestamp and environment.</returns>
        /// <response code="200">Server is healthy.</response>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<object>> Health()
        {
            var healthData = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };

            return Ok(ApiResponse<object>.SuccessResponse(healthData, "Service is healthy"));
        }
    }
}
