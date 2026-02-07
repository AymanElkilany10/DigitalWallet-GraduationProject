using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.MoneyRequest;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;
using System.Linq;

namespace DigitalWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MoneyRequestController : BaseController
    {
        private readonly IMoneyRequestService _moneyRequestService;
        private readonly ILogger<MoneyRequestController> _logger;

        public MoneyRequestController(
            IMoneyRequestService moneyRequestService,
            ILogger<MoneyRequestController> logger)
        {
            _moneyRequestService = moneyRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new money request directed at another user.
        /// The service layer prevents users from requesting money from themselves.
        /// </summary>
        /// <param name="request">Target user identifier, amount, and currency.</param>
        /// <returns>MoneyRequestDto with pending status.</returns>
        /// <response code="201">Request created.</response>
        /// <response code="400">Target not found, self-request, or validation error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<MoneyRequestDto>>> CreateRequest([FromBody] CreateMoneyRequestDto request)
        {
            // ── Input validation ──────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(request.ToUserPhoneOrEmail))
                return BadRequest(ApiResponse<MoneyRequestDto>.ErrorResponse("Target user phone or email is required."));

            if (request.Amount <= 0)
                return BadRequest(ApiResponse<MoneyRequestDto>.ErrorResponse("Amount must be greater than zero."));

            if (string.IsNullOrWhiteSpace(request.CurrencyCode))
                request.CurrencyCode = "EGP"; // default fallback

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("CreateMoneyRequest by UserId: {UserId}, Target: {Target}, Amount: {Amount}",
                currentUserId, request.ToUserPhoneOrEmail, request.Amount);

            var result = await _moneyRequestService.CreateRequestAsync(currentUserId, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("CreateMoneyRequest failed for UserId: {UserId}. Errors: {Errors}",
                    currentUserId, string.Join(", ", result.Errors ?? new List<string>()));
                return BadRequest(ApiResponse<MoneyRequestDto>.ErrorResponse(
                    result.ErrorMessage ?? "Failed to create request", result.Errors));
            }

            return CreatedAtAction(
                nameof(GetSentRequests),
                null,
                ApiResponse<MoneyRequestDto>.SuccessResponse(result.Data!, result.Message ?? "Success"));
        }

        /// <summary>
        /// Lists all money requests that the authenticated user has sent.
        /// </summary>
        /// <returns>Collection of MoneyRequestDto (sent).</returns>
        /// <response code="200">List retrieved.</response>
        [HttpGet("sent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<MoneyRequestDto>>>> GetSentRequests()
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Fetching sent requests for UserId: {UserId}", currentUserId);

            var result = await _moneyRequestService.GetSentRequestsAsync(currentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Lists all money requests that have been sent TO the authenticated user (pending responses).
        /// </summary>
        /// <returns>Collection of MoneyRequestDto (received).</returns>
        /// <response code="200">List retrieved.</response>
        [HttpGet("received")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<MoneyRequestDto>>>> GetReceivedRequests()
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Fetching received requests for UserId: {UserId}", currentUserId);

            var result = await _moneyRequestService.GetReceivedRequestsAsync(currentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Accepts or rejects a money request addressed to the authenticated user.
        /// Accepting requires a valid OTP; rejection does not.
        /// The service layer verifies that the request belongs to the caller and is still pending.
        /// </summary>
        /// <param name="request">RequestId, Accept flag, and (if accepting) OTP code.</param>
        /// <returns>Boolean success indicator.</returns>
        /// <response code="200">Request responded to.</response>
        /// <response code="400">
        /// Request not found, already processed, OTP missing/invalid, or insufficient balance.
        /// </response>
        [HttpPut("respond")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> RespondToRequest([FromBody] AcceptRejectRequestDto request)
        {
            if (request.RequestId == Guid.Empty)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Request ID is required."));

            // OTP is mandatory only when accepting
            if (request.Accept && (string.IsNullOrWhiteSpace(request.OtpCode) || request.OtpCode.Length != 6))
                return BadRequest(ApiResponse<bool>.ErrorResponse("A valid 6-digit OTP code is required to accept a request."));

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("RespondToRequest by UserId: {UserId}, RequestId: {RequestId}, Accept: {Accept}",
                currentUserId, request.RequestId, request.Accept);

            var result = await _moneyRequestService.AcceptOrRejectRequestAsync(currentUserId, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("RespondToRequest failed for UserId: {UserId}. Errors: {Errors}",
                    currentUserId, string.Join(", ", result.Errors ?? new List<string>()));
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    result.ErrorMessage ?? "Failed to respond to request", result.Errors));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(result.Data!, result.Message ?? "Success"));
        }
    }
}