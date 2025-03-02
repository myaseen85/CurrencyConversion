using Asp.Versioning;
using CurrencyConversion.Models.Base;
using CurrencyConversion.Models.Currency;
using CurrencyConversion.Models.Parameters;
using CurrencyConversion.Services.CurrencyExchange;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CurrencyConversion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyConversionController : ControllerBase
    {
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly ILogger<CurrencyConversionController> _logger;
        public CurrencyConversionController(ICurrencyExchangeService currencyExchangeService, ILogger<CurrencyConversionController> logger)
        {
            _currencyExchangeService = currencyExchangeService;
            _logger = logger;
        }

        [HttpPost]
        [ApiVersion("1.0")]
        [Route("latest/v{version:apiversion}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> GetLatestCurrencyExchangeRates(BaseParamModel baseParamModel)
        {
            var response = new APIResponse<CurrencyRates>();
            try
            {
                response = await _currencyExchangeService.GetLatestCurrencyExchangeRates(baseParamModel);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while fetching currency rates";
                response.Data = null;
                _logger.LogError(ex, "Error in GetLatestCurrencyExchangeRates");
            }
            return Ok(response);
        }

        [HttpPost]
        [ApiVersion("1.0")]
        [Route("rate/v{version:apiversion}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetCurrencyExchangeRate(CurrencyExchangeModel currencyExchangeModel)
        {
            var response = new APIResponse<double> { Success = false };
            try
            {
                if (string.IsNullOrEmpty(currencyExchangeModel.BaseCurrency) || string.IsNullOrEmpty(currencyExchangeModel.ToCurrency))
                    return BadRequest(response.Message = "Please provide the base and to currency");

                if (Constant.Constants.ExcludedCurrency.Any(x => x.Equals(currencyExchangeModel.BaseCurrency, StringComparison.InvariantCultureIgnoreCase) || x.Equals(currencyExchangeModel.ToCurrency, StringComparison.InvariantCultureIgnoreCase)))
                    return BadRequest(response.Message = "The Provided Base/To Currency is not support!");

                response = await _currencyExchangeService.GetCurrencyExchangeRate(currencyExchangeModel);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while fetching currency rates";
                _logger.LogError(ex, "Error in GetCurrencyExchangeRateHistory");
            }
            return Ok(response);
        }

        [HttpPost]
        [ApiVersion("1.0")]
        [Route("history/v{version:apiversion}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCurrencyExchangeRateHistory(CurrencyHistoryParameterModel currencyParameterModel)
        {
            var response = new APIResponse<string>();
            try
            {
                response = await _currencyExchangeService.GetCurrencyExchangeRateHistory(currencyParameterModel);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while fetching currency rates";
                response.Data = null;
                _logger.LogError(ex, "Error in GetCurrencyExchangeRateHistory");
            }

            return Ok(response);
        }
    }
}
