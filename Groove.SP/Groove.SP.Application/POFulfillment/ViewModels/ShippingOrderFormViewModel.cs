
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class ShippingOrderFormViewModel
    {
        public string PoffNumber { get; set; }
        public string BookingRequestReferenceNumber { get; set; }

        public string SoNumber { get; set; }

        public SOFormOrganizationModel ShipperOrganization { get; set; }
        public SOFormOrganizationModel OriginAgentOrganization { get; set; }
        public SOFormOrganizationModel SupplierOrganization { get; set; }
        public SOFormOrganizationModel ConsigneeOrganization { get; set; }
        public SOFormOrganizationModel NotifyOrganization { get; set; }
        public SOFormOrganizationModel PickupOrganization { get; set; }
        public SOFormOrganizationModel BillingPartyOrganization { get; set; }

        public SOFormContactModel Shipper { get; set; }
        public SOFormContactModel OriginAgent { get; set; }
        public SOFormContactModel Supplier { get; set; }
        public SOFormContactModel Consignee { get; set; }
        public SOFormContactModel Notify { get; set; }
        public SOFormContactModel Owner { get; set; }
        public SOFormContactModel PickupAddress { get; set; }
        public SOFormContactModel BillingAddress { get; set; }


        public SOFormItineraryModel FirstItinerrary { get; set; }

        public string PortOfLoading { get; set; }
        public string PortOfDischarge { get; set; }
        public string PlaceOfReceipt { get; set; }
        public string PlaceOfDelivery { get; set; }
        public string Incoterm { get; set; }
        public string ModeOfTransport { get; set; }
        public List<SOFormOrderModel> Orders { get; set; }
        public List<SOFormProductModel> Products { get; set; }
        public DateTime? ExpectedShipDate { get; set; }
        public DateTime? PoffShipmentDate { get; set; }
        public DateTime ConfirmDate { get; set; }
        public string BookingContainers { get; set; }
        public string DateFormat => "yyyy-MM-dd";

        public POFulfillmentStage SOFormStage { get; set; }
        public bool IsDangerousGoods { get; set; }
        public string Remarks { get; set; }
        public bool IsBatteryOrChemical { get; set; }
        public bool IsCIQOrFumigation { get; set; }
        public bool IsExportLicence { get; set; }
        public bool IsNotifyPartyAsConsignee { get; set; }
        public ShippingFormType FormType { get; set; }
        public FulfillmentType FulfillmentType { get; set; }
        public string VesselName { get; set; }
        public string VoyageNo { get; set; }
        public string CarrierName { get; set; }
        public DateTime? CargoReadyDate { get; set; }

    }

    public class SOFormContactModel
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string EmailAddress { get; set; }

        public string ContactDetails {
            get
            {
                var result = "";
                if (!string.IsNullOrEmpty(ContactName))
                {
                    result += ContactName;
                    if (!string.IsNullOrEmpty(ContactNumber))
                    {
                        result += ", ";
                    }
                }
                if (!string.IsNullOrEmpty(ContactNumber))
                {
                    result += ContactNumber;
                }

                return result;

            }
        }
    }

    public class SOFormItineraryModel
    {
        public string VesselFlight { get; set; }
        public string CarrierName { get; set; }

    }

    public class SOFormOrganizationModel
    {
        public string Address { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public Location Location { get; set; }
    }

    public class SOFormOrderModel
    {
        public long Id { get; set; }
        public long PurchaseOrderId { get; set; }

        public long POLineItemId { get; set; }

        public long POFulfillmentId { get; set; }

        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public decimal? Volume
        {
            get; set;
        }

        public decimal? Weight { get; set; }

        // POFF booked package on POFF Orders
        public int? NoOfCartons { get; set; }

        // POFF ordered quantity on POFF Orders
        public int NoOfPieces { get; set; }
        public string HsCode { get; set; }

        // From Article Master
        public int? InnerQuantity { get; set; }

        // From Article Master
        public int? OuterQuantity { get; set; }
        public bool DifferentSizedCarton { 
            get
            {
                if (!OuterQuantity.HasValue)
                {
                    return false;
                }

                var result = NoOfPieces % OuterQuantity.Value;
                return result != 0;
            }
        }
        public string ChineseDescription { get; set; }

        public string ShippingMarks { get; set; }

        public int? BookedPackage { get; set; }
    }

    public class SOFormProductModel
    {
        public long Id { get; set; }
        public long PurchaseOrderId { get; set; }
        public string DescriptionOfGoods { get; set; }

    }
}
