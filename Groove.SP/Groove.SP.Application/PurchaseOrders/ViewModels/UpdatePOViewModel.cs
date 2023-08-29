using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using FluentValidation;

using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.PurchaseOrders.Validations;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class UpdatePOViewModel : ViewModelBase<PurchaseOrderModel>, IHasFieldStatus, IValidatableObject
    {
        public string PONumber { get; set; }
        public DateTime? POIssueDate { get; set; }
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
        public POType? POType { get; set; }
        public POStageType? Stage { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public bool ProductionStarted { set; get; }
        public bool QCRequired { set; get; }
        public bool ShortShip { set; get; }
        public bool SplitShipment { set; get; }
        public DateTime? ProposeDate { set; get; }
        public string Remark { set; get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EquipmentType? ContainerType { get; set; }

        public decimal? Volume { get; set; }
        public decimal? GrossWeight { get; set; }
        public DateTime? ContractShipmentDate { get; set; }

        public ICollection<CreateOrUpdatePOContactViewModel> Contacts { get; set; }
        public ICollection<POLineItemViewModel> LineItems { get; set; }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<UpdatePOViewModel>)validationContext.GetService(typeof(IValidator<UpdatePOViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }

        protected override void AuditChildren(string user)
        {
            if (Contacts != null)
            {
                foreach (var contact in Contacts)
                {
                    contact.Audit(user);
                }
            }

            if (LineItems != null)
            {
                foreach (var lineItem in LineItems)
                {
                    lineItem.Audit(user);
                }
            }
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
