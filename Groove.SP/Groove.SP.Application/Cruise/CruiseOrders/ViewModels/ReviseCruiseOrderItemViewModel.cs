using Groove.SP.Application.Common;
using Groove.SP.Core.Entities.Cruise;
using System;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    /// <summary>
    /// It contains all properties which will override data on CruiseOrderItemModel / database
    /// </summary>
    public class ReviseCruiseOrderItemViewModel : ViewModelBase<CruiseOrderItemModel>
    {
        public long Id { get; set; }

        public decimal? NetUnitPrice { get; set; }
        public string CurrencyCode { get; set; }     

        public string Sub1 { get; set; }
        public string Sub2 { get; set; }
        public bool? CommercialInvoice { get; set; }
        public string Priority { get; set; }
        public string Contract { get; set; }
        public string Origin { get; set; }
        public decimal? QuotedCost { get; set; }
        public string QuotedCostCurrency { get; set; }
        public string ShipboardLoadingLocation { get; set; }
        public string DelayCause { get; set; }
        public DateTime? ReadyDate { get; set; }
        public DateTime? REQOnboardDate { get; set; }
        public string DeliveryPort { get; set; }
        public string DeliveryTicket { get; set; }
        public string Destination { get; set; }
        public string DestinationCountry { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ItemUpdates { get; set; }


        // Reference information
        public long OrderId { get; set; }
        public long? ShipmentId { get; set; }
        public string ShipmentNumber { get; set; }

        /// <summary>
        /// If array contains multiple values, user may want to update to multi items.
        /// </summary>
        public string[] SelectedItems { get; set; }

        /// <summary>
        /// Array contains a number of cruise order item  po line which are needed to change values.
        /// </summary>
        public int[] SelectedItemPOLines { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
