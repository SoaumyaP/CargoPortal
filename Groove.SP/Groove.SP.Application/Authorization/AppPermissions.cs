using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Authorization
{
    /// <summary>
    /// Permissions for the CS portal. Id of table Permissions will depend on position/index on the list
    /// </summary>
    /// <remarks><b>
    /// Notes:
    /// <list type="bullet">
    /// <item>
    /// Add new value to the end of the list
    /// </item>
    /// <item>
    /// Do not change existing order
    /// </item>
    /// <item>
    /// Free to update value
    /// </item>
    /// <item>
    /// Do not remove any existing value, please add [Obsolete] attribute instead and create migration script to remove RolePermissions and Permissions tables by name
    /// <code>
    /// migrationBuilder.Sql(@"DELETE FROM RolePermissions WHERE [RoleId] IN (SELECT id FROM Permissions WHERE [Name] IN('masterBillOfLading','masterBillOfLading.list','masterBillOfLading.detail'))
    /// go
    /// DELETE FROM Permissions WHERE [Name] IN ('masterBillOfLading','masterBillOfLading.list','masterBillOfLading.detail')"
    /// go ");
    /// </code>
    /// </item>
    /// </list>
    /// </b></remarks>
    public static class AppPermissions
    {
        //Dashboard
        public const string Dashboard = "Dashboard";
        public const string Dashboard_ThisWeekShipments = "Dashboard.ThisWeekShipments";
        public const string Dashboard_ThisWeekCustomerPO = "Dashboard.ThisWeekCustomerPO";
        public const string Dashboard_ThisWeekOceanVolume = "Dashboard.ThisWeekOceanVolume";
        public const string Dashboard_Top10ShipperThisWeek = "Dashboard.Top10ShipperThisWeek";
        public const string Dashboard_Top10ConsigneeThisWeek = "Dashboard.Top10ConsigneeThisWeek";
        public const string Dashboard_Top10CarrierThisWeek = "Dashboard.Top10CarrierThisWeek";
        public const string Dashboard_MonthlyTop5OceanVolumeByOrigin = "Dashboard.MonthlyTop5OceanVolumeByOrigin";
        public const string Dashboard_MonthlyTop5OceanVolumeByDestination = "Dashboard.MonthlyTop5OceanVolumeByDestination";
        public const string Dashboard_MonthlyOceanVolumeByMovement = "Dashboard.MonthlyOceanVolumeByMovement";
        public const string Dashboard_MonthlyOceanVolumeByServiceType = "Dashboard.MonthlyOceanVolumeByServiceType";

        //Products
        public const string Product = "Product";
        public const string Product_List = "Product.List";
        public const string Product_Detail = "Product.Detail";
        public const string Product_Detail_Add = "Product.Detail.Add";
        public const string Product_Detail_Edit = "Product.Detail.Edit";

        //Orders
        public const string Order = "Order";
        public const string PO_List = "Order.POList";
        public const string PO_Detail = "Order.PODetail";
        public const string PO_Detail_Add = "Order.PODetail.Add";
        public const string PO_Detail_Edit = "Order.PODetail.Edit";
        public const string PO_Delegation = "Order.PODelegation";
        public const string PO_Fulfillment_List = "Order.POFulfillmentList";
        public const string PO_Fulfillment_Detail = "Order.POFulfillmentDetail";
        public const string PO_Fulfillment_Detail_Add = "Order.POFulfillmentDetail.Add";
        public const string PO_Fulfillment_Detail_Edit = "Order.POFulfillmentDetail.Edit";
        public const string Order_PendingApprovalList = "Order.PendingApprovalList";
        public const string Order_PendingApproval_Detail = "Order.PendingApprovalDetail";
        public const string Order_PendingApproval_Detail_Edit = "Order.PendingApprovalDetail.Edit";

        //Shipments
        public const string Shipment = "Shipment";
        public const string Shipment_List = "Shipment.List";
        public const string Shipment_Detail = "Shipment.Detail";
        public const string Shipment_Detail_Confirm_Itinerary = "Shipment.Detail.ConfirmItinerary";
        public const string Shipment_ContainerDetail = "Shipment.ContainerDetail";
        [Obsolete("Do not use, it was removed on database.", true)]
        public const string Shipment_BillOfLadingDetail = "Shipment.BillOfLadingDetail";

        //MasterBillOfLading
        [Obsolete("Do not use, it was removed on database.", true)]
        public const string MasterBillOfLading = "MasterBillOfLading";
        [Obsolete("Do not use, it was removed on database.", true)]
        public const string MasterBillOfLading_List = "MasterBillOfLading.List";
        [Obsolete("Do not use, it was removed on database.", true)]
        public const string MasterBillOfLading_Detail = "MasterBillOfLading.Detail";

        //Consignments
        [Obsolete("Do not use, it was removed on database.", true)]
        public const string Consignment = "Shipment.Consignment";
        public const string Shipment_Consignment_List = "Shipment.ConsignmentList";
        public const string Shipment_Consignment_Detail = "Shipment.ConsignmentDetail";
        public const string Shipment_Consignment_Detail_Add = "Shipment.ConsignmentDetail.Add";
        public const string Shipment_Consignment_Detail_Edit = "Shipment.ConsignmentDetail.Edit";

        //Invoice
        public const string Invoice = "Invoice";
        public const string Invoice_List = "Invoice.List";

        //Organization
        public const string Organization = "Organization";
        public const string Organization_List = "Organization.List";
        public const string Organization_Detail = "Organization.Detail";
        public const string Organization_Detail_Edit = "Organization.Detail.Edit";
        public const string Organization_Compliance_List = "Organization.ComplianceList";
        public const string Organization_Compliance_Detail = "Organization.ComplianceDetail";
        public const string Organization_Compliance_Detail_Edit = "Organization.ComplianceDetail.Edit";

        //User
        public const string User = "User";
        public const string User_RequestList = "User.RequestList";
        public const string User_RequestDetail = "User.RequestDetail";
        public const string User_RequestDetail_Edit = "User.RequestDetail.Edit";
        public const string User_UserList = "User.UserList";
        public const string User_UserDetail = "User.UserDetail";
        public const string User_UserDetail_Edit = "User.UserDetail.Edit";
        public const string User_RoleList = "User.RoleList";
        public const string User_RoleDetail = "User.RoleDetail";
        public const string User_RoleDetail_Edit = "User.RoleDetail.Edit";

        //Reports
        public const string Reports = "Reports";
        public const string Reports_List = "Reports.List";

        //Integration Log
        public const string IntegrationLog = "IntegrationLog";
        public const string IntegrationLog_List = "IntegrationLog.List";

        //Cruise Orders
        public const string CruiseOrder = "CruiseOrder";
        public const string CruiseOrder_List = "CruiseOrder.List";
        public const string CruiseOrder_Detail = "CruiseOrder.Detail";
        public const string CruiseOrder_Detail_Add = "CruiseOrder.Detail.Add";
        public const string CruiseOrder_Detail_Edit = "CruiseOrder.Detail.Edit";

        public const string Organization_Detail_Add = "Organization.Detail.Add";

        //Freight Schedulers
        public const string Shipment_FreightScheduler_List = "Shipment.FreightSchedulers";
        public const string Shipment_FreightScheduler_List_Add = "Shipment.FreightSchedulers.Add";
        public const string Shipment_FreightScheduler_List_Edit = "Shipment.FreightSchedulers.Edit";

        // Shipped Purchase Orders
        public const string Shipment_ShippedPO_List = "Shipment.ShippedPOList";
        public const string Shipment_Container_List = "Shipment.ContainerList";

        // Master data
        public const string Organization_Carrier_List = "Organization.CarrierList";
        public const string Organization_Carrier_Detail = "Organization.CarrierDetail";
        public const string Organization_Carrier_Detail_Add = "Organization.CarrierDetail.Add";
        public const string Organization_Carrier_Detail_Edit = "Organization.CarrierDetail.Edit";

        public const string Organization_Vessel_List = "Organization.VesselList";
        public const string Organization_Vessel_Detail = "Organization.VesselDetail";
        public const string Organization_Vessel_Detail_Add = "Organization.VesselDetail.Add";
        public const string Organization_Vessel_Detail_Edit = "Organization.VesselDetail.Edit";

        public const string Organization_Location_List = "Organization.LocationList";
        public const string Organization_Location_Detail = "Organization.LocationDetail";
        public const string Organization_Location_Detail_Add = "Organization.LocationDetail.Add";
        public const string Organization_Location_Detail_Edit = "Organization.LocationDetail.Edit";

        public const string Dashboard_EndToEndShipmentStatus = "Dashboard.EndToEndShipmentStatus";

        // Master Dialog
        public const string Shipment_MasterDialog_List = "Shipment.MasterDialogs";
        public const string Shipment_MasterDialog_List_Add = "Shipment.MasterDialogs.Add";
        public const string Shipment_MasterDialog_List_Edit = "Shipment.MasterDialogs.Edit";

        public const string Shipment_ContainerDetail_Add = "Shipment.ContainerDetail.Add";
        public const string Shipment_ContainerDetail_Edit = "Shipment.ContainerDetail.Edit";

        // Consolidation
        public const string Shipment_ConsolidationList = "Shipment.ConsolidationList";
        public const string Shipment_ConsolidationDetail = "Shipment.ConsolidationDetail";
        public const string Shipment_ConsolidationDetail_Add = "Shipment.ConsolidationDetail.Add";
        public const string Shipment_ConsolidationDetail_Edit = "Shipment.ConsolidationDetail.Edit";
        public const string Shipment_ConsolidationDetail_Confirm = "Shipment.ConsolidationDetail.Confirm";
        public const string Shipment_ConsolidationDetail_Unconfirm = "Shipment.ConsolidationDetail.Unconfirm";

        //BillOfLading
        public const string BillOfLading = "BillOfLading";
        public const string BillOfLading_ListOfHouseBL = "BillOfLading.ListOfHouseBL";
        public const string BillOfLading_HouseBLDetail = "BillOfLading.HouseBLDetail";
        public const string BillOfLading_HouseBLDetail_Add = "BillOfLading.HouseBLDetail.Add";
        public const string BillOfLading_HouseBLDetail_Edit = "BillOfLading.HouseBLDetail.Edit";

        public const string BillOfLading_ListOfMasterBL = "BillOfLading.ListOfMasterBL";
        public const string BillOfLading_MasterBLDetail = "BillOfLading.MasterBLDetail";
        public const string BillOfLading_MasterBLDetail_Add = "BillOfLading.MasterBLDetail.Add";
        public const string BillOfLading_MasterBLDetail_Edit = "BillOfLading.MasterBLDetail.Edit";

        //Shipment
        public const string Shipment_Detail_Edit = "Shipment.Detail.Edit";

        public const string PO_ProgressCheckCRD = "Order.POProgressCheckCRD";

        public const string Shipment_ConsolidationDetail_Delete = "Shipment.ConsolidationDetail.Delete";

        public const string Reports_TaskList = "Reports.TaskList";
        public const string Reports_TaskDetail = "Reports.TaskDetail";
        public const string Reports_TaskDetail_Add = "Reports.TaskDetail.Add";
        public const string Reports_TaskDetail_Edit = "Reports.TaskDetail.Edit";

        //Contract
        public const string Contract_List = "Organization.ContractList";
        public const string Contract_Detail = "Organization.ContractDetail";
        public const string Contract_Detail_Add = "Organization.ContractDetail.Add";
        public const string Contract_Detail_Edit = "Organization.ContractDetail.Edit";

        //Order
        public const string PO_Detail_Close = "Order.PODetail.Close";

        public const string Organization_WarehouseLocation_List = "Organization.WarehouseLocationList";
        public const string Organization_WarehouseLocation_Detail = "Organization.WarehouseLocationDetail";
        public const string Organization_WarehouseLocation_Detail_Add = "Organization.WarehouseLocationDetail.Add";
        public const string Organization_WarehouseLocation_Detail_Edit = "Organization.WarehouseLocationDetail.Edit";

        //Article Master
        public const string Organization_ArticleMaster_List = "Organization.ArticleMasterList";
        public const string Organization_ArticleMaster_Detail = "Organization.ArticleMasterDetail";

        public const string User_UserDetail_Add = "User.UserDetail.Add";

        public const string Dashboard_CategorizedPO = "Dashboard.CategorizedPO";       

        //Balance of Goods
        public const string BalanceOfGoods = "BalanceOfGoods";
        public const string BalanceOfGoods_Enquiry = "BalanceOfGoods.Enquiry";

        //Survey
        public const string Organization_SurveyList = "Organization.SurveyList";
        public const string Organization_SurveyDetail = "Organization.SurveyDetail";
        public const string Organization_SurveyDetail_Add = "Organization.SurveyDetail.Add";
        public const string Organization_SurveyDetail_Edit = "Organization.SurveyDetail.Edit";

        public const string Dashboard_Shortships_List = "Dashboard.ShortshipsList";
        public const string Dashboard_VesselArrival_List = "Dashboard.VesselArrivalList";

        //Routing Order
        public const string RoutingOrder_List = "Order.RoutingOrderList";
        public const string RoutingOrder_Detail = "Order.RoutingOrderDetail";
        public const string RoutingOrder_Detail_Edit = "Order.RoutingOrderDetail.Edit";
        public const string RoutingOrder_Detail_Confirm = "Order.RoutingOrderDetail.Confirm";

        //Master events
        public const string Organization_MasterEvent_List = "Organization.MasterEventList";
        public const string Organization_MasterEvent_List_Add = "Organization.MasterEventList.Add";
        public const string Organization_MasterEvent_List_Edit = "Organization.MasterEventList.Edit";
    }

    public class AppPermissionOrders
    {
        // The inner dictionary.
        // Key: Permission name
        // Value: Order value
        public Dictionary<string, int> PermissionOrderDictionary { get; private set; }

        public AppPermissionOrders()
        {
            // Seed data into dictionary when it has been called by constructor
            PermissionOrderDictionary = new Dictionary<string, int>() { };

            // Dashboard
            PermissionOrderDictionary.Add(AppPermissions.Dashboard, 1001);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_ThisWeekShipments, 1002);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_ThisWeekCustomerPO, 1003);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_ThisWeekOceanVolume, 1004);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_Top10ShipperThisWeek, 1005);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_Top10ConsigneeThisWeek, 1006);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_Top10CarrierThisWeek, 1007);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_MonthlyTop5OceanVolumeByOrigin, 1008);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_MonthlyTop5OceanVolumeByDestination, 1009);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_MonthlyOceanVolumeByMovement, 1010);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_MonthlyOceanVolumeByServiceType, 1011);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_EndToEndShipmentStatus, 1012);
            PermissionOrderDictionary.Add(AppPermissions.Dashboard_CategorizedPO, 1013);

            //Products
            PermissionOrderDictionary.Add(AppPermissions.Product, 1101);
            PermissionOrderDictionary.Add(AppPermissions.Product_List, 1102);
            PermissionOrderDictionary.Add(AppPermissions.Product_Detail, 1103);
            PermissionOrderDictionary.Add(AppPermissions.Product_Detail_Add, 1104);
            PermissionOrderDictionary.Add(AppPermissions.Product_Detail_Edit, 1105);

            //Orders
            PermissionOrderDictionary.Add(AppPermissions.Order, 1201);
            PermissionOrderDictionary.Add(AppPermissions.PO_List, 1202);
            PermissionOrderDictionary.Add(AppPermissions.PO_Detail, 1203);
            PermissionOrderDictionary.Add(AppPermissions.PO_Detail_Add, 1204);
            PermissionOrderDictionary.Add(AppPermissions.PO_Detail_Edit, 1205);
            PermissionOrderDictionary.Add(AppPermissions.PO_Detail_Close, 1206);
            PermissionOrderDictionary.Add(AppPermissions.PO_Delegation, 1207);
            PermissionOrderDictionary.Add(AppPermissions.PO_ProgressCheckCRD, 1208);
            PermissionOrderDictionary.Add(AppPermissions.PO_Fulfillment_List, 1209);
            PermissionOrderDictionary.Add(AppPermissions.PO_Fulfillment_Detail, 1210);
            PermissionOrderDictionary.Add(AppPermissions.PO_Fulfillment_Detail_Add, 1211);
            PermissionOrderDictionary.Add(AppPermissions.PO_Fulfillment_Detail_Edit, 1212);
            PermissionOrderDictionary.Add(AppPermissions.Order_PendingApprovalList, 1213);
            PermissionOrderDictionary.Add(AppPermissions.Order_PendingApproval_Detail, 1214);
            PermissionOrderDictionary.Add(AppPermissions.Order_PendingApproval_Detail_Edit, 1215);
            PermissionOrderDictionary.Add(AppPermissions.RoutingOrder_List, 1216);
            PermissionOrderDictionary.Add(AppPermissions.RoutingOrder_Detail, 1217);
            PermissionOrderDictionary.Add(AppPermissions.RoutingOrder_Detail_Edit, 1218);
            PermissionOrderDictionary.Add(AppPermissions.RoutingOrder_Detail_Confirm, 1219);

            //Cruise Orders
            PermissionOrderDictionary.Add(AppPermissions.CruiseOrder, 1301);
            PermissionOrderDictionary.Add(AppPermissions.CruiseOrder_List, 1302);
            PermissionOrderDictionary.Add(AppPermissions.CruiseOrder_Detail, 1303);
            PermissionOrderDictionary.Add(AppPermissions.CruiseOrder_Detail_Add, 1304);
            PermissionOrderDictionary.Add(AppPermissions.CruiseOrder_Detail_Edit, 1305);

            //Shipments
            PermissionOrderDictionary.Add(AppPermissions.Shipment, 1401);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_List, 1402);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_Detail, 1403);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_Detail_Edit, 1404);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ShippedPO_List, 1405);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_Container_List, 1406);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ContainerDetail, 1407);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ContainerDetail_Add, 1408);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ContainerDetail_Edit, 1409);

            //Consignments
            PermissionOrderDictionary.Add(AppPermissions.Shipment_Consignment_List, 1410);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_Consignment_Detail, 1411);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_Consignment_Detail_Add, 1412);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_Consignment_Detail_Edit, 1413);

            PermissionOrderDictionary.Add(AppPermissions.Shipment_ConsolidationList, 1414);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ConsolidationDetail, 1415);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ConsolidationDetail_Add, 1416);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ConsolidationDetail_Edit, 1417);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ConsolidationDetail_Delete, 1418);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ConsolidationDetail_Confirm, 1419);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_ConsolidationDetail_Unconfirm, 1420);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_FreightScheduler_List, 1421);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_FreightScheduler_List_Add, 1422);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_FreightScheduler_List_Edit, 1423);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_Detail_Confirm_Itinerary, 1424);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_MasterDialog_List, 1425);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_MasterDialog_List_Add, 1426);
            PermissionOrderDictionary.Add(AppPermissions.Shipment_MasterDialog_List_Edit, 1427);


            //BillOfLading
            PermissionOrderDictionary.Add(AppPermissions.BillOfLading, 1501);
            PermissionOrderDictionary.Add(AppPermissions.BillOfLading_ListOfHouseBL, 1502);
            PermissionOrderDictionary.Add(AppPermissions.BillOfLading_HouseBLDetail, 1503);
            PermissionOrderDictionary.Add(AppPermissions.BillOfLading_HouseBLDetail_Add, 1504);
            PermissionOrderDictionary.Add(AppPermissions.BillOfLading_HouseBLDetail_Edit, 1505);

            PermissionOrderDictionary.Add(AppPermissions.BillOfLading_ListOfMasterBL, 1506);
            PermissionOrderDictionary.Add(AppPermissions.BillOfLading_MasterBLDetail, 1507);
            PermissionOrderDictionary.Add(AppPermissions.BillOfLading_MasterBLDetail_Add, 1508);
            PermissionOrderDictionary.Add(AppPermissions.BillOfLading_MasterBLDetail_Edit, 1509);

            //Invoice
            PermissionOrderDictionary.Add(AppPermissions.Invoice, 1601);
            PermissionOrderDictionary.Add(AppPermissions.Invoice_List, 1602);

            //Organizations
            PermissionOrderDictionary.Add(AppPermissions.Organization, 1701);
            PermissionOrderDictionary.Add(AppPermissions.Organization_List, 1702);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Detail, 1703);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Detail_Add, 1704);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Detail_Edit, 1705);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Compliance_List, 1706);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Compliance_Detail, 1707);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Compliance_Detail_Edit, 1708);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Carrier_List, 1709);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Carrier_Detail, 1710);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Carrier_Detail_Add, 1711);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Carrier_Detail_Edit, 1712);

            PermissionOrderDictionary.Add(AppPermissions.Contract_List, 1713);
            PermissionOrderDictionary.Add(AppPermissions.Contract_Detail, 1714);
            PermissionOrderDictionary.Add(AppPermissions.Contract_Detail_Add, 1715);
            PermissionOrderDictionary.Add(AppPermissions.Contract_Detail_Edit, 1716);


            PermissionOrderDictionary.Add(AppPermissions.Organization_Vessel_List, 1717);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Vessel_Detail, 1718);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Vessel_Detail_Add, 1719);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Vessel_Detail_Edit, 1720);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Location_List, 1721);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Location_Detail, 1722);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Location_Detail_Add, 1723);
            PermissionOrderDictionary.Add(AppPermissions.Organization_Location_Detail_Edit, 1724);
            PermissionOrderDictionary.Add(AppPermissions.Organization_WarehouseLocation_List, 1725);
            PermissionOrderDictionary.Add(AppPermissions.Organization_WarehouseLocation_Detail, 1726);
            PermissionOrderDictionary.Add(AppPermissions.Organization_WarehouseLocation_Detail_Add, 1727);
            PermissionOrderDictionary.Add(AppPermissions.Organization_WarehouseLocation_Detail_Edit, 1728);
            PermissionOrderDictionary.Add(AppPermissions.Organization_ArticleMaster_List, 1729);
            PermissionOrderDictionary.Add(AppPermissions.Organization_ArticleMaster_Detail, 1730);
            PermissionOrderDictionary.Add(AppPermissions.Organization_MasterEvent_List, 1735);
            PermissionOrderDictionary.Add(AppPermissions.Organization_MasterEvent_List_Add, 1736);
            PermissionOrderDictionary.Add(AppPermissions.Organization_MasterEvent_List_Edit, 1737);

            //Users
            PermissionOrderDictionary.Add(AppPermissions.User, 1801);
            PermissionOrderDictionary.Add(AppPermissions.User_RequestList, 1802);
            PermissionOrderDictionary.Add(AppPermissions.User_RequestDetail, 1803);
            PermissionOrderDictionary.Add(AppPermissions.User_RequestDetail_Edit, 1804);
            PermissionOrderDictionary.Add(AppPermissions.User_UserList, 1805);
            PermissionOrderDictionary.Add(AppPermissions.User_UserDetail, 1806);
            PermissionOrderDictionary.Add(AppPermissions.User_UserDetail_Add, 1807);
            PermissionOrderDictionary.Add(AppPermissions.User_UserDetail_Edit, 1808);
            PermissionOrderDictionary.Add(AppPermissions.User_RoleList, 1809);
            PermissionOrderDictionary.Add(AppPermissions.User_RoleDetail, 1810);
            PermissionOrderDictionary.Add(AppPermissions.User_RoleDetail_Edit, 1811);

            //Reports
            PermissionOrderDictionary.Add(AppPermissions.Reports, 1901);
            PermissionOrderDictionary.Add(AppPermissions.Reports_List, 1902);
            PermissionOrderDictionary.Add(AppPermissions.Reports_TaskList, 1903);
            PermissionOrderDictionary.Add(AppPermissions.Reports_TaskDetail, 1904);
            PermissionOrderDictionary.Add(AppPermissions.Reports_TaskDetail_Add, 1905);
            PermissionOrderDictionary.Add(AppPermissions.Reports_TaskDetail_Edit, 1906);

            //Integration Log
            PermissionOrderDictionary.Add(AppPermissions.IntegrationLog, 2001);
            PermissionOrderDictionary.Add(AppPermissions.IntegrationLog_List, 2002);

            // Balance of Goods
            PermissionOrderDictionary.Add(AppPermissions.BalanceOfGoods, 2101);
            PermissionOrderDictionary.Add(AppPermissions.BalanceOfGoods_Enquiry, 2102);

        }
    }

    public static class AppPolicies
    {
        public const string ImportClientOnly = "ImportClientOnly";
    }
}
