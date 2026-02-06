using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user account and creates a default EGP wallet.
        /// </summary>
        /// <param name="request">Registration details (name, email, phone, password).</param>
        /// <returns>Token, refresh token, and user summary on success.</returns>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Validation failed or email/phone already exists.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            var result = await _authService.RegisterAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}",
                    request.Email, string.Join(", ", result.Errors ?? Array.Empty<string>()));

                return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse(
                    result.Errors ?? new List<string> { "Registration failed" }));
            }

            _logger.LogInformation("Registration successful for UserId: {UserId}", result.Data?.UserId);

            return CreatedAtAction(
                nameof(Register),
                null,
                ApiResponse<LoginResponseDto>.SuccessResponse(result.Data!, result.Message));
        }

        /// <summary>
        /// Authenticates a user using email or phone number plus password.
        /// Returns a token and flags whether OTP verification is still required.
        /// </summary>
        /// <param name="request">Login credentials (email/phone and password).</param>
        /// <returns>Token and login metadata on success.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="400">Invalid credentials or account suspended.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt for identifier: {Identifier}", request.EmailOrPhone);

            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login failed for identifier: {Identifier}. Errors: {Errors}",
                    request.EmailOrPhone, string.Join(", ", result.Errors ?? Array.Empty<string>()));

                return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse(
                    result.Errors ?? new List<string> { "Login failed" }));
            }

            _logger.LogInformation("Login successful for UserId: {UserId}", result.Data?.UserId);

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result.Data!, result.Message));
        }

        /// <summary>
        /// Verifies a 6-digit OTP code sent to the user after login or for sensitive operations.
        /// </summary>
        /// <param name="request">UserId and the OTP code to verify.</param>
        /// <returns>Success flag indicating OTP is valid.</returns>
        /// <response code="200">OTP verified.</response>
        /// <response code="400">OTP invalid, expired, or already used.</response>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            if (request.UserId == Guid.Empty)
                return BadRequest(ApiResponse<bool>.ErrorResponse("User ID is required."));

            if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length != 6)
                return BadRequest(ApiResponse<bool>.ErrorResponse("OTP code must be exactly 6 digits."));

            _logger.LogInformation("OTP verification attempt for UserId: {UserId}", request.UserId);

            var result = await _authService.VerifyOtpAsync(request);
            return HandleResult(result, "OTP verified successfully");
        }

        /// <summary>
        /// Generates a new OTP for a given user and purpose (login or transfer).
        /// In development, the OTP code is returned directly in the response.
        /// </summary>
        /// <param name="userId">Target user's unique identifier.</param>
        /// <param name="type">OTP purpose: "login" or "transfer".</param>
        /// <returns>The generated OTP code (dev-only).</returns>
        /// <response code="200">OTP generated.</response>
        /// <response code="400">Invalid parameters or generation failed.</response>
        [HttpPost("generate-otp/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> GenerateOtp(
            [FromRoute] Guid userId,
            [FromQuery] string type = "login")
        {
            if (userId == Guid.Empty)
                return BadRequest(ApiResponse<string>.ErrorResponse("Valid User ID is required."));

            var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "login", "transfer" };
            if (!allowedTypes.Contains(type))
                return BadRequest(ApiResponse<string>.ErrorResponse("Type must be 'login' or 'transfer'."));

            // In production you would verify the authenticated user matches userId
            // or has admin privileges before generating OTP on behalf of another user.
            var currentUserId = GetCurrentUserId();
            if (currentUserId != userId)
                return Forbid("You can only generate OTP for your own account.");

            _logger.LogInformation("OTP generation requested for UserId: {UserId}, Type: {Type}", userId, type);

            var result = await _authService.GenerateOtpAsync(userId, type);
            return HandleResult(result, "OTP generated successfully");
        }

        /// <summary>
        /// Refreshes an authentication token using a valid refresh token.
        /// </summary>
        /// <param name="request">The refresh token to validate.</param>
        /// <returns>New token pair on success.</returns>
        /// <response code="200">Token refreshed.</response>
        /// <response code="400">Refresh token invalid or expired.</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse("Refresh token is required."));

            // TODO: Implement full refresh-token rotation logic in AuthService.
            // This stub returns an error until the service method is wired up.
            _logger.LogWarning("RefreshToken endpoint called but full logic is not yet implemented.");
            return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse("Refresh token feature is pending implementation."));
        }
    }
}
