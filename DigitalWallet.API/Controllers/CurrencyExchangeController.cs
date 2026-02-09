using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalWallet.Application.DTOs.Exchange;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CurrencyExchangeController : BaseController
    {
        private readonly ICurrencyExchangeService _exchangeService;
        private readonly ILogger<CurrencyExchangeController> _logger;

        public CurrencyExchangeController(
            ICurrencyExchangeService exchangeService,
            ILogger<CurrencyExchangeController> logger)
        {
            _exchangeService = exchangeService;
            _logger = logger;
        }

        /// <summary>
        /// Exchange currency between user's wallets
        /// </summary>
        [HttpPost("exchange")]
        public async Task<ActionResult<ApiResponse<ExchangeResponseDto>>> ExchangeCurrency([FromBody] ExchangeRequestDto request)
        {
            _logger.LogInformation(
                "Currency exchange requested from wallet {FromWallet} to {ToWallet}",
                request.FromWalletId,
                request.ToWalletId);

            var result = await _exchangeService.ExchangeCurrencyAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Get current exchange rate between two currencies
        /// </summary>
        [HttpGet("rate")]
        public async Task<ActionResult<ApiResponse<ExchangeRateDto>>> GetExchangeRate(
            [FromQuery] string from,
            [FromQuery] string to)
        {
            var result = await _exchangeService.GetExchangeRateAsync(from, to);
            return HandleResult(result);
        }

        /// <summary>
        /// Get all exchange rates for a base currency
        /// </summary>
        [HttpGet("rates/{baseCurrency}")]
        public async Task<ActionResult<ApiResponse<List<ExchangeRateDto>>>> GetAllRates(string baseCurrency)
        {
            var result = await _exchangeService.GetAllExchangeRatesAsync(baseCurrency);
            return HandleResult(result);
        }

        /// <summary>
        /// Get current user's exchange history
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<ExchangeResponseDto>>>> GetExchangeHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var result = await _exchangeService.GetUserExchangeHistoryAsync(userId, pageNumber, pageSize);
            return HandleResult(result);
        }

        /// <summary>
        /// Update all exchange rates (Admin only)
        /// </summary>
        [HttpPost("update-rates")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateExchangeRates()
        {
            _logger.LogInformation("Updating exchange rates");
            var result = await _exchangeService.UpdateExchangeRatesAsync();
            return HandleResult(result);
        }
    }
}
