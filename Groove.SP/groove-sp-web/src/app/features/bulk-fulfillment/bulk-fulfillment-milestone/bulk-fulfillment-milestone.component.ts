import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { POFulfillmentStageType } from 'src/app/core/models/enums/enums';

@Component({
    selector: 'app-bulk-fulfillment-milestone',
    templateUrl: './bulk-fulfillment-milestone.component.html',
    styleUrls: ['./bulk-fulfillment-milestone.component.scss']
})
export class BulkFulfillmentMilestoneComponent implements OnInit, OnChanges {

    @Input() currentStage: POFulfillmentStageType;

    stages = [
        {
            stage: POFulfillmentStageType.Draft,
            title: 'label.draft',
            class: 'n-draft',
            active: false,
            current: false
        },
        {
            stage: POFulfillmentStageType.ForwarderBookingRequest,
            title: 'label.forwarderBookingRequest',
            class: 'n-forwarderBookingRequest',
            active: false,
            current: false
        },
        {
            stage: POFulfillmentStageType.ForwarderBookingConfirmed,
            title: 'label.forwarderBookingConfirmed',
            class: 'n-forwarderBookingConfirmed',
            active: false,
            current: false
        },
        {
            stage: POFulfillmentStageType.ShipmentDispatch,
            title: 'label.shipmentDispatch',
            class: 'n-shipmentDispatch',
            active: false,
            current: false
        },
        {
            stage: POFulfillmentStageType.Closed,
            title: 'label.closed',
            class: 'n-closed',
            active: false,
            current: false
        }
    ];

    constructor() {
        this.currentStage = POFulfillmentStageType.Draft;
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes.currentStage.currentValue) {
            this.updateState();
        }
    }

    private updateState(): void {
        for (const item of this.stages) {
            item.active = item.stage <= this.currentStage;
            item.current = item.stage === this.currentStage;
        }
    }

    ngOnInit() {
    }

}
