import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class AppState {
    private _requestSPUrlBase: string;
    private _requestCSFEUrlBase: string;
    private _loginUrlBase: string;
    private _markettingUrlBase: string;

    constructor() {
        this._requestSPUrlBase = environment.apiUrl;
        this._requestCSFEUrlBase = environment.commonApiUrl;
        this._loginUrlBase = environment.identityUrl;
        this._markettingUrlBase = environment.marketingUrl;
    }

    public get requestSPBaseUrl(): string {
        return this._requestSPUrlBase + '/';
    }

    public get requestCSFEBaseUrl(): string {
        return this._requestCSFEUrlBase + '/';
    }

    public get loginUrl() {
        return this._loginUrlBase + '/';
    }

    public get marketingUrl() {
        return this._markettingUrlBase;
    }
}