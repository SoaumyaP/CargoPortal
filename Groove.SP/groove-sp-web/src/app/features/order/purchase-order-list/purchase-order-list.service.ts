import { Injectable } from '@angular/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { BuyerComplianceStatus, BuyerComplianceStage, HttpService, DropdownListModel, DropDownListItemModel, POStageType } from 'src/app/core';
import { find, map } from 'rxjs/operators';

@Injectable()
export class PurchaseOrderListService extends ListService {
    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/purchaseOrders`);
        this.defaultState.sort = [
            { field: 'createdDate', dir: 'desc' }
        ];

        this.defaultState.filter = {
            logic: 'and',
            filters: [{
                field: 'stage',
                operator: 'multiselect',
                value: `${POStageType.Released},${POStageType.ForwarderBookingRequest},${POStageType.ForwarderBookingConfirmed},${POStageType.CargoReceived},${POStageType.ShipmentDispatch},${POStageType.Closed}`
            },
            {
                field: 'status',
                operator: 'eq',
                value: 1
            }]
        };
    }

    getAllPOForProgressCheck(){
        return this.httpService.get<any[]>(`${environment.apiUrl}/purchaseOrders/progressCheck/search`);
    }

    getActiveBuyerCompliance(customerId: any): Observable<any> {
        return this.httpService.get<any[]>(`${environment.apiUrl}/buyercompliances`, { organizationId: customerId }).pipe(
            map(compliances => compliances.find(bc => bc.status == BuyerComplianceStatus.Active && bc.stage == BuyerComplianceStage.Activated))
        );
    }

    checkParentInCustomerRelationship(organizationId): Observable<boolean>{
        return this.httpService.get<boolean>(`${environment.commonApiUrl}/organizations/${organizationId}/checkParentInCustomerRelationship`);
    }

    populateSelectedStage(value: string): Array<DropDownListItemModel<number>> {
        const selectedStageModel: Array<DropDownListItemModel<number>> = [];

        if (!value) {
            return;
        }

        for (const item of this.poStageType) {
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
