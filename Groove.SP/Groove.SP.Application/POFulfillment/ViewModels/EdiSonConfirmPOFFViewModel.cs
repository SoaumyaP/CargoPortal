using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Groove.SP.Core.Models;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class EdiSonConfirmPOFFViewModel : IValidatableObject
    {
        public EdiSonConfirmPOFFViewModel()
        {
            Legs = new List<Leg>();
        }

        public string BookingReferenceNo { get; set; }

        public string SONumber { get; set; }

        public string BillOfLadingHeader { get; set; }

        public string CYEmptyPickupTerminalCode { get; set; }

        public string CYEmptyPickupTerminalDescription { get; set; }

        public string CFSWarehouseCode { get; set; }

        public string CFSWarehouseDescription { get; set; }

        public DateTime? CYClosingDate { get; set; }

        public DateTime? CFSClosingDate { get; set; }


        public IList<Leg> Legs { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validator = (IValidator<EdiSonConfirmPOFFViewModel>)validationContext.GetService(typeof(IValidator<EdiSonConfirmPOFFViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }
    }

    public class Leg
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ModeOfTransportType ModeOfTransport { get; set; }

        public string CarrierCode { get; set; }

        public string VesselFlight { get; set; }

        public string LoadingPortCode { get; set; }

        public string DischargePortCode { get; set; }

        public DateTime? ETD { get; set; }

        public DateTime? ETA { get; set; }
    }
}
