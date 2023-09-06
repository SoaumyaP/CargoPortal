import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { FormService, HttpService } from '../../../core';


@Injectable()
export class BillOfLadingFormService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/billOfLadings`);
    }

    getShipmentsByBOL(id: number): Observable<any> {
        if (this.checkApiPrefix(this.apiUrl)) {
            return this.httpService.get(
                `${environment.apiUrl}/billOfLadings/${id}/shipments?affiliates=${this.affiliateCodes}`);
        }
        return this.httpService.get(
            `${environment.apiUrl}/billOfLadings/${id}/shipments`);
    }

    getContainersByBOL(id: number): Observable<any> {
        if (this.checkApiPrefix(this.apiUrl)) {
            return this.httpService.get(
                `${environment.apiUrl}/billOfLadings/${id}/containers?affiliates=${this.affiliateCodes}`);
        }
        return this.httpService.get(
            `${environment.apiUrl}/billOfLadings/${id}/containers`);
    }

    getContactsByBOL(id: number): Observable<any> {
        return this.httpService.get(
            `${environment.apiUrl}/billOfLadings/${id}/contacts`);
    }


    getItinerariesByBOL(id: number): Observable<any> {
        return this.httpService.get(
            `${environment.apiUrl}/billOfLadings/${id}/itineraries`);
    }

    getAttachmentsByBOL(id: number): Observable<any> {
        return this.httpService.get(
            `${environment.apiUrl}/billOfLadings/${id}/attachments`);
    }

    filterHouseBL(
        houseBLNo: string,
        modeOfTransport?: string,
        executionAgent?: Number
    ) {
        return this.httpService.get(
            `${environment.apiUrl}/billOfLadings/houseBLs?houseBLNo=${encodeURIComponent(houseBLNo)}&modeOfTransport=${encodeURIComponent(modeOfTransport)}&executionAgent=${executionAgent}&affiliates=${this.affiliateCodes}`);
    }

    /**
     * To assign master bill of lading to house bill of lading
     * @param houseBOLId
     * @param masterBOLId
     * @returns
     */
    assignMasterBillOfLading(houseBOLId: number, masterBOLId: number): Observable<any> {
        return this.httpService.update(
            `${environment.apiUrl}/billOfLadings/${houseBOLId}/assignMasterBOL?masterBOLId=${masterBOLId}`);
    }

    searchShipments(
        houseBLId: number,
        shipmentNo: string,
        modeOfTransport: string,
        executionAgentId: number) {
        return this.httpService.get(`${environment.apiUrl}/billOfLadings/${houseBLId}/shipment-selection?shipmentNo=${encodeURIComponent(shipmentNo)}&modeOfTransport=${encodeURIComponent(modeOfTransport)}&executionAgentId=${executionAgentId}&affiliates=${this.affiliateCodes}`);
    }

    updateHouseBL(
        houseBLId: number,
        houseBLModel: any
    ) {
        return this.httpService.update(`${environment.apiUrl}/billOfLadings/internal/${houseBLId}`, houseBLModel);
    }

    linkHouseBLToShipment(
        shipmentId: number,
        houseBLId: number,
        executionAgentId: number
    ) {
        return this.httpService.update(`${environment.apiUrl}/billOfLadings/${houseBLId}/link-to-shipment?shipmentId=${shipmentId}&executionAgentId=${executionAgentId}`)
    }

    unlinkShipment(
        shipmentId: number,
        houseBLId: number,
        isTheLastLinkedShipment: number
    ) {
        return this.httpService.update(`${environment.apiUrl}/billOfLadings/${houseBLId}/unlink-shipment?shipmentId=${shipmentId}&isTheLastLinkedShipment=${isTheLastLinkedShipment}`)
    }

    checkHouseBLAlreadyExists(houseBLNo: string) {
        return this.httpService.get(`${environment.apiUrl}/billOfLadings/already-exists?houseBLNo=${encodeURIComponent(houseBLNo)}`);
    }
}
