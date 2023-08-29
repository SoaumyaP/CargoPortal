using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Groove.SP.Application.Utilities
{
    public static class CultureHelper
    {
        public static CultureInfo[] AllCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

        public static bool IsRtl => CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;

        public static bool UsingLunarCalendar = CultureInfo.CurrentUICulture.DateTimeFormat.Calendar.AlgorithmType == CalendarAlgorithmType.LunarCalendar;

        public static CultureInfo GetCultureInfoByChecking(string name)
        {
            try
            {
                return CultureInfo.GetCultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.CurrentCulture;
            }
        }
    }
}
