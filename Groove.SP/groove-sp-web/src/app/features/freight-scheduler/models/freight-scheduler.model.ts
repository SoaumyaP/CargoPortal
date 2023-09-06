import { LocalDateTime } from "src/app/core/models/local-date-time.model";
import { DataType, Model, ModelBase } from "src/app/core/models/model-base.model";

@Model
export class FreightSchedulerModel extends ModelBase {
    id: number = 0;
    modeOfTransport: string = undefined;
    carrierCode: string = undefined;
    carrierName: string = undefined;
    vesselName: string = undefined;
    voyage: string = undefined;
    mawb: string = undefined;
    flightNumber: string = undefined;
    locationFromCode: string = undefined;
    locationFromName: string = undefined;
    locationToCode: string = undefined;
    locationToName: string = undefined;
    isAllowExternalUpdate: boolean = true;
    @DataType(LocalDateTime)
    etdDate: LocalDateTime = undefined;
    @DataType(LocalDateTime)
    etaDate: LocalDateTime = undefined;
    @DataType(LocalDateTime)
    atdDate: LocalDateTime = undefined;
    @DataType(LocalDateTime)
    ataDate: LocalDateTime = undefined;

    @DataType(LocalDateTime)
    cyOpenDate: LocalDateTime = undefined;
    @DataType(LocalDateTime)
    cyClosingDate: LocalDateTime = undefined;
    updatedBy: string = null;
    updatedDate: string | null = null;
    isHasLinkedItineraries:boolean = false;
}
