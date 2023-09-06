import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core';
import { FormService } from 'src/app/core/form/form.service';
import { ItineraryModel } from 'src/app/core/models/itinerary.model';
import { environment } from 'src/environments/environment';
import { MasterBillOfLadingModel } from '../models/master-bill-of-lading-model';

@Injectable()
export class MasterBillOfLadingFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/masterBillOfLadings`);
    }

    getHouseBillOfLadings(masterBOLId) {
        if (this.checkApiPrefix(this.apiUrl)) {
            return this.httpService.get(`${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/billOfLadings?affiliates=${this.affiliateCodes}`);
        }
        return this.httpService.get(`${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/billOfLadings`);
    }

    /**
     * To get list of containers of master bill of lading
     * @param masterBOLId Id of master bill of lading
     * @param isDirectMaster Whether master bill of lading is direct
     * @returns
     */
    getContainers(masterBOLId: number, isDirectMaster: boolean) {
        return this.httpService.get(`${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/containers?isDirectMaster=${isDirectMaster}`);
    }

    /**
     * To get list of shipments of master bill of lading
     * @param masterBOLId Id of master bill of lading
     * @param isDirectMaster Whether master bill of lading is direct
     * @returns
     */
    getShipments(masterBOLId: number, isDirectMaster: boolean) {
        if (this.checkApiPrefix(this.apiUrl)) {
            return this.httpService.get(`${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/shipments?isDirectMaster=${isDirectMaster}&affiliates=${this.affiliateCodes}`);
        }
        return this.httpService.get(`${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/shipments?isDirectMaster=${isDirectMaster}`);
    }

    getContacts(masterBOLId) {
        return this.httpService.get(`${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/contacts`);
    }

    getAttachments(masterBOLId) {
        return this.httpService.get(`${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/attachments`);
    }

    getItineraries(masterBOLId: number): Observable<Array<ItineraryModel>> {
        return this.httpService.get(`${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/itineraries`);
    }

    updateMasterBOL(masterBLId: number, data: MasterBillOfLadingModel): Observable<any> {
        return this.httpService.update(`${environment.apiUrl}/masterBillOfLadings/internal/${masterBLId}`, data);
    }

    /**
     * To create master bill of lading from house bill of lading
     * @param houseBOLId
     * @param model
     * @returns
     */
    createMasterBLFromBL(houseBOLId: number, model: MasterBillOfLadingModel): Observable<MasterBillOfLadingModel> {
        return this.httpService.create(`${environment.apiUrl}/BillOfLadings/${houseBOLId}/masterBillOfLadings`, model);
    }

    /**
     * To create master bill of lading from shipment
     * @param shipmentId
     * @param model
     * @returns
     */
    createMasterBLFromShipment(shipmentId: number, model: MasterBillOfLadingModel): Observable<MasterBillOfLadingModel> {
        return this.httpService.create(`${environment.apiUrl}/Shipments/${shipmentId}/masterBillOfLadings`, model);
    }

    /**
     * To assign house bill of lading to master bill of lading
     * @param masterBOLId
     * @param houseBOLId
     * @returns
     */
    assignHouseBillOfLading(masterBOLId: number, houseBOLId: number): Observable<any> {
        return this.httpService.update(
            `${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/assignHouseBOL?houseBOLId=${houseBOLId}`);
    }

    /**
     * To remove house bill of lading from master bill of lading
     * @param shipmentId
     * @returns
     */
    removeHouseBillOfLading(masterBOLId: number, houseBOLId: number): Observable<any> {
        return this.httpService.update(
            `${environment.apiUrl}/masterBillOfLadings/${masterBOLId}/removeHouseBOL?houseBOLId=${houseBOLId}`);
    }

    /**
     * To get available locations in the system
     * @returns
     */
    getAllLocations(): any {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }
}
