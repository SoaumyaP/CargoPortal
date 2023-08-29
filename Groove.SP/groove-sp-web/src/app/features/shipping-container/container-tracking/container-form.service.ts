import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { EventLevelMapping, FormService, HttpService, StringHelper } from 'src/app/core';

@Injectable()
export class ContainerFormService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/containers`);
    }

    getActivitiesByContainer(id: string): Observable<any> {
        return this.httpService.get(
            `${environment.apiUrl}/containers/${id}/activities`);
    }

    getItinerariesByContainer(id: string): Observable<any> {
        return this.httpService.get(
            `${environment.apiUrl}/containers/${id}/itineraries`);
    }

    getCargodetailsByContainer(id: string): Observable<any> {
        if (this.checkApiPrefix(this.apiUrl)) {
            return this.httpService.get(
                `${environment.apiUrl}/containers/${id}/cargodetails?affiliates=${this.affiliateCodes}`);
        }
        return this.httpService.get(
            `${environment.apiUrl}/containers/${id}/cargodetails`);
    }

    getAttachmentsByContainers(id: string): Observable<any> {
        return this.httpService.get(
            `${environment.apiUrl}/containers/${id}/attachments`);
    }

    updateContainer(containerId, model) {
        return this.httpService.update(`${environment.apiUrl}/containers/internal/${containerId}`,
            model);
    }

    testReport(id: string): Observable<any> {
        return this.httpService.downloadFile(`${environment.apiUrl}/containers/${id}/testReport`, 'aaa.pdf');
    }

    getEvents(): any {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/eventCodes/eventByLevel?level=${EventLevelMapping.Container}`);
    }

    getAllLocations(): any {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }

    createActivity(containerId, model) {
        return this.httpService.create(`${environment.apiUrl}/containers/internal/${containerId}/activities`,
        model);
    }
    
    updateActivity(containerId, activityId, model) {
        return this.httpService.update(`${environment.apiUrl}/containers/internal/${containerId}/activities/${activityId}`,
        model);
    }

    deleteActivity(containerId, activityId) {
        return this.httpService.delete(`${environment.apiUrl}/containers/internal/${containerId}/activities/${activityId}`);
    }

    isDuplicatedContainer(
        containerId: number,
        containerNo: string,
        carrierSONo: string
    ): Observable<boolean> {
        return this.httpService.get<boolean>(`${environment.apiUrl}/containers/internal/${containerId}/checkDuplicateContainer?containerNo=${containerNo}&carrierSONo=${carrierSONo}`);
    }
}
