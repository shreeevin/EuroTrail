using EuroTrail.Models;

namespace EuroTrail.Helpers
{
    public static class CurrencyHelper
    {
        public static List<Currency> GetCurrencies()
        {
            return new List<Currency>
            {
                new Currency { Code = "USD", Name = "United States Dollar" },
                new Currency { Code = "EUR", Name = "Euro" },
                new Currency { Code = "JPY", Name = "Japanese Yen" },
                new Currency { Code = "GBP", Name = "British Pound Sterling" },
                new Currency { Code = "AUD", Name = "Australian Dollar" },
                new Currency { Code = "CAD", Name = "Canadian Dollar" },
                new Currency { Code = "CHF", Name = "Swiss Franc" },
                new Currency { Code = "CNY", Name = "Chinese Yuan Renminbi" },
                new Currency { Code = "INR", Name = "Indian Rupee" },
                new Currency { Code = "NPR", Name = "Nepalese Rupee" },
                new Currency { Code = "NZD", Name = "New Zealand Dollar" },
                new Currency { Code = "SGD", Name = "Singapore Dollar" }
            };
        }
    }
}
