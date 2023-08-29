using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class ImportWarehouseBookingViewModel
    {
        [Required]
        public string Owner { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        public List<IFormFile> Files { get; set; }

        [Required]
        public string EmailSubject { get; set; }

        [Required]
        public string Customer { get; set; }

        /// <summary>
        /// Get booking form excel file .xlsx that contains booking information.
        /// </summary>
        public IFormFile BookingForm
        {
            get
            {
                if (Files != null && Files.Any())
                {
                    return Files.FirstOrDefault(
                        x => x.FileName.ToLowerInvariant().Replace(" ", string.Empty).Contains("bookingform")
                        && x.FileName.ToLowerInvariant().Replace(" ", string.Empty).Contains(".xlsx"));
                }
                return null;
            }
        }

    }

    public class ImportingWarehouseBookingResultViewModel
    {
        [JsonIgnore]
        public ImportingWarehouseBookingResult ResultType { get; set; }
        public Exception Exception { get; set; }
        public bool Success
        {
            get
            {
                return ResultType == ImportingWarehouseBookingResult.Success;
            }
        }
        public Dictionary<string, ExpandoObject> Results { get; set; }

        public ImportingWarehouseBookingResultViewModel()
        {
            ResultType = ImportingWarehouseBookingResult.Success;
            Results = new Dictionary<string, ExpandoObject>
            {
                { "booking", new ExpandoObject() },
                { "products", new ExpandoObject() }
            };
        }

        public void LogBookingValidationFailed (string logName, string logContext, ImportingWarehouseBookingResult type = ImportingWarehouseBookingResult.InvalidBookingForm)
        {
            ResultType = type;
            var value = Results["booking"];
            value.TryAdd(logName, logContext); 
        }

        public void LogProductValidationFailed(string logName, string logContext)
        {
            ResultType = ImportingWarehouseBookingResult.InvalidBookingForm;
            var value = Results["products"];
            value.TryAdd(logName, logContext);
        }

        public void LogBookingSuccess(string logName, string logContext)
        {
            ResultType = ImportingWarehouseBookingResult.Success;
            if (!Results.ContainsKey("data"))
            {
                Results = new Dictionary<string, ExpandoObject>
                {
                    { "data", new ExpandoObject() }
                };
            }
            var value = Results["data"];
            value.TryAdd(logName, logContext);
        }

        public void LogErrors(string logName, string logContext)
        {
            ResultType = ImportingWarehouseBookingResult.ErrorDuringImport;
            if (!Results.ContainsKey("errors"))
            {
                Results = new Dictionary<string, ExpandoObject>
                {
                    { "errors", new ExpandoObject() }
                };
            }
            var value = Results["errors"];
            value.TryAdd(logName, logContext);
        }

        public string ExportLogDetails()
        {
            var result = new List<string>();
            if (Results.ContainsKey("booking"))
            {
                IDictionary<string, object> propertyValues = Results["booking"];

                foreach (var property in propertyValues.Keys)
                {
                    result.Add(string.Format("{0}: {1}", property, propertyValues[property]));
                }
            }
            if (Results.ContainsKey("products"))
            {
                IDictionary<string, object> propertyValues = Results["products"];

                foreach (var property in propertyValues.Keys)
                {
                    result.Add(string.Format("{0}: {1}", property, propertyValues[property]));
                }
            }
            if (Results.ContainsKey("errors"))
            {
                IDictionary<string, object> propertyValues = Results["errors"];

                foreach (var property in propertyValues.Keys)
                {
                    result.Add(string.Format("{0}: {1}", property, propertyValues[property]));
                }
            }
            return string.Join(Environment.NewLine, result);
        }

    }

    public enum ImportingWarehouseBookingResult
    {
        /// <summary>
        /// File naming incorrect -> Email back to vendor
        /// </summary>
        IncorrectFileNaming,
        /// <summary>
        /// File name duplicated -> Email back to vendor
        /// </summary>
        DuplicatedFile,
        /// <summary>
        /// Booking form content incorrect -> Email back to vendor
        /// </summary>
        InvalidBookingForm,
        /// <summary>
        /// Passed -> Email back to vendor
        /// </summary>
        Success,

        /// <summary>
        /// Request's param is missing or internal excepion -> Not email back to vendor
        /// </summary>
        ErrorDuringImport

    }
}
