using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Groove.SP.Application.Utilities
{
    public static class CommonHelper
    {

        public const string UNDERSCORE = "_";
        public static string GenerateGlobalId(long entityId, string type)
        {
            return type + UNDERSCORE + entityId;
        }

        public static GlobalIdPartsViewModel GetGlobalIdParts(string globalId)
        {
            var globalIdParts = globalId.Split(UNDERSCORE);
            return new GlobalIdPartsViewModel()
            {
                Type = globalIdParts[0],
                EntityId = long.Parse(globalIdParts[1])
            };
        }

        public static long? GetEntityId(string globalId, string type)
        {
            var prefix = type + UNDERSCORE;
            return globalId.StartsWith(prefix) ? Convert.ToInt64(globalId.Substring(prefix.Length)) : (long?)null;
        }

        /// <summary>
        /// Apply checking HSCode logic:
        /// <br/>-Each HS Code is numeric and include white space and fullstop.
        /// <br/>-Each HS Code length can only be: 4, 6, 8, 10 and 13.
        /// <br/>-Each HS Code max length is 13 digits.
        /// <br/>-Each HS Code is separated by a comma (semicolon or other characters are not allowed).
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool CheckValidHSCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return true;
            }
            Regex rgx = new(@"[.\s]", RegexOptions.Multiline);
            var valueToTest = rgx.Replace(code, "");

            // Check for comma first: Please separate the HS Code by comma.

            rgx = new(@"^\b([a-zA-Z0-9,])+$", RegexOptions.Multiline);
            if (!rgx.IsMatch(valueToTest))
            {
                return false;
            }

            // Check for valid format: Its length must be in 4, 6, 8, 10, and 13 digits only.

            var hsCodeArray = valueToTest.Split(",");

            rgx = new(@"^(?:\d{4},?|\d{6},?|\d{8},?|\d{10},?|\d{13},?)$", RegexOptions.Multiline);
            foreach (var hsCode in hsCodeArray)
            {
                if (!rgx.IsMatch(hsCode))
                {
                    return false;
                }
            }

            return true;
        }

        public static Dictionary<string, string> GetDateRange(string key)
        {
            Dictionary<string, string> dates = new Dictionary<string, string>();
            DateTime today = DateTime.Today;
            string formatDate = "yyyy-MM-dd";

            switch (key)
            {
                case "This week":
                    switch (today.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            dates.Add("FromDate", today.AddDays(-6).ToString(formatDate));
                            dates.Add("ToDate", today.ToString(formatDate));
                            break;
                        case DayOfWeek.Monday:
                            dates.Add("FromDate", today.ToString(formatDate));
                            dates.Add("ToDate", today.AddDays(6).ToString(formatDate));
                            break;
                        case DayOfWeek.Tuesday:
                            dates.Add("FromDate", today.AddDays(-1).ToString(formatDate));
                            dates.Add("ToDate", today.AddDays(5).ToString(formatDate));
                            break;
                        case DayOfWeek.Wednesday:
                            dates.Add("FromDate", today.AddDays(-2).ToString(formatDate));
                            dates.Add("ToDate", today.AddDays(4).ToString(formatDate));
                            break;
                        case DayOfWeek.Thursday:
                            dates.Add("FromDate", today.AddDays(-3).ToString(formatDate));
                            dates.Add("ToDate", today.AddDays(3).ToString(formatDate));
                            break;
                        case DayOfWeek.Friday:
                            dates.Add("FromDate", today.AddDays(-4).ToString(formatDate));
                            dates.Add("ToDate", today.AddDays(2).ToString(formatDate));
                            break;
                        case DayOfWeek.Saturday:
                            dates.Add("FromDate", today.AddDays(-5).ToString(formatDate));
                            dates.Add("ToDate", today.AddDays(1).ToString(formatDate));
                            break;
                        default:
                            break;
                    }
                    break;

                case "Next 3 days":
                    dates.Add("FromDate", today.AddDays(1).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(3).ToString(formatDate));
                    break;
                case "Next 4 days":
                    dates.Add("FromDate", today.AddDays(1).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(4).ToString(formatDate));
                    break;
                case "Next 5 days":
                    dates.Add("FromDate", today.AddDays(1).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(5).ToString(formatDate));
                    break;
                case "Next 6 days":
                    dates.Add("FromDate", today.AddDays(1).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(6).ToString(formatDate));
                    break;
                case "Next 7 days":
                    dates.Add("FromDate", today.AddDays(1).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(7).ToString(formatDate));
                    break;

                case "Next 14 days":
                    dates.Add("FromDate", today.AddDays(1).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(14).ToString(formatDate));
                    break;

                case "This month":
                    var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                    dates.Add("FromDate", firstDayOfMonth.ToString(formatDate));
                    dates.Add("ToDate", lastDayOfMonth.ToString(formatDate));
                    break;

                case "This year":
                    DateTime firstDay = new DateTime(today.Year, 1, 1);
                    DateTime lastDay = new DateTime(today.Year, 12, 31);

                    dates.Add("FromDate", firstDay.ToString(formatDate));
                    dates.Add("ToDate", lastDay.ToString(formatDate));
                    break;

                case "Last year":
                    dates.Add("FromDate", new DateTime(today.AddYears(-1).Year, 1, 1).ToString(formatDate));
                    dates.Add("ToDate", new DateTime(today.AddYears(-1).Year, 12, 31).ToString(formatDate));
                    break;

                case "All":
                    DateTime fromDate = new DateTime(1, 1, 1);
                    DateTime toDate = new DateTime(9999, 1, 1);

                    dates.Add("FromDate", fromDate.ToString(formatDate));
                    dates.Add("ToDate", toDate.ToString(formatDate));
                    break;

                case "Last 7 days":
                    dates.Add("FromDate", today.AddDays(-8).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(-1).ToString(formatDate));
                    break;

                case "Last 14 days":
                    dates.Add("FromDate", today.AddDays(-15).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(-1).ToString(formatDate));
                    break;

                case "Last 30 days":
                    dates.Add("FromDate", today.AddDays(-31).ToString(formatDate));
                    dates.Add("ToDate", today.AddDays(-1).ToString(formatDate));
                    break;

                case "Last month":
                    var month = new DateTime(today.Year, today.Month, 1);
                    var first = month.AddMonths(-1);
                    var last = month.AddDays(-1);
                    dates.Add("FromDate", first.ToString(formatDate));
                    dates.Add("ToDate", last.ToString(formatDate));
                    break;
                case "Week Before":
                    DateTime currentWeekStartDate = today.WeekStartDate();
                    DateTime lastWeekStartDate = currentWeekStartDate.WeekStartDate();
                    dates.Add("FromDate", lastWeekStartDate.ToString(formatDate));
                    dates.Add("ToDate", currentWeekStartDate.AddDays(-1).ToString(formatDate));
                    break;
                default:
                    return dates;
            }

            return dates;
        }
    }

    public class GlobalIdPartsViewModel
    {
        public long EntityId { get; set; }
        public string Type { get; set; }
    }
}
