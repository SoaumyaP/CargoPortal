namespace Groove.CSFE.Application.Authorization
{
    public static class AppPermissions
    {
        //Dashboard
        public const string Dashboard = "Dashboard";
        public const string Dashboard_ExceptionShipment = "Dashboard.ExceptionShipment";
        public const string Dashboard_CountShipment = "Dashboard.CountShipment";
        public const string Dashboard_CountContainer = "Dashboard.CountContainer";
        public const string Dashboard_Top5ShipFromLocation = "Dashboard.Top5ShipFromLocation";
        public const string Dashboard_Top5ShipToLocation = "Dashboard.Top5ShipToLocation";

        //Shipments
        public const string Shipment = "Shipment";
        public const string Shipment_List = "Shipment.List";
        public const string Shipment_Detail = "Shipment.Detail";
        public const string Shipment_ContainerDetail = "Shipment.ContainerDetail";
        public const string Shipment_BillOfLadingDetail = "Shipment.BillOfLadingDetail";

        //OceanBillOfLading
        public const string OceanBillOfLading = "OceanBillOfLading";
        public const string OceanBillOfLading_List = "OceanBillOfLading.List";
        public const string OceanBillOfLading_Detail = "OceanBillOfLading.Detail";

        //Consignments
        public const string Consignment = "Consignment";
        public const string Consignment_List = "Consignment.List";

        //Invoice
        public const string Invoice = "Invoice";
        public const string Invoice_List = "Invoice.List";

        //Vessel
        public const string Organization_Vessel_List = "Organization.VesselList";
        public const string Organization_Vessel_Detail = "Organization.VesselDetail";
        public const string Organization_Vessel_Detail_Add = "Organization.VesselDetail.Add";
        public const string Organization_Vessel_Detail_Edit = "Organization.VesselDetail.Edit";

        //Organization
        public const string Organization = "Organization";
        public const string Organization_List = "Organization.List";
        public const string Organization_Detail = "Organization.Detail";
        public const string Organization_Detail_Edit = "Organization.Detail.Edit";
        public const string Organization_Compliance_Detail_Edit = "Organization.ComplianceDetail.Edit";
        public const string Organization_Carrier_List = "Organization.CarrierList";
        public const string Organization_Carrier_Detail = "Organization.CarrierDetail";
        public const string Organization_Carrier_Detail_Add = "Organization.CarrierDetail.Add";
        public const string Organization_Carrier_Detail_Edit = "Organization.CarrierDetail.Edit";

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

        //Orders
        public const string PO_Delegation = "Order.PODelegation";
        public const string PO_Detail_Edit = "Order.PODetail.Edit";
        public const string PO_Fulfillment_Detail = "Order.POFulfillmentDetail";
        public const string PO_Fulfillment_Detail_Edit = "Order.POFulfillmentDetail.Edit";

        //Locations
        public const string Organization_Location_List = "Organization.LocationList";
        public const string Organization_Location_Detail = "Organization.LocationDetail";
        public const string Organization_Location_Detail_Add = "Organization.LocationDetail.Add";
        public const string Organization_Location_Detail_Edit = "Organization.LocationDetail.Edit";

        //Warehouse locations
        public const string Organization_WarehouseLocation_List = "Organization.WarehouseLocationList";
        public const string Organization_WarehouseLocation_Detail = "Organization.WarehouseLocationDetail";
        public const string Organization_WarehouseLocation_Detail_Add = "Organization.WarehouseLocationDetail.Add";
        public const string Organization_WarehouseLocation_Detail_Edit = "Organization.WarehouseLocationDetail.Edit";

        //Master events
        public const string Organization_MasterEvent_List = "Organization.MasterEventList";
        public const string Organization_MasterEvent_List_Add = "Organization.MasterEventList.Add";
        public const string Organization_MasterEvent_List_Edit = "Organization.MasterEventList.Edit";
    }

    public static class AppPolicies
    {
        public const string ImportClientOnly = "ImportClientOnly";
    }
}
