import { Injectable } from '@angular/core';
import { DropDownListItemModel, HttpService, POFulfillmentStageType, POFulfillmentStatus } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class POFulfillmentListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/pofulfillments`);
        this.defaultState.sort = [
            { field: 'createdDate', dir: 'desc' },
            { field: 'number', dir: 'desc' }
        ];

        this.defaultState.filter = {
            logic: 'and',
            filters: [
                {
                    field: 'stage',
                    operator: 'multiselect',
                    value: `${POFulfillmentStageType.Draft},${POFulfillmentStageType.ForwarderBookingRequest},${POFulfillmentStageType.ForwarderBookingConfirmed},${POFulfillmentStageType.CargoReceived},${POFulfillmentStageType.ShipmentDispatch},${POFulfillmentStageType.Closed}`
                },
                {
                    field: 'status',
                    operator: 'eq',
                    value: POFulfillmentStatus.Active
                }
            ]
        };
    }

    populateSelectedStage(value: string): Array<DropDownListItemModel<number>> {
        const selectedStageModel: Array<DropDownListItemModel<number>> = [];

        if (!value) {
            return;
        }

        for (const item of this.poFulfillmentStage) {
            if (value?.toString().indexOf(item.value.toString()) !== -1) {
                selectedStageModel.push({
                    text: item.text,
                    value: item.value
                });
            }
        }
        return selectedStageModel;
    }
}
