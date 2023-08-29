using Newtonsoft.Json;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ImportBookingResult
    {
        public long? BookingId { get; set; }

        public string BookingRefNumber { get; set; }

        public bool Success { get; set; }

        public List<System.ComponentModel.DataAnnotations.ValidationResult> Result { get; set; }
    }
}