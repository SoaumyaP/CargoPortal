import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core/services/http.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class NotificationService {

    constructor(private _httpService: HttpService,
        private _httpClient: HttpClient) {
    }

    get unreadTotal$() {
        return this._httpService.get(`${environment.apiUrl}/notifications/unreadTotal`);
    }

    getNotificationList$(skip: number, take: number) {
        return this._httpService.get(`${environment.apiUrl}/notifications/byUser?skip=${skip}&take=${take}`);
    }

    read$(notificationId: number) {
        return this._httpService.update(`${environment.apiUrl}/notifications/${notificationId}/read`);
    }

    readAll$() {
        return this._httpService.update(`${environment.apiUrl}/notifications/readAll`);
    }
}