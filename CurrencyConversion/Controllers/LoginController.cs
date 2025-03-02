using Asp.Versioning;
using CurrencyConversion.Models.Base;
using CurrencyConversion.Services.CurrencyExchange;
using CurrencyConversion.Services.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConversion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LoginController : ControllerBase
    {
        private readonly IUserLogin _userLogin;
        private readonly ILogger<LoginController> _logger;
        public LoginController(IUserLogin userLogin, ILogger<LoginController> logger)
        {
            _userLogin = userLogin;
            _logger = logger;
        }

        [HttpPost]
        [ApiVersion("1.0")]
        [Route("login/v{version:apiversion}")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var response = new APIResponse<string> { Success = false };
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    return BadRequest(response.Message = "Please provide the username and password");

                var token = await _userLogin.LogUser(username, password);

                if (string.IsNullOrEmpty(token))
                    return Unauthorized(response.Message = "Invalid username or password");

                response.Success = true;
                response.Message = "Login successful";
                response.Data = token;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while logging in";
                response.Data = null;
                _logger.LogError(ex, "Error in Login");
            }
            return Ok(response);
        }
    }
}
