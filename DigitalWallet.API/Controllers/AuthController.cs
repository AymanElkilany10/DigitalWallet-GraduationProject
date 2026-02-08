using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Handles user authentication operations including registration, login, and OTP verification
    /// </summary>
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
        /// Registers a new user account
        /// </summary>
        /// <param name="request">Registration details including full name, email, phone, and password</param>
        /// <returns>Login response with JWT token</returns>
        /// <response code="200">Registration successful, returns token</response>
        /// <response code="400">Validation errors or email/phone already exists</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            var result = await _authService.RegisterAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Authenticates a user and sends OTP for verification
        /// </summary>
        /// <param name="request">Login credentials (email or phone) and password</param>
        /// <returns>Partial login response requiring OTP verification</returns>
        /// <response code="200">OTP sent successfully</response>
        /// <response code="400">Invalid credentials or account suspended</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt for: {EmailOrPhone}", request.EmailOrPhone);

            var result = await _authService.LoginAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Verifies OTP code and completes login process
        /// </summary>
        /// <param name="request">User ID and OTP code</param>
        /// <returns>Confirmation of OTP verification</returns>
        /// <response code="200">OTP verified successfully</response>
        /// <response code="400">Invalid or expired OTP</response>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            _logger.LogInformation("OTP verification for UserId: {UserId}", request.UserId);
            var result = await _authService.VerifyOtpAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Sends a new OTP code to the user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="otpType">Type of OTP (Login, Transfer, etc.)</param>
        /// <returns>Confirmation that OTP was sent</returns>
        /// <response code="200">OTP sent successfully</response>
        /// <response code="400">User not found</response>
        /// <response code="401">Authentication required</response>
        [HttpPost("send-otp/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> SendOtp(Guid userId, [FromQuery] string otpType = "Login")
        {
            var currentUserId = GetCurrentUserId();

            // Users can only request OTP for themselves
            if (userId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to request OTP for {TargetId}", currentUserId, userId);
                return Forbid("You can only request OTP for your own account.");
            }

            _logger.LogInformation("Sending OTP to UserId: {UserId}, Type: {OtpType}", userId, otpType);

            var result = await _authService.SendOtpAsync(userId, otpType);
            return HandleResult(result);
        }
    }
}