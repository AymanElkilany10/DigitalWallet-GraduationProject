using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Transfer;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : BaseController
    {
        private readonly ITransferService _transferService;
        private readonly IWalletService _walletService;
        private readonly ILogger<TransferController> _logger;

        public TransferController(
            ITransferService transferService,
            IWalletService walletService,
            ILogger<TransferController> logger)
        {
            _transferService = transferService;
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Sends money from the authenticated user's wallet to another user
        /// identified by phone number or email.  Requires a valid OTP in the payload.
        /// </summary>
        /// <param name="request">
        /// SenderWalletId, receiver identifier, amount, optional description, and OTP code.
        /// </param>
        /// <returns>TransferResponseDto with confirmation details.</returns>
        /// <response code="200">Transfer completed.</response>
        /// <response code="400">Validation failure, insufficient balance, or invalid OTP.</response>
        /// <response code="403">Sender wallet does not belong to you.</response>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<TransferResponseDto>>> SendMoney([FromBody] SendMoneyRequestDto request)
        {
            // ── Basic field validation ────────────────────────────────────────
            if (request.SenderWalletId == Guid.Empty)
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("Sender Wallet ID is required."));

            if (string.IsNullOrWhiteSpace(request.ReceiverPhoneOrEmail))
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("Receiver phone or email is required."));

            if (request.Amount <= 0)
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("Amount must be greater than zero."));

            if (string.IsNullOrWhiteSpace(request.OtpCode) || request.OtpCode.Length != 6)
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("A valid 6-digit OTP code is required."));

            // ── Ownership guard ──────────────────────────────────────────────
            var currentUserId = GetCurrentUserId();
            var walletResult = await _walletService.GetWalletByIdAsync(request.SenderWalletId);

            if (!walletResult.IsSuccess)
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("Sender wallet not found."));

            if (walletResult.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to send from WalletId {WalletId} owned by {OwnerId}.",
                    currentUserId, request.SenderWalletId, walletResult.Data.UserId);
                return Forbid("You can only transfer from your own wallet.");
            }

            // ── Execute transfer ──────────────────────────────────────────────
            _logger.LogInformation("SendMoney initiated by UserId: {UserId}, Amount: {Amount}, Receiver: {Receiver}",
                currentUserId, request.Amount, request.ReceiverPhoneOrEmail);

            var result = await _transferService.SendMoneyAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("SendMoney failed for UserId: {UserId}. Errors: {Errors}",
                    currentUserId, string.Join(", ", result.Errors ?? Array.Empty<string>()));
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse(
                    result.Errors ?? new List<string> { "Transfer failed" }));
            }

            _logger.LogInformation("SendMoney succeeded. TransferId: {TransferId}",
                result.Data?.TransferId);

            return Ok(ApiResponse<TransferResponseDto>.SuccessResponse(result.Data!, result.Message));
        }

        /// <summary>
        /// Lists all transfers (sent and received) across every wallet the caller owns.
        /// </summary>
        /// <returns>Collection of TransferDto.</returns>
        /// <response code="200">Transfers retrieved (may be empty).</response>
        [HttpGet("my-transfers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransferDto>>>> GetMyTransfers()
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Fetching transfers for UserId: {UserId}", currentUserId);

            var result = await _transferService.GetUserTransfersAsync(currentUserId);
            return HandleResult(result);
        }
    }
}
