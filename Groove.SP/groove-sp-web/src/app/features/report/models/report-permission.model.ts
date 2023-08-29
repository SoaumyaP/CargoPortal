export interface ReportPermissionModel {
    reportId: number;
    organizationIds: string;
    grantInternal: boolean;
    grantPrincipal: boolean;
    grantShipper: boolean;
    grantAgent: boolean;
    grantWarehouse: boolean;
    createdBy: string;
    createdDate: Date;
}