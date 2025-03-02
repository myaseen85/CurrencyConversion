using CurrencyConversion.Models.Configurations;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Services.Login
{
    public class UserLogin : IUserLogin
    {
        private readonly CurrencyConversionAppSettingConfiguration _currencyConversionAppSettingConfiguration;
        public UserLogin(CurrencyConversionAppSettingConfiguration currencyConversionAppSettingConfiguration)
        {
            _currencyConversionAppSettingConfiguration = currencyConversionAppSettingConfiguration;
        }

        public async Task<string> LogUser(string username, string password)
        {
            if (ValidateUser(username, password))
                return GetAccessToken(username);
            else
                return string.Empty;
        }

        private string GetAccessToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_currencyConversionAppSettingConfiguration.JwtSetttings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", userName), new Claim(ClaimTypes.Role, $"{userName}") }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _currencyConversionAppSettingConfiguration.JwtSetttings.Issuer,
                Audience = _currencyConversionAppSettingConfiguration.JwtSetttings.Issuer // Set the audience claim
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private bool ValidateUser(string username, string password)
        {
            return true;
        }
    }
}
