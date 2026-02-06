using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the authenticated user's profile fetched by their JWT-embedded user ID.
        /// </summary>
        /// <returns>The current user's DTO.</returns>
        /// <response code="200">Profile retrieved.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User record not found in database.</response>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserManagementDto>>> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching profile for UserId: {UserId}", userId);

            var result = await _userService.GetUserByIdAsync(userId);

            if (!result.IsSuccess)
                return NotFound(ApiResponse<UserManagementDto>.ErrorResponse("User profile not found."));

            return Ok(ApiResponse<UserManagementDto>.SuccessResponse(result.Data!));
        }

        /// <summary>
        /// Retrieves a user profile by their unique ID.
        /// Only the owner of the account may access this endpoint; 403 is returned otherwise.
        /// </summary>
        /// <param name="userId">Target user's GUID.</param>
        /// <returns>The requested user's DTO.</returns>
        /// <response code="200">Profile retrieved.</response>
        /// <response code="400">Invalid GUID format.</response>
        /// <response code="403">Authenticated user is not the owner.</response>
        /// <response code="404">No user with the given ID exists.</response>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserManagementDto>>> GetUserById([FromRoute] Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest(ApiResponse<UserManagementDto>.ErrorResponse("A valid User ID is required."));

            // Privacy guard: only allow fetching your own profile unless you are an admin.
            var currentUserId = GetCurrentUserId();
            if (currentUserId != userId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to access profile of UserId {TargetId}.",
                    currentUserId, userId);
                return Forbid("You are only allowed to view your own profile.");
            }

            _logger.LogInformation("Fetching profile for UserId: {UserId}", userId);

            var result = await _userService.GetUserByIdAsync(userId);

            if (!result.IsSuccess)
                return NotFound(ApiResponse<UserManagementDto>.ErrorResponse("User not found."));

            return Ok(ApiResponse<UserManagementDto>.SuccessResponse(result.Data!));
        }

        /// <summary>
        /// Looks up a user by email address.
        /// Intended for internal / admin use; regular users should use /me.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>Matching user DTO if found.</returns>
        /// <response code="200">User found.</response>
        /// <response code="400">Email parameter missing or empty.</response>
        /// <response code="404">No user with the given email.</response>
        [HttpGet("by-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserManagementDto>>> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ApiResponse<UserManagementDto>.ErrorResponse("Email parameter is required."));

            // Basic format sanity check
            if (!email.Contains('@'))
                return BadRequest(ApiResponse<UserManagementDto>.ErrorResponse("Invalid email format."));

            _logger.LogInformation("Searching user by email: {Email}", email);

            var result = await _userService.GetUserByEmailAsync(email);

            if (!result.IsSuccess)
                return NotFound(ApiResponse<UserManagementDto>.ErrorResponse("User with the specified email was not found."));

            return Ok(ApiResponse<UserManagementDto>.SuccessResponse(result.Data!));
        }
    }
}
