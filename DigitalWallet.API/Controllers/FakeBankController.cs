using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.FakeBank;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;
using System.Linq;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Simulates a bank gateway for deposit and withdrawal operations.
    /// All monetary operations include a short artificial delay to mimic real bank processing.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FakeBankController : BaseController
    {
        private readonly IFakeBankService _fakeBankService;
        private readonly ILogger<FakeBankController> _logger;

        public FakeBankController(IFakeBankService fakeBankService, ILogger<FakeBankController> logger)
        {
            _fakeBankService = fakeBankService;
            _logger = logger;
        }

        /// <summary>
        /// Deposits funds from the simulated external bank account into the user's wallet.
        /// The amount is deducted from the FakeBankAccount and credited to the first wallet.
        /// </summary>
        /// <param name="request">UserId and the deposit Amount.</param>
        /// <returns>FakeBankTransactionDto with status and simulated delay info.</returns>
        /// <response code="200">Deposit completed successfully.</response>
        /// <response code="400">
        /// Insufficient bank balance, amount out of range, or UserId mismatch.
        /// </response>
        /// <response code="403">Caller is not the target user.</response>
        [HttpPost("deposit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<FakeBankTransactionDto>>> Deposit([FromBody] DepositRequestDto request)
        {
            // ── Input validation ──────────────────────────────────────────────
            if (request.UserId == Guid.Empty)
                return BadRequest(ApiResponse<FakeBankTransactionDto>.ErrorResponse("User ID is required."));

            if (request.Amount <= 0)
                return BadRequest(ApiResponse<FakeBankTransactionDto>.ErrorResponse("Deposit amount must be greater than zero."));

            if (request.Amount > 100_000m)
                return BadRequest(ApiResponse<FakeBankTransactionDto>.ErrorResponse("Deposit amount cannot exceed 100,000."));

            // ── Ownership guard ──────────────────────────────────────────────
            var currentUserId = GetCurrentUserId();
            if (currentUserId != request.UserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to deposit on behalf of UserId {TargetId}.",
                    currentUserId, request.UserId);
                return Forbid("You can only perform deposits into your own account.");
            }

            _logger.LogInformation("Deposit initiated by UserId: {UserId}, Amount: {Amount}",
                request.UserId, request.Amount);

            var result = await _fakeBankService.DepositAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Deposit failed for UserId: {UserId}. Errors: {Errors}",
                    request.UserId, string.Join(", ", result.Errors ?? Array.Empty<string>()));
                return BadRequest(ApiResponse<FakeBankTransactionDto>.ErrorResponse(
                    result.Errors?.FirstOrDefault() ?? "Deposit failed", result.Errors ?? Array.Empty<string>()));
            }

            _logger.LogInformation("Deposit succeeded for UserId: {UserId}, TxnId: {TxnId}",
                request.UserId, result.Data?.Id);

            return Ok(ApiResponse<FakeBankTransactionDto>.SuccessResponse(result.Data!, result.Message ?? "Success"));
        }

        /// <summary>
        /// Withdraws funds from the user's wallet and credits the simulated bank account.
        /// </summary>
        /// <param name="request">UserId and the withdrawal Amount.</param>
        /// <returns>FakeBankTransactionDto with status and simulated delay info.</returns>
        /// <response code="200">Withdrawal completed successfully.</response>
        /// <response code="400">
        /// Insufficient wallet balance, amount out of range, or UserId mismatch.
        /// </response>
        /// <response code="403">Caller is not the target user.</response>
        [HttpPost("withdraw")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<FakeBankTransactionDto>>> Withdraw([FromBody] WithdrawRequestDto request)
        {
            // ── Input validation ──────────────────────────────────────────────
            if (request.UserId == Guid.Empty)
                return BadRequest(ApiResponse<FakeBankTransactionDto>.ErrorResponse("User ID is required."));

            if (request.Amount <= 0)
                return BadRequest(ApiResponse<FakeBankTransactionDto>.ErrorResponse("Withdrawal amount must be greater than zero."));

            if (request.Amount > 100_000m)
                return BadRequest(ApiResponse<FakeBankTransactionDto>.ErrorResponse("Withdrawal amount cannot exceed 100,000."));

            // ── Ownership guard ──────────────────────────────────────────────
            var currentUserId = GetCurrentUserId();
            if (currentUserId != request.UserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to withdraw on behalf of UserId {TargetId}.",
                    currentUserId, request.UserId);
                return Forbid("You can only perform withdrawals from your own account.");
            }

            _logger.LogInformation("Withdraw initiated by UserId: {UserId}, Amount: {Amount}",
                request.UserId, request.Amount);

            var result = await _fakeBankService.WithdrawAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Withdraw failed for UserId: {UserId}. Errors: {Errors}",
                    request.UserId, string.Join(", ", result.Errors ?? Array.Empty<string>()));
                return BadRequest(ApiResponse<FakeBankTransactionDto>.ErrorResponse(
                    result.Errors?.FirstOrDefault() ?? "Withdrawal failed", result.Errors ?? Array.Empty<string>()));
            }

            _logger.LogInformation("Withdraw succeeded for UserId: {UserId}, TxnId: {TxnId}",
                request.UserId, result.Data?.Id);

            return Ok(ApiResponse<FakeBankTransactionDto>.SuccessResponse(result.Data!, result.Message ?? "Success"));
        }
    }
}
