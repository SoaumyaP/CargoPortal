import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core';
import { TelerikAccessTokenModel } from 'src/app/core/models/telerik/telerik-access-token.model';
import { environment } from 'src/environments/environment';

@Injectable({
    providedIn: 'root'
})
export class TelerikReportService {
    private _apiUrl: string;

    constructor(
        private _httpService: HttpService) {
        this._apiUrl = `${environment.apiUrl}/reports`;
    }

    /**
     * To get access token from server
     * @returns Observable<string>
     */
    public getReportToken(reportId: number, selectedCustomerId: number): Observable<TelerikAccessTokenModel> {
        const url = `${this._apiUrl}/${reportId}/token?selectedCustomerId=${selectedCustomerId}`;
        return this._httpService.get(url);
    }
}
