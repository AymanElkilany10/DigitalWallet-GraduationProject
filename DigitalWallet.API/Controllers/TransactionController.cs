using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Transaction;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : BaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly IWalletService _walletService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ITransactionService transactionService,
            IWalletService walletService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Returns a single transaction by its ID.
        /// The transaction's wallet must belong to the authenticated user.
        /// </summary>
        /// <param name="transactionId">Unique identifier of the transaction.</param>
        /// <returns>TransactionDto if found and owned by the caller.</returns>
        /// <response code="200">Transaction found.</response>
        /// <response code="400">Invalid transactionId.</response>
        /// <response code="404">Transaction does not exist or does not belong to you.</response>
        [HttpGet("{transactionId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransaction([FromRoute] Guid transactionId)
        {
            if (transactionId == Guid.Empty)
                return BadRequest(ApiResponse<TransactionDto>.ErrorResponse("A valid Transaction ID is required."));

            _logger.LogInformation("Fetching transaction {TransactionId}", transactionId);

            var result = await _transactionService.GetTransactionByIdAsync(transactionId);

            if (!result.IsSuccess)
                return NotFound(ApiResponse<TransactionDto>.ErrorResponse("Transaction not found."));

            // Ownership: verify the wallet referenced by this transaction belongs to the caller
            var currentUserId = GetCurrentUserId();
            var walletResult = await _walletService.GetWalletByIdAsync(result.Data!.WalletId);

            if (!walletResult.IsSuccess || walletResult.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to view transaction {TxnId} belonging to another user.",
                    currentUserId, transactionId);
                return NotFound(ApiResponse<TransactionDto>.ErrorResponse("Transaction not found."));
            }

            return Ok(ApiResponse<TransactionDto>.SuccessResponse(result.Data));
        }

        /// <summary>
        /// Returns a paginated list of transactions for a given wallet.
        /// The wallet must belong to the authenticated user.
        /// </summary>
        /// <param name="walletId">Wallet whose transactions are requested.</param>
        /// <param name="pageNumber">1-based page index (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 20, max 100).</param>
        /// <returns>PaginatedResult containing TransactionDto items.</returns>
        /// <response code="200">Page of transactions retrieved.</response>
        /// <response code="400">Invalid parameters.</response>
        /// <response code="404">Wallet not found or not owned by you.</response>
        [HttpGet("wallet/{walletId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaginatedResult<TransactionDto>>>> GetWalletTransactions(
            [FromRoute] Guid walletId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            // ── Input validation ──────────────────────────────────────────────
            if (walletId == Guid.Empty)
                return BadRequest(ApiResponse<PaginatedResult<TransactionDto>>.ErrorResponse("A valid Wallet ID is required."));

            if (pageNumber < 1)
                return BadRequest(ApiResponse<PaginatedResult<TransactionDto>>.ErrorResponse("Page number must be at least 1."));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<PaginatedResult<TransactionDto>>.ErrorResponse("Page size must be between 1 and 100."));

            // ── Ownership check ──────────────────────────────────────────────
            var currentUserId = GetCurrentUserId();
            var walletResult = await _walletService.GetWalletByIdAsync(walletId);

            if (!walletResult.IsSuccess)
                return NotFound(ApiResponse<PaginatedResult<TransactionDto>>.ErrorResponse("Wallet not found."));

            if (walletResult.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} tried to list transactions of WalletId {WalletId} owned by {OwnerId}.",
                    currentUserId, walletId, walletResult.Data.UserId);
                return NotFound(ApiResponse<PaginatedResult<TransactionDto>>.ErrorResponse("Wallet not found."));
            }

            // ── Fetch paginated transactions ─────────────────────────────────
            _logger.LogInformation("Fetching transactions for WalletId: {WalletId}, Page: {Page}, Size: {Size}",
                walletId, pageNumber, pageSize);

            var result = await _transactionService.GetWalletTransactionsAsync(walletId, pageNumber, pageSize);
            return HandleResult(result);
        }
    }
}
