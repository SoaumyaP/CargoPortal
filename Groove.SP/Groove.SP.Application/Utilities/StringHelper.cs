using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Groove.SP.Application.Utilities
{
    public static class StringHelper
    {
        public static string FirstCharToUpperCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            return $"{char.ToUpper(value[0])}{value.Substring(1)}";
        }

        public static string FirstCharToLowerCase(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static IList<string> Split(string value, char sepatator)
        {
            return value?.Split(sepatator)?.ToList();
        }

        public static IList<long> SplitToLong(string value, char sepatator)
        {
            return value?.Split(sepatator)
                .Where(x => long.TryParse(x, out _))
                .Select(long.Parse)?.ToList();
        }

        public static string PurifyAffiliates(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return Regex.Replace(value, @"[^0-9,]+", "");
        }

        public static IEnumerable<long> ToAffiliateIds(this string value)
        {
            var purified = value.PurifyAffiliates();
            if (string.IsNullOrEmpty(value))
            {
                return Enumerable.Empty<long>();
            }
            return purified.Split(',', System.StringSplitOptions.RemoveEmptyEntries).Select(x => long.Parse(x));
        }

        public static bool IsValidEmailAddress(this string value)
        {
            if (value.Trim().EndsWith("."))
            {
                return false;
            }

            var emailCheck = new EmailAddressAttribute();

            return emailCheck.IsValid(value);
        }

        /// <summary>
        /// To split a string into multiple address lines based on the number of characters without word-break.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Dictionary<int, string> SplitAddressBySize(this string value, int size = 50)
        {
            var res = new Dictionary<int, string>();

            if (value.Length == 0) return res;

            int currentLine = 1;
            do
            {
                var address = string.Empty;

                if (value.Length - 1 < size)
                {
                    address = value;
                    res.Add(currentLine, address.Trim());
                    break;
                }
                else
                {
                    address = value[..size];
                }

                var nextWordIndex = address.Length;

                // is word-break.
                if (!Char.IsWhiteSpace(value, nextWordIndex - 1) && !Char.IsWhiteSpace(value, nextWordIndex))
                {
                    for (int i = nextWordIndex; i < value.Length; i++)
                    {
                        if (Char.IsWhiteSpace(value, i))
                        {
                            break;
                        }
                        address += value[i];
                        nextWordIndex += 1;
                    }
                }


                if (nextWordIndex >= value.Length)
                {
                    value = "";
                }
                else
                {
                    value = value[nextWordIndex..];
                }

                res.Add(currentLine, address.Trim());
                currentLine += 1;

            } while (value.Length > 0);

            return res;
        }

        public static string AppendTimeStamp(this string fileName, char seperator = '_')
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(fileName),
                DateTime.Now.ToString($"{seperator}yyyyMMddHHmmssfff"),
                Path.GetExtension(fileName));
        }
    }
}