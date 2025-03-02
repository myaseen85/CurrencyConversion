using CurrencyConversion.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Services.Factory
{
    public interface ICurrencyConversionFactory
    {
        ICurrencyConversionProvider GetCurrencyConversionProvider(string providerName);
    }
}
