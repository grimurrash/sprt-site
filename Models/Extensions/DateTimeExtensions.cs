using System;
using System.Collections.Generic;

namespace NewSprt.Models.Extensions
{
    public static class DateTimeExtensions
    {
        public static string GetMonthName(this DateTime dateTime)
        {
            var listMonthR = new List<string>()
            {
                "января",
                "февраля",
                "марта",
                "апреля",
                "мая",
                "июня",
                "июля",
                "августа",
                "сентября", 
                "октября", 
                "ноября", 
                "декабря"
            };

            return listMonthR[dateTime.Month-1];
        }
    }
}