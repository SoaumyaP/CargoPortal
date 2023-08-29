using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class WarehouseSOFormViewModel
    {
        public WarehouseSOFormViewModel()
        {
            Orders = new List<WarehouseOrderSOFormViewModel>();
        }

        public string ContactPerson { set; get; }
        public string ContactPhone { set; get; }
        public string ContactEmail { set; get; }
        public string LocationName { set; get; }
        public string AddressLine1 { set; get; }
        public string AddressLine2 { set; get; }
        public string AddressLine3 { set; get; }
        public string AddressLine4 { set; get; }

        public string CompanyName { set; get; }
        public string CustomerAddress { set; get; }
        public string CustomerAddressLine2 { set; get; }
        public string CustomerAddressLine3 { set; get; }
        public string CustomerAddressLine4 { set; get; }
        public string CustomerLogo { set; get; }
        public string FixedLogo { set; get; }

        public string SupplierCompanyName { set; get; }
        public string SupplierAddress { set; get; }
        public string SupplierAddressLine2 { set; get; }
        public string SupplierAddressLine3 { set; get; }
        public string SupplierAddressLine4 { set; get; }
        public string WorkingHours { get; set; }
        public string Remarks { get; set; }

        public string BookingNo { set; get; }
        public string ShipmentNo { set; get; }
        public string CompanyNo { set; get; }
        public string PlanNo { set; get; }
        public string ActualTimeArrival { set; get; }
        public string ExpectedHubArrivalDate { set; get; }
        public string ConfirmedHubArrivalDate { set; get; }
        public string ContainerNo { set; get; }
        public string HAWBNo { set; get; }
        public string DeliveryMode { set; get; }
        public IncotermType? Incoterm { set; get; }
        public string ETDOrigin { set; get; }
        public string ETADestination { set; get; }
        public string ShipFromName { set; get; }
        public string NameofInternationalAccount { set; get; }
        public byte[] QRCode { get; set; }

        public List<WarehouseOrderSOFormViewModel> Orders { set; get; }
    }
}
