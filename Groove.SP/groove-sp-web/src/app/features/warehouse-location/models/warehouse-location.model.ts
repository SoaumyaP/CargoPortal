
export interface WarehouseLocationModel {
    id: number;
    code: string;
    name: string;
    addressLine1: string;
    addressLine2: string;
    addressLine3: string;
    addressLine4: string;
    contactPerson: string;
    contactPhone: string;
    contactEmail: string;
    locationId: number;
    organizationId: number;
    organizationName: string;
    workingHours: string;
    remarks: string;


    countryId?: number;
}
