using CurrencyConversion.Models.Base;
using CurrencyConversion.Models.Currency;
using CurrencyConversion.Models.Parameters;
using CurrencyConversion.Services.Factory;
using Newtonsoft.Json;

namespace CurrencyConversion.Services.CurrencyExchange
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly ICurrencyConversionFactory _currencyConversionFactory;
        public CurrencyExchangeService(ICurrencyConversionFactory currencyConversionFactory)
        {
            _currencyConversionFactory = currencyConversionFactory;
        }
        public async Task<APIResponse<CurrencyRates>> GetLatestCurrencyExchangeRates(BaseParamModel baseParamModel)
        {
            var currencyConversionProvider = _currencyConversionFactory.GetCurrencyConversionProvider(baseParamModel.ProviderName);
            if (currencyConversionProvider == null)
                throw new ArgumentException("Invalid provider name");

            var apiResponse = new APIResponse<CurrencyRates> { Success = false };

            var response = await currencyConversionProvider.GetLatestCurrencyExchangeRates(baseParamModel.BaseCurrency);
            if (!string.IsNullOrEmpty(response))
            {
                apiResponse.Data = JsonConvert.DeserializeObject<CurrencyRates>(response);
                apiResponse.Success = true;
                apiResponse.Message = "latest rates are fetched successfully";
            }
            else
            {
                apiResponse.Success = false;
                apiResponse.Message = "latest rates are fetched successfully";
            }

            return apiResponse;
        }

        public async Task<APIResponse<double>> GetCurrencyExchangeRate(CurrencyExchangeModel currencyExchangeModel)
        {
            var currencyConversionProvider = _currencyConversionFactory.GetCurrencyConversionProvider(currencyExchangeModel.ProviderName);
            if (currencyConversionProvider == null)
                throw new Exception("Invalid provider name");

            var apiResponse = new APIResponse<double> { Success = false };
            var response = await currencyConversionProvider.GetCurrencyExchangeRate(currencyExchangeModel.BaseCurrency, currencyExchangeModel.ToCurrency, currencyExchangeModel.Amount);
            if (!string.IsNullOrEmpty(response))
            {
                var data = JsonConvert.DeserializeObject<ExchangeRateResponse>(response);
                data.Rates.TryGetValue(currencyExchangeModel.ToCurrency, out double exchangeRate);
                apiResponse.Data = exchangeRate * currencyExchangeModel.Amount;
                apiResponse.Success = true;
                apiResponse.Message = $"Exchange rate {exchangeRate}, From Currency=> {currencyExchangeModel.BaseCurrency},To Currency=> {currencyExchangeModel.ToCurrency}, Provided Amount=>{currencyExchangeModel.Amount}  fetched successfully";
            }
            else
            {
                apiResponse.Success = false;
                apiResponse.Message = "Unable to find the exchange rate for the provided currency";
                apiResponse.Data = 0;
            }

            return apiResponse;
        }

        public async Task<APIResponse<string>> GetCurrencyExchangeRateHistory(CurrencyHistoryParameterModel currencyParameterModel)
        {
            var currencyConversionProvider = _currencyConversionFactory.GetCurrencyConversionProvider(currencyParameterModel.ProviderName);
            if (currencyConversionProvider == null)
                throw new ArgumentException("Invalid provider name");

            var apiResponse = new APIResponse<string> { Success = false };

            var response = await currencyConversionProvider.GetCurrencyExchangeRateHistory(currencyParameterModel.BaseCurrency, currencyParameterModel.StartDate, currencyParameterModel.EndDate);
            if (!string.IsNullOrEmpty(response))
            {
                apiResponse.Data = response;
                apiResponse.Success = true;
                apiResponse.Message = "Exchange rate history fetched successfully";
            }
            else
            {
                apiResponse.Success = false;
                apiResponse.Message = "Unable to find the exchange rate history for the provided currency";
            }

            return apiResponse;
        }
    }
}
