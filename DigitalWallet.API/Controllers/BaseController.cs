using Microsoft.AspNetCore.Mvc;
using DigitalWallet.Application.Common;
using System.Security.Claims;

namespace DigitalWallet.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Extracts the authenticated user's ID from the JWT claims.
        /// Throws UnauthorizedAccessException if the claim is missing or invalid.
        /// </summary>
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid user identifier in token.");

            return userId;
        }

        /// <summary>
        /// Wraps a successful ServiceResult into an appropriate HTTP response.
        /// </summary>
        protected ActionResult<ApiResponse<T>> HandleResult<T>(ServiceResult<T> result, string? successMessage = null)
        {
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<T>.ErrorResponse(result.Errors ?? new List<string> { "Unknown error" }));

            return Ok(ApiResponse<T>.SuccessResponse(result.Data!, successMessage ?? result.Message));
        }

        /// <summary>
        /// Wraps a successful ServiceResult into a CreatedAtAction response (201).
        /// </summary>
        protected ActionResult<ApiResponse<T>> HandleCreatedResult<T>(ServiceResult<T> result, string actionName, object? routeValues = null, string? successMessage = null)
        {
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<T>.ErrorResponse(result.Errors ?? new List<string> { "Unknown error" }));

            return CreatedAtAction(actionName, routeValues, ApiResponse<T>.SuccessResponse(result.Data!, successMessage ?? result.Message));
        }
    }
}
