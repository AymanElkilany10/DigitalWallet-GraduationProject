using System.Net.Http;
using Newtonsoft.Json;

namespace DigitalWallet.Application.Services
{
    public interface IExternalExchangeRateService
    {
        Task<decimal?> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        Task<Dictionary<string, decimal>?> GetAllRatesAsync(string baseCurrency);
    }

    public class ExternalExchangeRateService : IExternalExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "https://api.exchangerate-api.com/v4/latest/";

        public ExternalExchangeRateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal?> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{API_BASE_URL}{fromCurrency}");
                var data = JsonConvert.DeserializeObject<ExchangeRateApiResponse>(response);

                if (data?.Rates != null && data.Rates.ContainsKey(toCurrency))
                {
                    return (decimal)data.Rates[toCurrency];
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Dictionary<string, decimal>?> GetAllRatesAsync(string baseCurrency)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{API_BASE_URL}{baseCurrency}");
                var data = JsonConvert.DeserializeObject<ExchangeRateApiResponse>(response);

                if (data?.Rates != null)
                {
                    return data.Rates.ToDictionary(
                        kvp => kvp.Key,
                        kvp => (decimal)kvp.Value
                    );
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private class ExchangeRateApiResponse
        {
            public string? Base { get; set; }
            public Dictionary<string, double>? Rates { get; set; }
        }
    }
}