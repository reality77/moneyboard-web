using System;
using dto;

namespace web.Utils
{
    public static class Utils
    {
        public static string ToCurrencyString(this decimal value)
        {
            return value.ToString("N2");
        }

        public static string ToCurrencyString(this ECurrency currency) =>
            currency switch
            {
                ECurrency.EUR => "€",
                _ => string.Empty,
            };
    }
}