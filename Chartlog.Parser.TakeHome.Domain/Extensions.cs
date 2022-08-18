using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain
{
    internal static class Extensions
    {
        internal static string[] SplitIntoRows(this string content)
        {
            return Regex.Split(content, "\r\n|\r|\n");
        }

        internal static string ToMD5(this string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        internal static string Obfuscate(this string input)
        {

            if (input.Length < 5)
                return input;

            var sb = new StringBuilder();
            var arr = input.ToCharArray();
            for (var i = 0; i < arr.Length; i++)
            {
                if (i > 1 && i < arr.Length - 2)
                    sb.Append('x');
                else
                    sb.Append(arr[i]);
            }

            return sb.ToString();

        }

        public static TimeZoneInfo ConvertTimeZoneFromIanaToWindows(this string timezone)
        {
            var tz = TimeZoneConverter.TZConvert.IanaToWindows(timezone);
            return TimeZoneInfo.FindSystemTimeZoneById(tz);
        }

        public static DateTime ParseDateTimeWithCulture(this string dateTime, string culture)
        {
            if (string.IsNullOrWhiteSpace(culture))
                throw new ArgumentNullException(culture);

            return DateTime.Parse(dateTime, new CultureInfo(culture));
        }


        public static DateTime ParseDateTimeWithCultureAndTimezone(this string dateTime, string culture,
            string timeZone)
        {
            var tz = timeZone.ConvertTimeZoneFromIanaToWindows();
            var time = DateTime.Parse(dateTime, new CultureInfo(culture));
            var offset = new DateTimeOffset(time, tz.GetUtcOffset(time));
            return offset.ToUniversalTime().DateTime;
        }

        public static DateTime ParseDateTimeWithCultureAndTimeZoneAndSpecialFormat(this string dateTime, string culture,
            string timeZone, string format)
        {
            var tz = timeZone.ConvertTimeZoneFromIanaToWindows();
            var time = DateTime.ParseExact(dateTime, format, new CultureInfo(culture));
            var offset = new DateTimeOffset(time, tz.GetUtcOffset(time));
            return offset.ToUniversalTime().DateTime;
        }

        public static DateTime ParseDateTimeWithCultureAndFormat(this string dateTime, string culture,
            string specifiedFormat)
        {
            if (string.IsNullOrWhiteSpace(culture))
                throw new ArgumentNullException(culture);

            return DateTime.ParseExact(dateTime, specifiedFormat, new CultureInfo(culture));
        }
    }
}
