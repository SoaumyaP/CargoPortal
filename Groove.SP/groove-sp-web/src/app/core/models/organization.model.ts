export class OrganizationReferenceDataModel {
    id: number;
    code: string;
    name: string;
    orgCodeName: string;
    address: string;
    addressLine2: string;
    addressLine3: string;
    addressLine4: string;
    contactEmail: string;
    contactName: string;
    contactNumber: string;
    customerPrefix: string;
    isBuyer: boolean;
    organizationType: number;
    organizationTypeName: string;
    weChatOrWhatsApp: string;
    status: number;
    statusName: string;
}

/**Info of user's organization in dbo.UserProfiles */
export class UserOrganizationProfileModel {
    organizationId: number | null;
    organizationCode: string;
    organizationName: string;
    organizationRoleId: number | null;
    organizationType: string;
}