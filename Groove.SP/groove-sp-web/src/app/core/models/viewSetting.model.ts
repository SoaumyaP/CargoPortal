export interface ViewSettingModel {
    field: string;
    viewType: ViewSettingType;
    moduleId: string;
}

export enum ViewSettingType {
    Field = 10,
    Column = 20
}