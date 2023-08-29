import { EventCodeStatus, EventOrderType } from "src/app/core";

export interface EventCodeModel {
    activityCode: string;
    activityTypeCode: string;
    activityDescription: string;
    locationRequired: boolean | string;
    remarkRequired: boolean | string;
    sortSequence: number;
    eventOrderType: EventOrderType;
    beforeEvent: string;
    afterEvent: string;
    status: EventCodeStatus;
}