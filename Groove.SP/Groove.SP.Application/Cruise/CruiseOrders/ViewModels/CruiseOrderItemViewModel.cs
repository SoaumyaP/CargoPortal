using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities.Cruise;
using Newtonsoft.Json;
using System;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class CruiseOrderItemViewModel : ViewModelBase<CruiseOrderItemModel>
    {
        public long Id { get; set; }
        public DateTime? LineEstimatedDeliveryDate { get; set; }
        public DateTime? FirstReceivedDate { get; set; }
        public string MakerReferenceOfItemName2 { get; set; }
        public string RequestLineShoreNotes { get; set; }
        public string ShipRequestLineNotes { get; set; }
        public long? QuantityDelivered { get; set; }
        public int? RequestLine { get; set; }
        public string RequestNumber { get; set; }
        public long? RequestQuantity { get; set; }
        public string UOM { get; set; }
        public string CurrencyCode { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal? NetUSUnitPrice { get; set; }
        public decimal? NetUnitPrice { get; set; }
        public decimal? TotalOrderPrice { get; set; }
        public long? OrderQuantity { get; set; }
        public int? POLine { get; set; }
        public string LatestDialog { get; set; }

        public string Sub1 { get; set; }
        public string Sub2 { get; set; }
        public string QuotedCostCurrency { get; set; }

        public DateTime? ReadyDate { get; set; }
        public DateTime? REQOnboardDate { get; set; }
        public string Origin { get; set; }
        public DateTime? ApprovedDate { get; set; }


        // Miscellaneous
        public bool? CommercialInvoice { get; set; }
        public string Contract { get; set; }
        public string ShipboardLoadingLocation { get; set; }
        public string DeliveryPort { get; set; }
        public string Destination { get; set; }
        public string ApprovedBy { get; set; }
        public string Comments { get; set; }
        public string Priority { get; set; }
        public decimal? QuotedCost { get; set; }
        public string DelayCause { get; set; }
        public string DeliveryTicket { get; set; }
        public string DestinationCountry { get; set; }
        public string BuyerName { get; set; }
        public string ItemUpdates { get; set; }

        /// <summary>
        /// To define cruise item id which was copied to current item
        /// <br></br><b>Only item copied from another contains a value</b>
        /// </summary>
        public long? OriginalItemId { get; set; }

        /// <summary>
        /// To define organization id of user who copied data to current item
        /// <br></br><b>Only item copied from another contains a value</b>
        /// </summary>
        public long? OriginalOrganizationId { get; set; }

        public long OrderId { get; set; }
        public long? ShipmentId { get; set; }

        public CruiseOrderItemShipmentReferenceViewModel Shipment { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
