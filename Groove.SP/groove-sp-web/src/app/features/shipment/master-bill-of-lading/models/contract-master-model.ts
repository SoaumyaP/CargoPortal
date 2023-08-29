import { BaseModel } from 'src/app/core/models/base.model';

export interface ContractMasterModel  extends BaseModel {
    id: number;
    carrierContractNo: string;
    carrierCode: string;
    realContractNo: string;
    accountName: string;
    contractType: string;
    coloaderCode: string;
    contractHolder: string;
    validFrom: string;
    validTo: string;
    status: ContractMasterStatus;
    isVIP: boolean | null;
    remarks: string;
    parentContract: string;
}

export enum ContractMasterStatus {
    Active = 1,
    Inactive = 0
}
