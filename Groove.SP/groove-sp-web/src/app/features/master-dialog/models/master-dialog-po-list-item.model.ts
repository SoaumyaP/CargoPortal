export interface MasterDialogPOListItemModel {
    text: string;
    value: string;
    parentId?: string;
    dialogItemNumber?: string;
    childrenItems?: MasterDialogPOListItemModel[];
    isChecked: boolean;
    isDisabled: boolean;
    recordCount: number;
}
