using CurrencyConversion.Models;
using CurrencyConversion.Models.Base;
using CurrencyConversion.Models.Configurations;
using CurrencyConversion.Models.Currency;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyConversion.Providers
{
    public class FrankFurterCurrencyConversionProvider : ICurrencyConversionProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CurrencyConversionAppSettingConfiguration _currencyConversionAppSettingConfiguration;

        public FrankFurterCurrencyConversionProvider(IHttpClientFactory httpClientFactory, CurrencyConversionAppSettingConfiguration currencyConversionAppSettingConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            _currencyConversionAppSettingConfiguration = currencyConversionAppSettingConfiguration;
        }

        public async Task<string> GetCurrencyExchangeRate(string fromCurrency, string toCurrency, double amount)
        {
            var httpClient = _httpClientFactory.CreateClient("FrankfurterProviderClient");
            httpClient.BaseAddress = new Uri(_currencyConversionAppSettingConfiguration.FrankFurterCurrencyConversionProviderSetting.BaseUrl);
            string parameters = string.Format("latest?base={0}&symbols={1}", fromCurrency, toCurrency);

            var response = await httpClient.GetAsync(parameters);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else
                return string.Empty;
        }
        public async Task<string> GetCurrencyExchangeRateHistory(string baseCurrency, string startDate, string endDate)
        {
            var httpClient = _httpClientFactory.CreateClient("FrankfurterProviderClient");
            httpClient.BaseAddress = new Uri(_currencyConversionAppSettingConfiguration.FrankFurterCurrencyConversionProviderSetting.BaseUrl);
            string parameters = $"{startDate}..{endDate}";

            if (!string.IsNullOrEmpty(baseCurrency))
                parameters = $"{parameters}?base={baseCurrency}";

            var response = await httpClient.GetAsync(parameters);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else
                return string.Empty;
        }
        public async Task<string> GetLatestCurrencyExchangeRates(string baseCurrency)
        {
            var parameters = string.Empty;
            var httpClient = _httpClientFactory.CreateClient("FrankfurterProviderClient");
            httpClient.BaseAddress = new Uri(_currencyConversionAppSettingConfiguration.FrankFurterCurrencyConversionProviderSetting.BaseUrl);

            if (string.IsNullOrEmpty(baseCurrency))
                parameters = "latest";
            else
                parameters = string.Format("latest?base={0}", baseCurrency);

            var response = await httpClient.GetAsync(parameters);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else
                return string.Empty;
        }
    }
}
