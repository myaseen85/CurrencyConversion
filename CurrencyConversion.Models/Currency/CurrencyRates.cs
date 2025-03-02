using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Models.Currency
{
    public class CurrencyRates
    {
        public double Amount { get; set; }
        public string Base { get; set; }
        public string Date { get; set; }
        public Rates Rates { get; set; }
    }
}
