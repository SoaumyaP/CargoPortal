import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { FormService, HttpService } from 'src/app/core';

@Injectable()
export class MasterDialogFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/masterdialogs`);
    }

    getDummyPO(): any[] {
        let listOfPOItems = [];
        for (let i = 1; i <= 50; i++) {
            listOfPOItems.push({
                text: `PO ${i}`,
                id: i,
                isDisabled: false,
                isChecked: false,
                childrenItems: this.getDummyChildrenPO(i)
            })
        }

        // for (let i = 0; i < 20; i++) {
        //     listOfPOItems.push({
        //         label: `PO ${2}`,
        //         id: i,
        //         isDisabled: false,
        //         isChecked: false,
        //         childrenItem: this.getDummyChildrenPO(i)
        //     })
        // }

        return listOfPOItems;
    }

    private getDummyChildrenPO(parentId) {
        let childrenItem = [];
        for (let index = 0; index <= 2; index++) {
            childrenItem.push({
                text: `Item ${parentId}`,
                isDisabled: false,
                isChecked: false,
                parentId: parentId
            })
        }
        return childrenItem;
    }

    create(viewModel: any) {
        return this.httpService.create(`${environment.apiUrl}/masterdialogs`, viewModel);
    }

    update(id: any, viewModel: any) {
        return this.httpService.update(`${environment.apiUrl}/masterdialogs/${id}`, viewModel);
    }

    searchNumberByFilterCriteria(value: string, criteria: string) {
        return this.httpService.get(`${environment.apiUrl}/masterdialogs/searchNumberByFilterCriteria?filtercriteria=${criteria}&filtervalue=${value}`);
    }

    searchListOfPurchaseOrders(messageShownOn: string, filterCriteria: string, filterValue: string, searchTerm: string, skip: number = 0, take: number = 20) {
        return this.httpService.get(`${environment.apiUrl}/masterdialogs/searchListOfPurchaseOrders?messageShownOn=${messageShownOn}&filterCriteria=${filterCriteria}&filterValue=${filterValue}&searchTerm=${searchTerm}&skip=${skip}&take=${take}`);
    }

    searchListOfPurchaseOrdersByMasterDialogId(masterDialogId: number, searchTerm: string, skip: number = 0, take: number = 20, priorityEnable: boolean = false) {
        return this.httpService.get(`${environment.apiUrl}/masterdialogs/${masterDialogId}/searchListOfPurchaseOrdersById?searchTerm=${searchTerm}&skip=${skip}&take=${take}`);
    }
}
