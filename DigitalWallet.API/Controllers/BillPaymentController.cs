using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.BillPayment;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BillPaymentController : BaseController
    {
        private readonly IBillPaymentService _billPaymentService;
        private readonly IWalletService _walletService;
        private readonly ILogger<BillPaymentController> _logger;

        public BillPaymentController(
            IBillPaymentService billPaymentService,
            IWalletService walletService,
            ILogger<BillPaymentController> logger)
        {
            _billPaymentService = billPaymentService;
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Returns all active billers available for payment.
        /// This endpoint is read-only and does not require ownership checks.
        /// </summary>
        /// <returns>Collection of BillerDto.</returns>
        /// <response code="200">Biller list retrieved.</response>
        [HttpGet("billers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BillerDto>>>> GetBillers()
        {
            _logger.LogInformation("Fetching all active billers.");

            var result = await _billPaymentService.GetAllBillersAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Pays a bill by deducting the specified amount from the caller's wallet.
        /// Requires a valid OTP, and the wallet referenced in the request must belong to the caller.
        /// </summary>
        /// <param name="request">WalletId, BillerId, Amount, and OTP code.</param>
        /// <returns>BillPaymentDto with receipt path on success.</returns>
        /// <response code="200">Bill paid successfully.</response>
        /// <response code="400">Validation failure, invalid OTP, or insufficient balance.</response>
        /// <response code="403">Wallet does not belong to the caller.</response>
        [HttpPost("pay")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<BillPaymentDto>>> PayBill([FromBody] PayBillRequestDto request)
        {
            // ── Input validation ──────────────────────────────────────────────
            if (request.WalletId == Guid.Empty)
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResponse("Wallet ID is required."));

            if (request.BillerId == Guid.Empty)
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResponse("Biller ID is required."));

            if (request.Amount <= 0)
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResponse("Payment amount must be greater than zero."));

            if (string.IsNullOrWhiteSpace(request.OtpCode) || request.OtpCode.Length != 6)
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResponse("A valid 6-digit OTP code is required."));

            // Ensure only digits in OTP
            if (!request.OtpCode.All(char.IsDigit))
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResponse("OTP code must contain only digits."));

            // ── Ownership guard ──────────────────────────────────────────────
            var currentUserId = GetCurrentUserId();
            var walletResult = await _walletService.GetWalletByIdAsync(request.WalletId);

            if (!walletResult.IsSuccess)
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResponse("Wallet not found."));

            if (walletResult.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to pay bill using WalletId {WalletId} owned by {OwnerId}.",
                    currentUserId, request.WalletId, walletResult.Data.UserId);
                return Forbid("You can only pay bills from your own wallet.");
            }

            // ── Execute payment ───────────────────────────────────────────────
            _logger.LogInformation("PayBill initiated by UserId: {UserId}, BillerId: {BillerId}, Amount: {Amount}",
                currentUserId, request.BillerId, request.Amount);

            var result = await _billPaymentService.PayBillAsync(currentUserId, request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("PayBill failed for UserId: {UserId}. Errors: {Errors}",
                    currentUserId, string.Join(", ", result.Errors ?? Array.Empty<string>()));
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResponse(
                    result.Errors ?? new List<string> { "Bill payment failed" }));
            }

            _logger.LogInformation("PayBill succeeded for UserId: {UserId}, PaymentId: {PaymentId}",
                currentUserId, result.Data?.Id);

            return Ok(ApiResponse<BillPaymentDto>.SuccessResponse(result.Data!, result.Message));
        }

        /// <summary>
        /// Returns all bill payments made by the authenticated user.
        /// </summary>
        /// <returns>Collection of BillPaymentDto.</returns>
        /// <response code="200">Payment history retrieved (may be empty).</response>
        [HttpGet("my-payments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BillPaymentDto>>>> GetMyPayments()
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Fetching bill payments for UserId: {UserId}", currentUserId);

            var result = await _billPaymentService.GetUserBillPaymentsAsync(currentUserId);
            return HandleResult(result);
        }
    }
}
