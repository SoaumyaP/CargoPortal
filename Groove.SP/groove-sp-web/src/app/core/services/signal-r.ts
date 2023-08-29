import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { PushNotification } from '../models/push-notification';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Subject } from 'rxjs';

@Injectable()
export class SignalRService {
    private hubConnection: signalR.HubConnection;

    public onPushNotificationArrived = (data: PushNotification) => { };
    public pushNotification$: Subject<PushNotification> = new Subject();

    constructor(
        private oidcSecurityService: OidcSecurityService) { }

    public startConnection() {
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${environment.hubUrl}/notification`, {
                accessTokenFactory: () => {
                    const token = this.oidcSecurityService.getToken();
                    return token;
                },
            })
            .build();

        this.hubConnection
            .start()
            .then(() => console.log('Connection started'))
            .catch(err => console.log('Error while starting connection: ' + err));
    }

    public addPushNotificationListener() {
        this.hubConnection.on('PushNotification', (response: PushNotification) => {
            this.onPushNotificationArrived(response);
            this.pushNotification$.next(response);
        });
    }
}