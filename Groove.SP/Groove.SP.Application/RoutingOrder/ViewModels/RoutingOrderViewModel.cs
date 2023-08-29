using Groove.SP.Application.Common;
using Groove.SP.Application.RoutingOrderContainer.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.RoutingOrder.ViewModels
{
    public class RoutingOrderViewModel : ViewModelBase<RoutingOrderModel>
    {
        public long Id { get; set; }
        public string RoutingOrderNumber { get; set; }
        public DateTime RoutingOrderDate { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public RoutingOrderStageType Stage { get; set; }
        public RoutingOrderStatus Status { get; set; }
        public long NumberOfLineItems { get; set; }
        public IncotermType Incoterm { get; set; }
        public ModeOfTransportType ModeOfTransport { get; set; }
        public LogisticsServiceType? LogisticsService { get; set; }
        public MovementType MovementType { get; set; }
        public long ShipFromId { get; set; }
        public long ShipToId { get; set; }
        public DateTime? EarliestShipDate { get; set; }
        public DateTime ExpectedShipDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? LatestShipDate { get; set; }
        public DateTime LastShipmentDate { get; set; }
        public long? CarrierId { get; set; }
        public string VesselName { get; set; }
        public string VoyageNo { get; set; }
        public bool IsContainDangerousGoods { get; set; }
        public bool IsBatteryOrChemical { get; set; }
        public bool IsCIQOrFumigation { get; set; }
        public bool IsExportLicence { get; set; }
        public string Remarks { get; set; }

        public virtual ICollection<RoutingOrderContactModel> Contacts { get; set; }
        public virtual ICollection<ROLineItemModel> LineItems { get; set; }
        public virtual ICollection<RoutingOrderContainerViewModel> Containers { get; set; }
        public virtual ICollection<RoutingOrderInvoiceModel> Invoices { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }
}