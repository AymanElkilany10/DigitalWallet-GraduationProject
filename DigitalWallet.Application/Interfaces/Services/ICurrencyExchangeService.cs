using DigitalWallet.Application.DTOs.Exchange;
using DigitalWallet.Application.Common;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface ICurrencyExchangeService
    {
        Task<ServiceResult<ExchangeResponseDto>> ExchangeCurrencyAsync(ExchangeRequestDto request);
        Task<ServiceResult<ExchangeRateDto>> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        Task<ServiceResult<List<ExchangeRateDto>>> GetAllExchangeRatesAsync(string baseCurrency);
        Task<ServiceResult<List<ExchangeResponseDto>>> GetUserExchangeHistoryAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
        Task<ServiceResult<bool>> UpdateExchangeRatesAsync();
    }
}