import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FormService, HttpService } from 'src/app/core';
import { TelerikAccessTokenModel } from 'src/app/core/models/telerik/telerik-access-token.model';
import { environment } from 'src/environments/environment';
import { SchedulingModel } from '../models/scheduling.model';


@Injectable()
export class SchedulingFormService extends FormService<any> {
    constructor(private _httpService: HttpService) {
        super(_httpService, `${environment.apiUrl}/schedulings`);
    }

    /**
     * To get Telerik access token
     * @returns Observable<string>
     */
    public getTelerikAccessToken$(): Observable<TelerikAccessTokenModel> {
        const url = `${environment.apiUrl}/reports/telerikAccessToken`;
        return this._httpService.get(url);
    }

    /**
     * To create new scheduling
     */
    public createNewScheduling$(data: SchedulingModel): Observable<SchedulingModel> {
        const url = `${environment.apiUrl}/schedulings`;
        return this._httpService.create(url, data);
    }

    /**
    * To update scheduling
    */
    public updateScheduling$(data: SchedulingModel): Observable<SchedulingModel> {
        const url = `${environment.apiUrl}/schedulings`;
        return this._httpService.update(url, data);
    }

    /**
      * To update subscribers
      */
    public setSubscribers$(csPortalSchedulingId: number, telerikTaskId: string, emails: Array<string>) {
        const url = `${environment.apiUrl}/schedulings/${csPortalSchedulingId}/subscribers?telerikTaskId=${telerikTaskId}`;
        return this._httpService.update(url, emails);
    }

    /**
     * To update subscribers
     */
    public removeSubscriber$(csPortalSchedulingId: number, telerikTaskId: string, telerikUserId: string, email: string) {
        const url = `${environment.apiUrl}/schedulings/${csPortalSchedulingId}/subscribers?telerikTaskId=${telerikTaskId}&telerikUserId=${encodeURIComponent(telerikUserId)}&email=${encodeURIComponent(email)}`;
        return this._httpService.delete(url);
    }

    /**
     * To remove activity (document)
     * @param csPortalSchedulingId
     * @param telerikDocumentId
     * @param telerikTaskId
     * @returns
     */
    removeActivity$(csPortalSchedulingId: number, telerikDocumentId: string, telerikTaskId: string) {
        const url = `${environment.apiUrl}/schedulings/${csPortalSchedulingId}/activities?telerikTaskId=${telerikTaskId}&telerikDocumentId=${telerikDocumentId}`;
        return this._httpService.delete(url);
    }

    /**
     * To deactivate the current scheduling
     * @param csPortalSchedulingId
     * @param telerikTaskId
     * @returns
     */
    deactivateScheduling$(csPortalSchedulingId: number, telerikTaskId: string) {
        const url = `${environment.apiUrl}/schedulings/${csPortalSchedulingId}/deactivate?telerikTaskId=${telerikTaskId}`;
        return this._httpService.update(url);
    }

    /**
     * To activate the current scheduling
     * @param csPortalSchedulingId
     * @param telerikTaskId
     * @returns
     */
    activateScheduling$(csPortalSchedulingId: number, telerikTaskId: string) {
        const url = `${environment.apiUrl}/schedulings/${csPortalSchedulingId}/activate?telerikTaskId=${telerikTaskId}`;
        return this._httpService.update(url);
    }

    /**
     * To execute the current scheduling
     * @param csPortalSchedulingId
     * @param telerikTaskId
     * @returns
     */
    executeScheduling$(csPortalSchedulingId: number, telerikTaskId: string) {
        const url = `${environment.apiUrl}/schedulings/${csPortalSchedulingId}/executeTask?telerikTaskId=${telerikTaskId}`;
        return this._httpService.get(url);
    }

    /**
     * To delete the current scheduling
     * @param csPortalSchedulingId
     * @param telerikTaskId
     * @returns
     */
    deleteScheduling$(csPortalSchedulingId: number, telerikTaskId: string) {
        const url = `${environment.apiUrl}/schedulings/${csPortalSchedulingId}?telerikTaskId=${telerikTaskId}`;
        return this._httpService.delete(url);
    }
}
