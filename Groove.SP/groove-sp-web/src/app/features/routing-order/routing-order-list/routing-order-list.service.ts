import { Injectable } from '@angular/core';
import { DropDownListItemModel, HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class RoutingOrderListService extends ListService {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/routingOrders`);
        this.defaultState.sort = [
            { field: 'routingOrderDate', dir: 'desc' }
        ];
    }

    populateSelectedStage(value: string): Array<DropDownListItemModel<number>> {
        const selectedStageModel: Array<DropDownListItemModel<number>> = [];

        if (!value) {
            return;
        }

        for (const item of this.routingOrderStageType) {
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