using CurrencyConversion.Models.Base;
using CurrencyConversion.Models.Currency;
using CurrencyConversion.Models.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Services.CurrencyExchange
{
    public interface ICurrencyExchangeService
    {
        Task<APIResponse<CurrencyRates>> GetLatestCurrencyExchangeRates(BaseParamModel baseParamModel);
        Task<APIResponse<double>> GetCurrencyExchangeRate(CurrencyExchangeModel currencyExchangeModel);
        Task<APIResponse<string>> GetCurrencyExchangeRateHistory(CurrencyHistoryParameterModel currencyParameterModel);
    }
}
