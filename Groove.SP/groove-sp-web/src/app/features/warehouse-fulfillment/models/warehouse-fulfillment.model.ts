import {
    FulfillmentType,
    POFulfillmentStageType,
    POFulfillmentStatus
} from "src/app/core";
import { LocalDateTime } from "src/app/core/models/local-date-time.model";
import { DataType, Model, ModelBase } from "src/app/core/models/model-base.model";

@Model
export class WarehouseFulfillmentModel extends ModelBase {
    id: number = 0;

    number: string | null = null;

    stage: POFulfillmentStageType = POFulfillmentStageType.Draft;

    status: POFulfillmentStatus = POFulfillmentStatus.Active;

    statusName: string | null = null;

    isRejected: boolean = false;

    // General
    owner: string = null;

    customerPrefix: string = "";

    @DataType(LocalDateTime)
    cargoReadyDate: LocalDateTime = null;

    @DataType(LocalDateTime)
    expectedDeliveryDate: LocalDateTime = null;

    bookedBy: string = null;

    preferredCarrier: number = null;

    plantNo: string | null = null;

    incoterm: string | null = null;

    remarks: string = null;

    currentOrganization: any = null;

    deliveryMode: string | null = null;

    @DataType(LocalDateTime)
    actualTimeArrival: LocalDateTime = null;

    containerNo: string | null = null;

    companyNo: string | null = null;

    hawbNo: string | null = null;

    forwarder: string | null = null;

    nameofInternationalAccount: string | null = null;

    @DataType(LocalDateTime)
    etdOrigin: LocalDateTime = null;

    @DataType(LocalDateTime)
    etaDestination: LocalDateTime = null;

    fulfillmentType: FulfillmentType = FulfillmentType.Warehouse;

    isFulfilledFromPO: boolean = true;

    shipFromName: string | null = null;

    confirmBy: string = null;
    confirmedAt: string = null;
    @DataType(LocalDateTime)
    confirmedHubArrivalDate: LocalDateTime = null;
    time:string = null;
    loadingBay:string = null;
    soNo:string=null;
    locationName :string = null;
    locationCode :string = null;
    addressLine1 :string = null;
    addressLine2 :string = null;
    addressLine3 :string = null;
    addressLine4 :string = null;
    city :string = null;
    country :string = null;
    contactPhone :string = null;
    contactEmail :string = null;
    contactPerson :string = null;

    attachments: any[] = [];

    contacts: any[] = [];

    orders: any[] = [];

    buyerApprovals: any[] = [];
}
