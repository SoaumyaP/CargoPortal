using System;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class ShipmentConfirmItineraryViewModel
    {
        public bool SkipUpdates { get; set; }

        public string CYEmptyPickupTerminalCode { get; set; }

        public string CYEmptyPickupTerminalDescription { get; set; }

        public string CFSWarehouseCode { get; set; }

        public string CFSWarehouseDescription { get; set; }

        public DateTime? CYClosingDate { get; set; }

        public DateTime? CFSClosingDate { get; set; }
    }
}
