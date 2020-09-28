using System;
using System.Web;
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

        /// <summary>
        /// Permet de nettoyer une valeur paramètre query string avant l'envoi
        /// </summary>
        public static string ToCleanQuery(this string value) => HttpUtility.UrlEncode(value);

        public static DateTime ToDate(this dto.Model.TagStatisticsModel model) => new DateTime(model.Year, model.Month ?? 1, model.Day ?? 1);
    }
}