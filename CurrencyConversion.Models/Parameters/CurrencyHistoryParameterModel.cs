using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Models.Parameters
{
    public class CurrencyHistoryParameterModel : BaseParamModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }
}
