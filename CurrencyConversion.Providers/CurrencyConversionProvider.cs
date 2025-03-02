using CurrencyConversion.Models;
using CurrencyConversion.Models.Base;
using CurrencyConversion.Models.Currency;

namespace CurrencyConversion.Providers
{
    public interface ICurrencyConversionProvider
    {
        public Task<string> GetLatestCurrencyExchangeRates(string baseCurrency);
        public Task<string> GetCurrencyExchangeRate(string fromCurrency, string toCurrency, double amount);
        public Task<string> GetCurrencyExchangeRateHistory(string baseCurrency, string startDate, string endDate);
    }
}
