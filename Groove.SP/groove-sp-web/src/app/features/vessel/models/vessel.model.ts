import { VesselStatus } from "src/app/core";

export class VesselModel {
    id: number;
    name: string;
    code: string;
    isRealVessel: boolean;
    status: VesselStatus

    constructor() {
        this.status = VesselStatus.Active;
        this.isRealVessel = true;
    }
}
