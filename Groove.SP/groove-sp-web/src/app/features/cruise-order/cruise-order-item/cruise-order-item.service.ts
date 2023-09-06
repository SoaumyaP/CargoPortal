import { Injectable, Type } from "@angular/core";
import {
    HttpService,
    FormService,
    DropDownListItemModel,
    DateHelper,
} from "src/app/core";
import { environment } from "src/environments/environment";
import { Observable } from "rxjs";
import { NoteModel } from "src/app/core/models/note.model";
import { CruiseOrderWarehouseModel } from "../models/cruise-order-warehouse.model";
import { CruiseOrderItemModel, ReviseCruiseOrderItemModel } from "../models/cruise-order-item.model";
import { CruiseOrderModel } from "../models/cruise-order.model";
import { map } from "rxjs/operators";
import 'reflect-metadata';
import { ModelBase } from "src/app/core/models/model-base.model";

@Injectable()
export class CruiseOrderItemService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/cruiseOrderItems`);
    }

    getWarehouse(
        cruiseOrderItemId: number
    ): Observable<CruiseOrderWarehouseModel> {
        return this.httpService.get(
            `${environment.apiUrl}/cruiseOrderItems/${cruiseOrderItemId}/warehouses`
        );
    }

    getNotes(cruiseOrderItemId: number): Observable<NoteModel[]> {
        return this.httpService.get(
            `${environment.apiUrl}/cruiseOrderItems/${cruiseOrderItemId}/notes`
        );
    }

    searchShipmentSelectionOptions(
        shipmentNumber: string,
        cruiseOrderId: number
    ): Observable<Array<DropDownListItemModel<number>>> {
        const params = {
            shipmentNumber: shipmentNumber,
            cruiseOrderId: cruiseOrderId,
        };
        return this.httpService.get(`${environment.apiUrl}/shipments`, params);
    }

    updateCruiseOrderItem(
        cruiseOrderItemId: number,
        model: ReviseCruiseOrderItemModel
    ): Observable<Array<CruiseOrderItemModel>> {
        return this.httpService.convertModelThenUpdate(ReviseCruiseOrderItemModel, `${environment.apiUrl}/cruiseOrderItems/${cruiseOrderItemId}`, model
        ).pipe(
            map(
                (returnedData: Array<any>) => {
                    let result: Array<CruiseOrderItemModel> = [];
                    if (returnedData && returnedData.length > 0) {
                        result = returnedData.map(x => new CruiseOrderItemModel(x));
                    }
                    return result;
                }
            )
        );
    }

    createCruiseOrderItem( model: CruiseOrderItemModel): Observable<CruiseOrderItemModel> {
        return this.httpService.create(`${environment.apiUrl}/cruiseOrderItems`, model);
    }

    deleteCruiseOrderItem(cruiseItemId: number): Observable<any> {
        return this.httpService.delete(`${environment.apiUrl}/cruiseOrderItems/${cruiseItemId}`);
    }
}
