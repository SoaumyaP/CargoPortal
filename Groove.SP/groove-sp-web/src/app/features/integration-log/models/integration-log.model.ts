export interface IntegrationLogModel {
    name: string;
    profile: string;
    aPIName: string;
    aPIMessage: string;
    eDIMessageType: string;
    eDIMessageRef: string;
    postingDate: string;
    status: IntegrationLogStatus;
    remark: string;
    response: string;
    responseJsonPreview: string;
}

export enum IntegrationLogStatus {
    Succeed = 1,
    Failed = 2
}
