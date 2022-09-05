using System.Linq;

namespace Lykke.Snow.PriceAlerts.Domain.Extensions
{
    public static class DecimalExtensions
    {
        private const int DefaultDigitPrecision = 5;
        
        public static string ToUiString(this decimal? value, int? precision = null)
        {
            return value.HasValue 
                ? value.Value.ToUiString(precision) 
                : ((decimal) 0).ToUiString(precision);
        }
        
        public static string ToUiString(this decimal value, int? precision = null)
        {
            precision = precision ?? DefaultDigitPrecision;
            
            var sharps = precision > 0
                ? "." + string.Join(string.Empty, Enumerable.Repeat("#", precision.Value))
                : "";
            
            return value.ToString($"0{sharps}");
        }
    }
}