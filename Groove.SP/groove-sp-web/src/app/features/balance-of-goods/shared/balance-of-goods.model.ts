export interface BalanceOfGoodModel {
    principleId?: number;
    principleCode?: string;
    principleName?: string;
    articleId?: number;
    articleCode?: string;
    articleName?: string;
    warehouseId?: number;
    warehouseCode?: string;
    warehouseName?: string;
    locationId?: number;
    locationName?: string;
    availableQuantity?: number;
    receivedQuantity?: number;
    shippedQuantity?: number;
    adjustQuantity?: number;
    damageQuantity?: number;
}
