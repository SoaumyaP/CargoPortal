using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.POFulfillment.Validations;
using Groove.SP.Application.POFulfillmentContact.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Groove.SP.Application.Converters;
using System.Linq;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ImportBookingViewModel : ViewModelBase<POFulfillmentModel>, IHasFieldStatus
    {   
        /// <summary>
        /// Email address who send/import the booking.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Active to import new booking.<br/>
        /// Inactive to cancel the booking.
        /// </summary>
        public POFulfillmentStatus Status { get; set; }

        public DateTime CargoReadyDate { get; set; }

        public DateTime ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public IncotermType Incoterm { get; set; }

        public ModeOfTransportType ModeOfTransport { get; set; }

        public string PreferredCarrier { get; set; }

        public ImportLogisticsServiceType LogisticsService { get; set; }

        public ImportMovementType MovementType { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public string DeliveryPort { get; set; }

        public string ReceiptPort { get; set; }

        public string Remarks { get; set; }

        public BooleanOption IsContainDangerousGoods { get; set; }

        public BooleanOption IsShipperPickup { get; set; }

        public BooleanOption IsNotifyPartyAsConsignee { get; set; }

        public BooleanOption IsBatteryOrChemical { get; set; }

        public BooleanOption IsCIQOrFumigation { get; set; }

        public BooleanOption IsExportLicence { get; set; }

        public DateTime BookingDate { get; set; }

        public string VesselName { get; set; }

        public string VoyageNo { get; set; }

        #region Confirm POFF info

        public string CustomerBookingNo { get; set; }

        public string SONumber { get; set; }

        public string BillOfLadingHeader { get; set; }

        public string CYEmptyPickupTerminalCode { get; set; }

        public string CYEmptyPickupTerminalDescription { get; set; }

        public string CFSWarehouseCode { get; set; }

        public string CFSWarehouseDescription { get; set; }

        public DateTime? CYClosingDate { get; set; }

        public DateTime? CFSClosingDate { get; set; }
        public string PoRemark { get; set; }

        #endregion Confirm POFF info

        public ICollection<ImportBookingContactViewModel> Contacts { get; set; }

        public ICollection<POFulfillmentLoadViewModel> Loads { get; set; }

        public ICollection<ImportBookingOrderViewModel> CustomerPOs { get; set; }

        public ICollection<ImportBookingLegViewModel> Legs { get; set; }
        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            
        }

        public bool Validate(ICSFEApiClient csfeApiClient, out List<System.ComponentModel.DataAnnotations.ValidationResult> returnedErrors)
        {
            returnedErrors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (Status == 0)
            {
                var error = new System.ComponentModel.DataAnnotations.ValidationResult($"'{nameof(ImportBookingViewModel.Status)}' must not be empty.",
                            new[] { $"{nameof(ImportBookingViewModel.Status)}" });

                returnedErrors.Add(error);

                return false;
            }
            var validator = new ImportBookingViewModelValidator(csfeApiClient, Status);
            var validationResult = validator.Validate(this);

            if (!validationResult.IsValid)
            {
                foreach (var item in validationResult.Errors)
                {
                    var error = new System.ComponentModel.DataAnnotations.ValidationResult(item.ErrorMessage, new[] { item.PropertyName });
                    returnedErrors.Add(error);
                }
            }
            return validationResult.IsValid;
        }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }

    public class ImportBookingLegViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ModeOfTransportType ModeOfTransport { get; set; }

        public string CarrierCode { get; set; }

        public string VesselFlight { get; set; }

        public string LoadingPort { get; set; }

        public string DischargePort { get; set; }

        public DateTime? ETDDate { get; set; }

        public DateTime? ETADate { get; set; }
    }

    public enum ImportMovementType
    {
        /// <summary>
        /// CY/CY
        /// </summary>
        CY = 1 << 0,

        /// <summary>
        /// CFS/CY
        /// </summary>
        CFS = 1 << 1
    }

    public enum ImportLogisticsServiceType
    {
        [EnumMember(Value ="Port-to-Port")]
        InternationalPortToPort = 1 << 0,

        [EnumMember(Value ="Port-to-Door")]
        InternationalPortToDoor = 1 << 1,

        [EnumMember(Value = "Door-to-Port")]
        InternationalDoorToPort = 1 << 2,

        [EnumMember(Value = "Door-to-Door")]
        InternationalDoorToDoor = 1 << 3

    }
    
}