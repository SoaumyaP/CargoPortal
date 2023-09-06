import { WarehouseLocationModel } from "../warehouse-locations/warehouse-location.model";

export interface WarehouseAssignmentModel {
    warehouseLocationId: number;
    organizationId: number;
    contactPerson: string;
    contactPhone: string;
    contactEmail: string;
    warehouseLocation: WarehouseLocationModel;

    // using on UI

    /**to mark this as being adding */
    isAddLine: boolean;
}