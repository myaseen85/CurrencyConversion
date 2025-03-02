using CurrencyConversion.Models.Configurations;
using CurrencyConversion.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Services.Factory
{
    public class CurrencyConversionFactory : ICurrencyConversionFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CurrencyConversionAppSettingConfiguration _currencyConversionAppSettingConfiguration;
        public CurrencyConversionFactory(IHttpClientFactory httpClientFactory, CurrencyConversionAppSettingConfiguration currencyConversionAppSettingConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            _currencyConversionAppSettingConfiguration = currencyConversionAppSettingConfiguration;
        }

        public ICurrencyConversionProvider GetCurrencyConversionProvider(string providerName)
        {
            ICurrencyConversionProvider currencyConversionProvider = null;
            switch (providerName)
            {
                case "FrankFurter":
                    currencyConversionProvider = new FrankFurterCurrencyConversionProvider(_httpClientFactory, _currencyConversionAppSettingConfiguration);
                    break;
                default:
                    break;
            }

            return currencyConversionProvider;
        }
    }
}
