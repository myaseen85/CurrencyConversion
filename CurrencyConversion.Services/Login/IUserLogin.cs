using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Services.Login
{
    public interface IUserLogin
    {
        Task<string> LogUser(string username, string password);
    }
}
