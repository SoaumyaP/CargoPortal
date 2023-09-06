import { Injectable } from '@angular/core';
import { DropdownListModel, HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class ShipmentListService extends ListService {
    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/shipments`);
        this.defaultState.sort = [
            { field: 'shipFromETDDate', dir: 'asc' }
        ];
        this.defaultState.filter = {
            logic: 'and',
            filters: [{
                field: 'activityCode',
                operator: 'multiselect',
                value: '2005,2014,7001,7003,7003,7001,7002,7004'
            }]
        };
    }

    getExceptionList(idList) {
        return this.httpService.get(`${environment.apiUrl}/shipments/exceptions?idList=[${idList}]`);
    }

    initializeShipmentMilestoneDropdown(): Array<DropdownListModel<string>> {
        return [
            {
                label: 'label.shipmentBooked',
                value: '2005'
            },
            {
                label: 'label.handoverFromShipper',
                value: '2014'
            },
            {
                label: 'label.departureFromPort',
                value: '7001,7003'
            },
            {
                label: 'label.inTransit',
                value: '7003,7001'
            },
            {
                label: 'label.arrivalAtPort',
                value: '7002,7004'
            },
            {
                label: 'label.handoverToConsignee',
                value: '2054'
            }
        ];
    }

    populateSelectedMilestone(value: string): Array<DropdownListModel<string>> {
        const milestoneDropdown = this.initializeShipmentMilestoneDropdown();

        const selectedMilestoneModel: Array<DropdownListModel<string>> = [];

        if (!value) {
            return;
        }

        for (const item of milestoneDropdown) {
            if (value.indexOf(item.value) !== -1) {
                selectedMilestoneModel.push({
                    label: item.label,
                    value: item.value
                });
            }
        }
        return selectedMilestoneModel;
    }
}
