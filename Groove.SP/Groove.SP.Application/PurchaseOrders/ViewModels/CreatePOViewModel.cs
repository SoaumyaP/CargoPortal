using System;
using FluentValidation;
using System.Collections.Generic;

using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.Validations;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class CreatePOViewModel: ViewModelBase<PurchaseOrderModel>, IValidatableObject
    {
        public string PONumber { get; set; }
        public string POKey { get; set; }
        public DateTime? POIssueDate { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public long? NumberOfLineItems { get; set; }
        public string CarrierCode { get; set; }
        public string GatewayCode { get; set; }
        public string PaymentCurrencyCode { get; set; }
        public DateTime? EarliestDeliveryDate { get; set; }
        public DateTime? EarliestShipDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ExpectedShipDate { get; set; }
        public string Incoterm { get; set; }
        public DateTime? LatestDeliveryDate { get; set; }
        public DateTime? LatestShipDate { get; set; }
        public string ModeOfTransport { get; set; }       
        public string CustomerReferences { get; set; }
        public string Department { get; set; }
        public string Season { get; set; }
        public string PaymentTerms { get; set; }
        public string ShipFrom { get; set; }
        public string ShipTo { get; set; }
        public string PORemark { get; set; }
        public string POTerms { get; set; }
        public string HazardousMaterialsInstruction { get; set; }
        public string SpecialHandlingInstruction { get; set; }
        public string CarrierName { get; set; }
        public string GatewayName { get; set; }
        public string ShipFromName { get; set; }
        public string ShipToName { get; set; }
        public string BlanketPOKey { get; set; }
        public PurchaseOrderStatus? Status { get; set; }
        public bool ProductionStarted { set; get; }
        public bool QCRequired { set; get; }
        public bool ShortShip { set; get; }
        public bool SplitShipment { set; get; }
        public DateTime? ProposeDate { set; get; }
        public string Remark { set; get; }
        public POType? POType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EquipmentType? ContainerType { get; set; }
        public decimal? Volume { get; set; }
        public decimal? GrossWeight { get; set; }
        public DateTime? ContractShipmentDate { get; set; }

        public ICollection<CreateOrUpdatePOContactViewModel> Contacts { get; set; }
        public ICollection<POLineItemViewModel> LineItems { get; set; }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<CreatePOViewModel>)validationContext.GetService(typeof(IValidator<CreatePOViewModel>));
            var result = validator.Validate(this);
            foreach(var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }        
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
