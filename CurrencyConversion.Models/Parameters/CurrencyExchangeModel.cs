using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Models.Parameters
{
    public class CurrencyExchangeModel : BaseParamModel
    {
        public string ToCurrency { get; set; }
        public double Amount { get; set; }
    }
}
