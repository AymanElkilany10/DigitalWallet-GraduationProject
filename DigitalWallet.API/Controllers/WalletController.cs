using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Wallet;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : BaseController
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Returns every wallet that belongs to the currently authenticated user.
        /// </summary>
        /// <returns>List of WalletDto for the caller.</returns>
        /// <response code="200">Wallets retrieved (may be an empty list).</response>
        [HttpGet("my-wallets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<WalletDto>>>> GetMyWallets()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching wallets for UserId: {UserId}", userId);

            var result = await _walletService.GetUserWalletsAsync(userId);
            return HandleResult(result);
        }

        /// <summary>
        /// Returns the details of a single wallet identified by its GUID.
        /// The wallet must belong to the authenticated user.
        /// </summary>
        /// <param name="walletId">The wallet's unique identifier.</param>
        /// <returns>Single WalletDto.</returns>
        /// <response code="200">Wallet found.</response>
        /// <response code="400">walletId is empty.</response>
        /// <response code="404">No wallet with that ID, or it does not belong to you.</response>
        [HttpGet("{walletId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<WalletDto>>> GetWalletById([FromRoute] Guid walletId)
        {
            if (walletId == Guid.Empty)
                return BadRequest(ApiResponse<WalletDto>.ErrorResponse("A valid Wallet ID is required."));

            var result = await _walletService.GetWalletByIdAsync(walletId);

            if (!result.IsSuccess)
                return NotFound(ApiResponse<WalletDto>.ErrorResponse("Wallet not found."));

            // Ownership check — the wallet must belong to the logged-in user
            var currentUserId = GetCurrentUserId();
            if (result.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} tried to access WalletId {WalletId} owned by {OwnerId}.",
                    currentUserId, walletId, result.Data.UserId);
                return NotFound(ApiResponse<WalletDto>.ErrorResponse("Wallet not found."));
                // Return 404 (not 403) to avoid leaking the existence of the wallet.
            }

            return Ok(ApiResponse<WalletDto>.SuccessResponse(result.Data));
        }

        /// <summary>
        /// Creates a new wallet for the authenticated user with a specified currency.
        /// Only one wallet per currency is allowed per user.
        /// </summary>
        /// <param name="request">Currency code for the new wallet (default "EGP").</param>
        /// <returns>The newly created WalletDto.</returns>
        /// <response code="201">Wallet created.</response>
        /// <response code="400">Duplicate currency wallet or validation error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<WalletDto>>> CreateWallet([FromBody] CreateWalletRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            // Always force the UserId to the authenticated caller to prevent spoofing
            request.UserId = currentUserId;

            if (string.IsNullOrWhiteSpace(request.CurrencyCode))
                return BadRequest(ApiResponse<WalletDto>.ErrorResponse("Currency code is required."));

            // Normalise currency to upper-case
            request.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();

            _logger.LogInformation("Creating wallet for UserId: {UserId}, Currency: {Currency}",
                currentUserId, request.CurrencyCode);

            var result = await _walletService.CreateWalletAsync(request);
            return HandleCreatedResult(result, nameof(GetWalletById), new { walletId = result.Data?.Id },
                "Wallet created successfully");
        }

        /// <summary>
        /// Returns the current balance of a specific wallet.
        /// The wallet must belong to the authenticated user.
        /// </summary>
        /// <param name="walletId">The wallet to query.</param>
        /// <returns>WalletBalanceDto with balance and currency.</returns>
        /// <response code="200">Balance retrieved.</response>
        /// <response code="400">Invalid walletId.</response>
        /// <response code="404">Wallet not found or not owned by you.</response>
        [HttpGet("{walletId:guid}/balance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<WalletBalanceDto>>> GetBalance([FromRoute] Guid walletId)
        {
            if (walletId == Guid.Empty)
                return BadRequest(ApiResponse<WalletBalanceDto>.ErrorResponse("A valid Wallet ID is required."));

            // First verify ownership via the full wallet lookup
            var walletResult = await _walletService.GetWalletByIdAsync(walletId);
            if (!walletResult.IsSuccess)
                return NotFound(ApiResponse<WalletBalanceDto>.ErrorResponse("Wallet not found."));

            var currentUserId = GetCurrentUserId();
            if (walletResult.Data!.UserId != currentUserId)
                return NotFound(ApiResponse<WalletBalanceDto>.ErrorResponse("Wallet not found."));

            var result = await _walletService.GetBalanceAsync(walletId);
            return HandleResult(result);
        }
    }
}
