import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService, FormService, DropDownListItemModel } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { ConsolidationService } from '../consolidation.service';

@Injectable()
export class ConsolidationConsignmentFormService extends FormService<any> {

    constructor(httpService: HttpService, private consolidationService: ConsolidationService) {
        super(httpService, `${environment.apiUrl}/consolidations/internal`);
    }

    createLinkingConsignment(
        consolidationId: number,
        consignmentId: number
    ) {
        return this.httpService.create(`${environment.apiUrl}/consolidations/internal/${consolidationId}/consignments/${consignmentId}`, {'id': consignmentId});
    }

    removeLinkingConsignment(
        consolidationId: number,
        consignmentId: number
    ) {
        return this.httpService.delete(`${environment.apiUrl}/consolidations/internal/${consolidationId}/consignments/${consignmentId}`);
    }

    searchShipmentNumberSelectionOptions(
        consolidationId: number,
        shipmentNumber: string
    ): Observable<Array<DropDownListItemModel<number>>> {
        const params = {
            shipmentNumber: shipmentNumber,
        };
        return this.httpService.get(`${environment.apiUrl}/consolidations/internal/${consolidationId}/shipments/dropdown`, params);
    }

    /**
     * To get data source for drop-down as adding new Consignment into Consolidation via GUI
     *
     * Only get Sea/Air mode of transport
     * @param shipmentId Id of provided shipment
     * @returns Observable of list of options
     */
    getAgentDropdown(
        shipmentId: number
    ): Observable<Array<ConsignmentDropdownItemModel>> {
        return this.httpService.get(`${environment.apiUrl}/consignments/${shipmentId}/dropdown`);
    }
}

export class ConsignmentDropdownItemModel extends DropDownListItemModel<number> {
    executionAgentId: number;
}
