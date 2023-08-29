using Newtonsoft.Json.Converters;
namespace Groove.SP.Application.Converters
{
    /// <summary>
    /// Custom JsonConverter for Date time value which is UTC
    /// </summary>
    /// <remarks>
    /// Use it if stored data is in UTC and it will display on front-end differently by time-zone
    /// </remarks>
    public class UtcDateTimeConverter: IsoDateTimeConverter
    {
        /// <summary>
        /// With default format string ISO
        /// </summary>
        public UtcDateTimeConverter()
        {
            DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ";
        }

        /// <summary>
        /// Use it only what value is UTC
        /// </summary>
        /// <param name="format">'Z' should be the last char in the format</param>
        public UtcDateTimeConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
