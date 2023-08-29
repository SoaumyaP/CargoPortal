using System.Collections.Generic;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Application.PurchaseOrders.ViewModels;

namespace Groove.SP.Application.CruiseOrders.ViewModels
{
    /// <summary>
    /// It contains all properties which will override data on CruiseOrderModel / database
    /// </summary>
    public class ReviseCruiseOrderViewModel : ViewModelBase<CruiseOrderModel>
    {
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

        public ICollection<ReviseCruiseOrderItemViewModel> Items { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
