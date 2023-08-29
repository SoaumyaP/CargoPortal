using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE.Models;

namespace Groove.SP.Application.Utilities;

public static class EntityExtension
{
    public static void PopulateFromShipment(this ConsignmentModel consignment, ShipmentModel shipment)
    {
        consignment.ShipFrom = shipment.ShipFrom;
        consignment.ShipFromETDDate = shipment.ShipFromETDDate;
        consignment.ShipTo = shipment.ShipTo;
        consignment.ShipToETADate = shipment.ShipToETADate ?? default;// TODO Hau check
        consignment.Status = shipment.Status;
        consignment.ModeOfTransport = shipment.ModeOfTransport;
        consignment.Movement = shipment.Movement;
        consignment.Unit = shipment.TotalUnit;
        consignment.UnitUOM = shipment.TotalUnitUOM;
        consignment.Package = shipment.TotalPackage;
        consignment.PackageUOM = shipment.TotalPackageUOM;
        consignment.Volume = shipment.TotalVolume;
        consignment.VolumeUOM = shipment.TotalVolumeUOM;
        consignment.GrossWeight = shipment.TotalGrossWeight;
        consignment.GrossWeightUOM = shipment.TotalGrossWeightUOM;
        consignment.NetWeight = shipment.TotalNetWeight;
        consignment.NetWeightUOM = shipment.TotalNetWeightUOM;
    }

    public static void PopulateFromContainer(this ConsolidationModel consolidation, ContainerModel container)
    {
        consolidation.ContainerNo = container.ContainerNo;
        consolidation.SealNo = container.SealNo;
        consolidation.SealNo2 = container.SealNo2;
        consolidation.CarrierSONo = container.CarrierSONo;
        consolidation.EquipmentType = container.ContainerType;
        consolidation.TotalGrossWeight = container.TotalGrossWeight;
        consolidation.TotalNetWeight = container.TotalNetWeight;
        consolidation.TotalPackage = container.TotalPackage;
        consolidation.TotalVolume = container.TotalVolume;
        consolidation.LoadingDate = container.LoadingDate;
        consolidation.TotalPackageUOM = container.TotalPackageUOM;
    }

    public static void PopulateFromShipmentContact(this BillOfLadingContactModel houseBillContact, ShipmentContactModel shipmentContact)
    {
        if (shipmentContact.OrganizationId != 0)
        {
            houseBillContact.OrganizationId = shipmentContact.OrganizationId;
        }

        houseBillContact.OrganizationRole = shipmentContact.OrganizationRole;
        houseBillContact.CompanyName = shipmentContact.CompanyName;
        houseBillContact.ContactName = shipmentContact.ContactName;
        houseBillContact.Address = shipmentContact.Address;
        houseBillContact.ContactEmail = shipmentContact.ContactEmail;
        houseBillContact.ContactNumber = shipmentContact.ContactNumber;
    }

    public static void PopulateFromShipmentContact(this MasterBillOfLadingContactModel masterBillContact, ShipmentContactModel shipmentContact)
    {
        if (shipmentContact.OrganizationId != 0)
        {
            masterBillContact.OrganizationId = shipmentContact.OrganizationId;
        }

        masterBillContact.OrganizationRole = shipmentContact.OrganizationRole;
        masterBillContact.CompanyName = shipmentContact.CompanyName;
        masterBillContact.ContactName = shipmentContact.ContactName;
        masterBillContact.Address = shipmentContact.Address;
        masterBillContact.ContactEmail = shipmentContact.ContactEmail;
        masterBillContact.ContactNumber = shipmentContact.ContactNumber;
    }

    public static void SyncFromCruiseOrder(this PurchaseOrderModel purchaseOrder, CruiseOrderModel cruiseOrder)
    {
        purchaseOrder.CreatedBy = cruiseOrder.CreatedBy;
        purchaseOrder.CreatedDate = cruiseOrder.CreatedDate;
        purchaseOrder.UpdatedBy = cruiseOrder.UpdatedBy;
        purchaseOrder.UpdatedDate = cruiseOrder.UpdatedDate;
        purchaseOrder.PONumber = cruiseOrder.PONumber;
        purchaseOrder.POIssueDate = cruiseOrder.PODate;
        purchaseOrder.Status = (PurchaseOrderStatus)cruiseOrder.POStatus;
        purchaseOrder.Stage = POStageType.Released;
        purchaseOrder.POType = POType.Bulk;
    }

    public static void SyncFromCruiseOrderContact(this PurchaseOrderContactModel purchaseOrderContact, CruiseOrderContactModel cruiseOrderContact, Organization contactOrg)
    {
        purchaseOrderContact.CreatedBy = cruiseOrderContact.CreatedBy;
        purchaseOrderContact.CreatedDate = cruiseOrderContact.CreatedDate;
        purchaseOrderContact.UpdatedBy = cruiseOrderContact.UpdatedBy;
        purchaseOrderContact.UpdatedDate = cruiseOrderContact.UpdatedDate;
        purchaseOrderContact.OrganizationId = cruiseOrderContact.OrganizationId;
        purchaseOrderContact.OrganizationCode = contactOrg?.Code ?? "0";
        purchaseOrderContact.OrganizationRole = cruiseOrderContact.OrganizationRole;
        purchaseOrderContact.CompanyName = cruiseOrderContact.CompanyName;
        purchaseOrderContact.AddressLine1 = contactOrg?.Address;
        purchaseOrderContact.AddressLine2 = cruiseOrderContact.Address;
        purchaseOrderContact.AddressLine3 = contactOrg?.AddressLine3;
        purchaseOrderContact.AddressLine4 = contactOrg?.AddressLine4;
        purchaseOrderContact.ContactName = cruiseOrderContact.ContactName;
        purchaseOrderContact.ContactNumber = cruiseOrderContact.ContactNumber;
        purchaseOrderContact.ContactEmail = cruiseOrderContact.ContactEmail;
    }

    public static void SyncFromCruiseOrderItem(this POLineItemModel poLineItem, CruiseOrderItemModel cruiseOrderItem)
    {
        poLineItem.CreatedBy = cruiseOrderItem.CreatedBy;
        poLineItem.CreatedDate = cruiseOrderItem.CreatedDate;
        poLineItem.UpdatedBy = cruiseOrderItem.UpdatedBy;
        poLineItem.UpdatedDate = cruiseOrderItem.UpdatedDate;
        poLineItem.LineOrder = cruiseOrderItem.POLine;
        poLineItem.OrderedUnitQty = cruiseOrderItem.OrderQuantity != null ? (int)cruiseOrderItem.OrderQuantity : 0;
        poLineItem.BookedUnitQty = 0;
        poLineItem.BalanceUnitQty = poLineItem.OrderedUnitQty - poLineItem.BookedUnitQty;
        poLineItem.ProductCode = cruiseOrderItem.ItemId;
        poLineItem.ProductName = cruiseOrderItem.ItemName;

        // UOM mapping
        switch (cruiseOrderItem.UOM)
        {
            case CruiseUOM.Each:
                poLineItem.UnitUOM = UnitUOMType.Each;
                break;
            case CruiseUOM.Unit:
                poLineItem.UnitUOM = UnitUOMType.Each;
                break;
            case CruiseUOM.Cartons:
                poLineItem.UnitUOM = UnitUOMType.Set;
                break;
            case CruiseUOM.Packages:
                poLineItem.UnitUOM = UnitUOMType.Set;
                break;
            case CruiseUOM.Pieces:
                poLineItem.UnitUOM = UnitUOMType.Piece;
                break;
            default:
                poLineItem.UnitUOM = 0;
                break;
        }

        poLineItem.UnitPrice = cruiseOrderItem.NetUnitPrice != null ? cruiseOrderItem.NetUnitPrice.Value : 0;
        poLineItem.CurrencyCode = cruiseOrderItem.CurrencyCode;
    }
}