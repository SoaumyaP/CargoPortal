export interface BaseModel {
    createdBy: string;
    createdDate: string;
    updatedBy?: string | null;
    updatedDate?: string | null;
}
