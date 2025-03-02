using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Models.Configurations
{
    public class CurrencyConversionAppSettingConfiguration
    {
        public LogFile LogFile { get; set; }
        public FrankFurterCurrencyConversionProviderSetting FrankFurterCurrencyConversionProviderSetting { get; set; }
        public JwtSetttings JwtSetttings { get; set; }
    }
}
