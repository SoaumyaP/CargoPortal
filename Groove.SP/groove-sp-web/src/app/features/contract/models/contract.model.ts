import { ContractMasterStatus } from "src/app/core";

export class ContractModel {
    id: number;
    realContractNo: string;
    carrierContractNo: string;
    carrierCode: string;
    contractType: string;
    coloaderCode: string;
    contractHolder: string;
    validFromDate: string;
    validToDate: string;
    status: ContractMasterStatus;
}