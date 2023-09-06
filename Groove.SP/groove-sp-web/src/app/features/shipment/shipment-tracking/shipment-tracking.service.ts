import { Injectable } from '@angular/core';
import { HttpService, FormService, EventLevelMapping } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { mergeMap, map, reduce, take } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { NoteModel } from 'src/app/core/models/note.model';
import { ContractMasterModel } from '../master-bill-of-lading/models/contract-master-model';
import { ShipmentModel } from 'src/app/core/models/shipments/shipment.model';
import { ShipmentLoadDetailModel } from 'src/app/core/models/shipments/shipment-load-detail.model';

@Injectable()
export class ShipmentTrackingService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/shipments`);
    }

    getActivity(shipmentId) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentId}/activities`);
    }

    getAttachment(shipmentNo) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentNo}/attachments`);
    }

    getCountries(): Observable<any> {
        return this.httpService.get(`${environment.commonApiUrl}/countries/dropDownCode`);
    }

    getBuyerCompliance(customerId: any): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/buyercompliances`, { organizationId: customerId }).pipe(
            take(1)
        );
    }

    getCargoDetail(shipmentNo) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentNo}/cargodetails`);
    }

    getContact(shipmentNo) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentNo}/contacts`);
    }

    getConsignment(shipmentNo) {
        return this.httpService.get<any[]>(`${environment.apiUrl}/shipments/${shipmentNo}/consignments`)
        .pipe(
            mergeMap(consignments => {
                let agentIds = consignments.map(c => c.executionAgentId);
                if (agentIds === null || agentIds.length === 0) {
                    return [];
                }
                agentIds = agentIds.filter(this._onlyUnique);
                return this.httpService.get<any[]>(`${environment.commonApiUrl}/organizations/orgReferenceData`, {idList: agentIds})
                .pipe(
                    map(organizations => {
                        return consignments.map(c => {
                            const org = organizations.find(o => o.id === c.executionAgentId);
                            return {
                                ...c,
                                executionAgent: org && org.name
                            };
                        });
                    })
                );
            })
        );
    }

    getItinerary(shipmentNo) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentNo}/itineraries`);
    }

    getConsolidation(shipmentNo) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentNo}/consolidations`);
    }

    getContainer(shipmentNo) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentNo}/containers?affiliates=${this.affiliateCodes}`);
    }

    getPOLineItem(purchaseOrderId, poLineItemId) {
        return this.httpService.get(`${environment.apiUrl}/purchaseorders/${purchaseOrderId}/polineitems/${poLineItemId}`);
    }

    createConsignment(model) {
        return this.httpService.create(`${environment.apiUrl}/consignments`,
        model);
    }

    confirmItinerary(shipmentId, model) {
        return this.httpService.update(`${environment.apiUrl}/shipments/${shipmentId}/confirmItinerary`, model);
    }

    updateConfirmItinerary(shipmentId, model) {
        return this.httpService.update(`${environment.apiUrl}/shipments/${shipmentId}/confirmItineraryUpdates`, model);
    }

    createItinerary(consignmentId: number, model: any, affiliates: string) {
        return this.httpService.create(`${environment.apiUrl}/consignments/${consignmentId}/itineraries?affiliates=${affiliates}`,
        model);
    }

    updateItinerary(consignmentId: number, itineraryId: number, model: any, affiliates: string) {
        return this.httpService.update(`${environment.apiUrl}/consignments/${consignmentId}/itineraries/${itineraryId}?affiliates=${affiliates}`,
        model);
    }

    deleteItinerary(consignmentId: number, itineraryId: number, affiliates: string) {
        return this.httpService.delete(`${environment.apiUrl}/consignments/${consignmentId}/itineraries/${itineraryId}?affiliates=${affiliates}`);
    }

    getDefaultCFSClosingDate(shipmentId: number) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentId}/defaultCFSClosingDate`);
    }

    createActivity(shipmentId, model) {
        return this.httpService.create(`${environment.apiUrl}/shipments/${shipmentId}/activities`,
        model);
    }

    updateActivity(shipmentId, activityId, model) {
        return this.httpService.update(`${environment.apiUrl}/shipments/${shipmentId}/activities/${activityId}`,
        model);
    }

    deleteActivity(shipmentId, activityId) {
        return this.httpService.delete(`${environment.apiUrl}/shipments/${shipmentId}/activities/${activityId}`);
    }

    getAllLocations(): any {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }

    getEvents(): any {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/eventCodes/eventByLevel?level=${EventLevelMapping.Shipment}`);
    }

    trialValidateOnCancelShipment(shipmentId) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentId}/trialValidateOnCancelShipment`);
    }

    cancelShipment(shipmentId) {
        return this.httpService.update(`${environment.apiUrl}/shipments/${shipmentId}/cancel/`);
    }

    getNotes(shipmentId: number): Observable<NoteModel[]> {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentId}/notes`);
    }

    getMasterNotes(shipmentId: number) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentId}/masterDialogs`);
    }

    trialValidateOnAssignHouseBL(
        shipmentId: number
    ) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentId}/trialValidateOnAssignHouseBL`);
    }

    assignHouseBL(shipmentId: number, houseBLModel: any) {
        return this.httpService.create(`${environment.apiUrl}/shipments/${shipmentId}/houseBLs/assign`, houseBLModel);
    }

    createAndAssignHouseBL(shipmentId: number, houseBLModel: any) {
        return this.httpService.create(`${environment.apiUrl}/shipments/${shipmentId}/houseBLs`, houseBLModel);
    }

    checkFullLoadShipment(
        id: number
    ): Observable<boolean> {
        return this.httpService.get(`${environment.apiUrl}/shipments/${id}/checkFullLoadShipment`);
    }

    assignMasterBillOfLading(shipmentId: number, masterBOLId: number): Observable<any> {
        return this.httpService.update(
            `${environment.apiUrl}/shipments/${shipmentId}/assignMasterBOL?masterBOLId=${masterBOLId}`);
    }

    unlinkMasterBillOfLading(shipmentId: number): Observable<any> {
        return this.httpService.update(
            `${environment.apiUrl}/shipments/${shipmentId}/unlinkMasterBOL`);
    }

    /**
     * To search contract master on server-side
     * @param searchTerm Text to search
     * @param modeOfTransport Mode of transport of current shipment
     * @param currentDate Current date without time
     * @returns Observable<Array<ContractMasterModel>>
     */
    getContractMasters(searchTerm: string, shipmentId: number, currentDate: Date): Observable<Array<ContractMasterModel>> {
        const currentDateOnlyString = currentDate.toISOString().substring(0, 10);
        const result = this.httpService.get<Array<ContractMasterModel>>(`${environment.apiUrl}/contractMasters/shipmentContractMasterOptions` +
                        `?searchTerm=${encodeURIComponent(searchTerm)}&shipmentId=${encodeURIComponent(shipmentId)}&currentDate=${currentDateOnlyString}`);
        return result;
    }

    updateShipment(shipmentId: number, model: ShipmentModel): Observable<any> {
        return this.httpService.update(`${environment.apiUrl}/shipments/internal/${shipmentId}`, model);
    }

    getShipmentLoadDetails(shipmentId: number): Observable<Array<ShipmentLoadDetailModel>> {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentId}/shipmentLoadDetails`);
    }
}