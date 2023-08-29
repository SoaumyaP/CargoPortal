using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ROLineItems.ViewModels;
using Groove.SP.Application.RoutingOrder.Validations;
using Groove.SP.Application.RoutingOrderContact.ViewModels;
using Groove.SP.Application.RoutingOrderContainer.ViewModels;
using Groove.SP.Application.RoutingOrderInvoice.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Groove.SP.Application.RoutingOrder.ViewModels
{
    [XmlRoot("RoutingOrder", Namespace = "http://tempuri.org/RoutingOrders_CSFE_v0_1.xsd")]
    public class ImportRoutingOrderViewModel : ViewModelBase<RoutingOrderModel>
    {
        [XmlElement("RoutingOrderNumber")]
        public string RoutingOrderNumber { get; set; }

        [XmlElement("RoutingOrderDate")]
        public string RoutingOrderDateString { get; set; }
        public DateTime RoutingOrderDate => DateTime.TryParse(RoutingOrderDateString, out var result) ? result : default;

        [XmlElement("CargoReadyDate")]
        public string CargoReadyDateString { get; set; }
        public DateTime? CargoReadyDate => !string.IsNullOrWhiteSpace(CargoReadyDateString) ? (DateTime.TryParse(CargoReadyDateString, out var result)
            ? result : throw new FormatException($"'{nameof(this.CargoReadyDate)}' is not in the correct format."))
            : null;

        [XmlElement("Status")]
        public string StatusName { get; set; }
        public int? Status => !string.IsNullOrWhiteSpace(StatusName) ? (int.TryParse(StatusName, out var result)
            ? result : throw new FormatException($"'{nameof(this.Status)}' is not in the correct format."))
            : null;

        [XmlElement("NumberOfLineItems")]
        public long NumberOfLineItems { get; set; }

        [XmlElement("Incoterm")]
        public string Incoterm { get; set; }

        [XmlElement("ModeOfTransport")]
        public string ModeOfTransport { get; set; }

        [XmlElement("LogisticsService")]
        public string LogisticsService { get; set; }

        [XmlElement("MovementType")]
        public string MovementType { get; set; }

        /// <summary>
        /// Location Name.
        /// </summary>
        [XmlElement("ShipFrom")]
        public string ShipFrom { get; set; }

        /// <summary>
        /// Location Name.
        /// </summary> 
        [XmlElement("ShipTo")]
        public string ShipTo { get; set; }

        /// <summary>
        /// Location Name.
        /// </summary>
        [XmlElement("ReceiptPort")]
        public string ReceiptPort { get; set; }

        /// <summary>
        /// Location Name.
        /// </summary> 
        [XmlElement("DeliveryPort")]
        public string DeliveryPort { get; set; }

        [XmlElement("EarliestShipDate")]
        public string EarliestShipDateString { get; set; }
        public DateTime? EarliestShipDate => !string.IsNullOrWhiteSpace(EarliestShipDateString) ? (DateTime.TryParse(EarliestShipDateString, out var result)
            ? result : throw new FormatException($"'{nameof(this.EarliestShipDate)}' is not in the correct format."))
            : null;

        [XmlElement("ExpectedShipDate")]
        public string ExpectedShipDateString { get; set; }
        public DateTime? ExpectedShipDate => !string.IsNullOrWhiteSpace(ExpectedShipDateString) ? (DateTime.TryParse(ExpectedShipDateString, out var result)
            ? result : throw new FormatException($"'{nameof(this.ExpectedShipDate)}' is not in the correct format."))
            : null;

        [XmlElement("ExpectedDeliveryDate")]
        public string ExpectedDeliveryDateString { get; set; }
        public DateTime? ExpectedDeliveryDate => !string.IsNullOrWhiteSpace(ExpectedDeliveryDateString) ? (DateTime.TryParse(ExpectedDeliveryDateString, out var result)
            ? result : throw new FormatException($"'{nameof(this.ExpectedDeliveryDate)}' is not in the correct format."))
            : null;

        [XmlElement("LatestShipDate")]
        public string LatestShipDateString { get; set; }
        public DateTime? LatestShipDate => !string.IsNullOrWhiteSpace(LatestShipDateString) ? (DateTime.TryParse(LatestShipDateString, out var result)
            ? result : throw new FormatException($"'{nameof(this.LatestShipDate)}' is not in the correct format."))
            : null;

        [XmlElement("LastDateForShipment")]
        public string LastDateForShipmentString { get; set; }
        public DateTime? LastDateForShipment => !string.IsNullOrWhiteSpace(LastDateForShipmentString) ? (DateTime.TryParse(LastDateForShipmentString, out var result)
            ? result : throw new FormatException($"'{nameof(this.LastDateForShipment)}' is not in the correct format."))
            : null;

        /// <summary>
        /// Carrier Code.
        /// </summary>
        [XmlElement("Carrier")]
        public string Carrier { get; set; }

        [XmlElement("VesselName")]
        public string VesselName { get; set; }

        [XmlElement("VoyageNumber")]
        public string VoyageNo { get; set; }

        [XmlElement("IsContainDangerousGoods")]
        public string IsContainDangerousGoods { get; set; }

        [XmlElement("IsBatteryOrChemical")]
        public string IsBatteryOrChemical { get; set; }

        [XmlElement("IsCIQOrFumigation")]
        public string IsCIQOrFumigation { get; set; }

        [XmlElement("IsExportLicence")]
        public string IsExportLicence { get; set; }

        [XmlElement("Remarks")]
        public string Remarks { get; set; }

        [XmlElement("Contact")]
        public List<ImportRoutingOrderContactViewModel> Contacts { get; set; }

        [XmlElement("LineItem")]
        public List<ImportROLineItemViewModel> LineItems { get; set; }

        [XmlElement("Invoice")]
        public List<ImportRoutingOrderInvoiceViewModel> Invoices { get; set; }

        [XmlElement("Container")]
        public List<ImportRoutingOrderContainerViewModel> Containers { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ImportRoutingOrderViewModelValidator().ValidateAndThrow(this);
        }
    }
}