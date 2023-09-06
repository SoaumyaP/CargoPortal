import { Injectable } from '@angular/core';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';

@Injectable()
export class ConsignmentFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/consignments`);
    }

    getAttachments(id) {
        return this.httpService.get(`${environment.apiUrl}/consignments/${id}/attachments`);
    }

    getCargoDetail(shipmentId) {
        return this.httpService.get(`${environment.apiUrl}/consignments/${shipmentId}/cargodetails`);
    }

    getContact(shipmentId) {
        return this.httpService.get(`${environment.apiUrl}/consignments/${shipmentId}/contacts`);
    }

    getContainers(shipmentId) {
        return this.httpService.get(`${environment.apiUrl}/consignments/${shipmentId}/containers?affiliates=${this.affiliateCodes}`);
    }

    getConsolidations(consolidationId){
        return this.httpService.get(`${environment.apiUrl}/consignments/${consolidationId}/consolidations`);
    }

    getItineraries(id): Observable<any[]>{
        return this.httpService.get(`${environment.apiUrl}/consignments/${id}/itineraries`);
    }

    getActivities(id): Observable<any[]>{
        return this.httpService.get(`${environment.apiUrl}/consignments/${id}/activities`);
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

    createActivity(consignmentId, model) {
        return this.httpService.create(`${environment.apiUrl}/consignments/${consignmentId}/activities`,
        model);
    }

    updateActivity(consignmentId, activityId, model) {
        return this.httpService.update(`${environment.apiUrl}/consignments/${consignmentId}/activities/${activityId}`,
        model);
    }

    deleteActivity(consignmentId, activityId) {
        return this.httpService.delete(`${environment.apiUrl}/consignments/${consignmentId}/activities/${activityId}`);
    }

    getAllCarriers(): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/carriers`);
    }

    getAllLocations(): any {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }

    getEvents(types): any {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/eventCodes/eventByTypes?types=${types}`);
    }

    getOrganizations(): Observable<any[]> {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/organizations/activeCodes`);
    }

    getOrganization(id): Observable<any> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}`);
    }

    moveToTrash(consignmentId) {
        return this.httpService.update(`${environment.apiUrl}/consignments/${consignmentId}/trash`);
    }

    getSchedules(queryParams: string): Observable<any[]> {
        return this.httpService.get(`${environment.apiUrl}/freightSchedulers/filter?${queryParams}`);
    }

    checkFullLoadShipment(
        id: number
    ) : Observable<boolean> {
        return this.httpService.get(`${environment.apiUrl}/shipments/${id}/checkFullLoadShipment`);
    }
}
