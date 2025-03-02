using CurrencyConversion.Controllers;
using CurrencyConversion.Models.Base;
using CurrencyConversion.Models.Currency;
using CurrencyConversion.Models.Parameters;
using CurrencyConversion.Services.CurrencyExchange;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace CurrencyConversion.UnitTest
{
    [TestClass]
    public class CurrencyConversionControllerTests
    {
        private Mock<ICurrencyExchangeService> _mockCurrencyExchangeService;
        private Mock<ILogger<CurrencyConversionController>> _mockLogger;
        private CurrencyConversionController _controller;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<CurrencyConversionController>>();
            _mockCurrencyExchangeService = new Mock<ICurrencyExchangeService>();
            _controller = new CurrencyConversionController(_mockCurrencyExchangeService.Object, _mockLogger.Object);
        }


        [Test]
        public async Task GetLatestCurrencyExchangeRates_ReturnsOkResult()
        {
            // Arrange
            var baseParamModel = new BaseParamModel { ProviderName = "FrankFurter", BaseCurrency = "" };
            var apiResponse = new APIResponse<CurrencyRates> { Success = true, Data = new CurrencyRates() };
            _mockCurrencyExchangeService.Setup(service => service.GetLatestCurrencyExchangeRates(baseParamModel)).ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetLatestCurrencyExchangeRates(baseParamModel) as OkObjectResult;

            // Assert

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetCurrencyExchangeRate_ReturnsBadRequest_WhenBaseOrToCurrencyIsEmpty()
        {
            // Arrange
            var currencyExchangeModel = new CurrencyExchangeModel { BaseCurrency = "", ToCurrency = "EUR" };

            // Act
            var badRequestResult = await _controller.GetCurrencyExchangeRate(currencyExchangeModel) as BadRequestObjectResult;

            Assert.Equals("Please provide the base and to currency", badRequestResult.Value);
        }

        [Test]
        public async Task GetCurrencyExchangeRate_ReturnsOkResult()
        {
            // Arrange
            var currencyExchangeModel = new CurrencyExchangeModel { BaseCurrency = "USD", ToCurrency = "EUR", Amount = 1 };
            var apiResponse = new APIResponse<double> { Success = true, Data = 0.84825 };
            _mockCurrencyExchangeService.Setup(service => service.GetCurrencyExchangeRate(currencyExchangeModel)).ReturnsAsync(apiResponse);

            // Act
            var okResult = await _controller.GetCurrencyExchangeRate(currencyExchangeModel) as OkObjectResult;
            var returnValue = (APIResponse<double>)okResult.Value;
            // Assert
            Assert.IsTrue(returnValue.Success);
            Assert.Equals(apiResponse.Data, returnValue.Data);
        }
    }
}