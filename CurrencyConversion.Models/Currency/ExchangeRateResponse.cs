using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Models.Currency
{
    public class ExchangeRateResponse
    {
        public double Amount { get; set; }
        public string Base { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, double> Rates { get; set; }
        public double ConvertedAmount { get; set; }
    }
}
