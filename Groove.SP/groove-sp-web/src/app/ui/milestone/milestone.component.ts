/**
 * Component explanation: Each milestone will be mapped with an activity code and it will be highlighted as the activity is existing.
 */
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { DATE_FORMAT, MilestoneType } from 'src/app/core';
import { DatePipe } from '@angular/common';

@Component({
    selector: 'app-milestone',
    templateUrl: './milestone.component.html',
    styleUrls: ['./milestone.component.scss']
})
export class MilestoneComponent implements OnInit {

    @Input()
    data: any[] = new Array<any>();

    @Input()
    type: number;

    @Input()
    isInitDataLoaded: boolean = false;
    result = [];

    initDataShipmentFreightTracking() {
        return  [
            {
                activityCode: '2005',
                title: 'label.shipmentBooked',
                class: 'n-shipment',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '2014',
                title: 'label.handoverFromShipper',
                class: 'n-handover-shipper',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '7001',
                title: 'label.departureFromPort',
                class: 'n-departure',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '7001',
                dependent: true,
                title: 'label.inTransit',
                class: 'n-in-transit',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '7002',
                title: 'label.arrivalAtPort',
                class: 'n-arrival',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '2054',
                title: 'label.handoverToConsignee',
                class: 'n-handover-consignee',
                activityDate: null,
                active: false,
                current: false
            }
        ];
    }

    initDataAirFreightShipmentTracking() {
        return  [
            {
                activityCode: '2005',
                title: 'label.shipmentBooked',
                class: 'n-shipment',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '2014',
                title: 'label.handoverFromShipper',
                class: 'n-handover-shipper',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '7003',
                title: 'label.departureFromPort',
                class: 'n-departure',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '7003',
                dependent: true,
                title: 'label.inTransit',
                class: 'n-in-transit',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '7004',
                title: 'label.arrivalAtPort',
                class: 'n-arrival',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '2054',
                title: 'label.handoverToConsignee',
                class: 'n-handover-consignee',
                activityDate: null,
                active: false,
                current: false
            }
        ];
    }

    initDataCruiseOrderSeaOrOcean () {
        return [
            {
                activityCode: '5003',
                title: 'label.cruiseOrder.bookedForVendor',
                class: 'n-shipment',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5005',
                title: 'label.cruiseOrder.confirm',
                class: 'n-handover-shipper',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5006',
                title: 'label.cruiseOrder.shipped',
                class: 'n-departure',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5008',
                title: 'label.cruiseOrder.transhipment',
                class: 'n-in-transit',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5010',
                title: 'label.delivered',
                class: 'n-handover-consignee',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5013',
                title: 'label.cruiseOrder.emptyContainerReturned',
                class: 'n-empty-return',
                activityDate: null,
                active: false,
                current: false
            }
        ];
    }

    initDataCruiseOrderAirOrCourier () {
        return [
            {
                activityCode: '5000',
                title: 'label.cruiseOrder.pickupArranged',
                class: 'n-shipment',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5001',
                title: 'label.cruiseOrder.goodsReceived',
                class: 'n-handover-shipper',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5002',
                title: 'label.cruiseOrder.authorizedToShip',
                class: 'n-cruise-au',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5006',
                title: 'label.cruiseOrder.shipped',
                class: 'n-in-transit',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5009',
                title: 'label.cruiseOrder.arrived',
                class: 'n-cruise-arrived',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5010',
                title: 'label.delivered',
                class: 'n-handover-consignee',
                activityDate: null,
                active: false,
                current: false
            }
        ];
    }

    initDataCruiseOrderRoad () {
        return  [
            {
                activityCode: '5000',
                title: 'label.cruiseOrder.pickupArranged',
                class: 'n-shipment',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5007',
                title: 'label.inTransit',
                class: 'n-in-transit',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5001',
                title: 'label.cruiseOrder.goodsReceived',
                class: 'n-handover-shipper',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5006',
                title: 'label.cruiseOrder.shipped',
                class: 'n-cruise-road-shipped',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '5010',
                title: 'label.delivered',
                class: 'n-handover-consignee',
                activityDate: null,
                active: false,
                current: false
            }
        ];
    }

    // please define init milestone container.
    initDataContainer () {
        return [
            {
                activityCode: '3001',
                title: 'label.gateIn',
                class: 'n-gate-in',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '3002',
                title: 'label.vesselLoad',
                class: 'n-vessel-load',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '7001',
                title: 'label.portDeparture',
                class: 'n-port-departure',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '7002',
                title: 'label.portArrival',
                class: 'n-port-arrival',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '3005',
                title: 'label.vesselUnload',
                class: 'n-vessel-unload',
                activityDate: null,
                active: false,
                current: false
            },
            {
                activityCode: '3006',
                title: 'label.gateOut',
                class: 'n-gate-out',
                activityDate: null,
                active: false,
                current: false
            }
        ];
    }

    constructor(private datePipe: DatePipe) {

    }

    ngOnInit() {
      this.reload();
    }

    reload() {
        if (this.type === MilestoneType.ShipmentFreight) {
            this.processDataMilestone(this.initDataShipmentFreightTracking());
        }
        else if (this.type === MilestoneType.ShipmentCruiseSeaOrOcean) {
            this.processDataMilestone(this.initDataCruiseOrderSeaOrOcean());
        } 
        else if (this.type === MilestoneType.ShipmentCruiseAirOrCourier) {
            this.processDataMilestone(this.initDataCruiseOrderAirOrCourier());
        } 
        else if (this.type === MilestoneType.ShipmentCruiseRoad) {
            this.processDataMilestone(this.initDataCruiseOrderRoad());
        }  
        else if (this.type === MilestoneType.Container) {
            this.processDataMilestone(this.initDataContainer());
        } 
        else if (this.type === MilestoneType.AirFreightShipment) {
            this.processDataMilestone(this.initDataAirFreightShipmentTracking());
        } 
    }

    processDataMilestone(initData) {
        this.result = [];
        initData.forEach((m) => {
            const independent = m.dependent === false || m.dependent === undefined;
            const item = this.data.find(i => i.activityCode === m.activityCode);
            if (item != null) {
                m.active = true;
                m.current = true;

                if (independent) {
                    m.activityDate = this.datePipe.transform(item.activityDate, DATE_FORMAT);
                }
                // set current prev item = false
                this.result.forEach(r => {
                    if (independent) {
                        r.current = false;
                    }
                    r.active = true;
                });
            }          
            this.result.push(m);
        });
    }
}