import { Injectable } from '@angular/core';
import { Observable, Subscription, timer } from 'rxjs';
import { environment } from 'src/environments/environment';
import { UserAuditLogModel } from '../models/user-audit-log.model';
import { HttpService } from './http.service';

@Injectable({
    providedIn: 'root'
})

export class UserTrackTraceService {

    private _featureTrackList: Array<UserAuditLogModel> = [];
    private _subscriptions: Array<Subscription> = [];

    constructor(
        private _httpService: HttpService
    ) {
    }
    public screenSize(): string {
        let screenSize = '';
        if (screen.width) {
            const width = (screen.width) ? screen.width : '';
            const height = (screen.height) ? screen.height : '';
            screenSize += '' + width + ' x ' + height;
        }
        return screenSize;
    }

    public browserVersion(): string {
        const userAgent = navigator.userAgent;
        let browser = navigator.appName;
        let version = '' + parseFloat(navigator.appVersion);
        let majorVersion = parseInt(navigator.appVersion, 10);
        let nameOffset, verOffset, ix;

        // detect browsers

        if ((verOffset = userAgent.indexOf('Opera')) !== -1) {
            browser = 'Opera';
            version = userAgent.substring(verOffset + 6);
            if ((verOffset = userAgent.indexOf('Version')) !== -1) {
                version = userAgent.substring(verOffset + 8);
            }
        }

        if ((verOffset = userAgent.indexOf('OPR')) !== -1) {
            browser = 'Opera';
            version = userAgent.substring(verOffset + 4);
        } else if ((verOffset = userAgent.indexOf('Edge')) !== -1) {
            browser = 'Microsoft Legacy Edge';
            version = userAgent.substring(verOffset + 5);
        } else if ((verOffset = userAgent.indexOf('Edg')) !== -1) {
            browser = 'Microsoft Edge';
            version = userAgent.substring(verOffset + 4);
        } else if ((verOffset = userAgent.indexOf('MSIE')) !== -1) {
            browser = 'Microsoft Internet Explorer';
            version = userAgent.substring(verOffset + 5);
        } else if ((verOffset = userAgent.indexOf('Chrome')) !== -1) {
            browser = 'Chrome';
            version = userAgent.substring(verOffset + 7);
        } else if ((verOffset = userAgent.indexOf('Safari')) !== -1) {
            browser = 'Safari';
            version = userAgent.substring(verOffset + 7);
            if ((verOffset = userAgent.indexOf('Version')) !== -1) {
                version = userAgent.substring(verOffset + 8);
            }
        } else if ((verOffset = userAgent.indexOf('Firefox')) !== -1) {
            browser = 'Firefox';
            version = userAgent.substring(verOffset + 8);
        } else if (userAgent.indexOf('Trident/') !== -1) {
            browser = 'Microsoft Internet Explorer';
            version = userAgent.substring(userAgent.indexOf('rv:') + 3);
        } else if ((nameOffset = userAgent.lastIndexOf(' ') + 1) < (verOffset = userAgent.lastIndexOf('/'))) {
            browser = userAgent.substring(nameOffset, verOffset);
            version = userAgent.substring(verOffset + 1);
            if (browser.toLowerCase() === browser.toUpperCase()) {
                browser = navigator.appName;
            }
        }
        // trim the version string
        if ((ix = version.indexOf(';')) !== -1) { version = version.substring(0, ix); }
        if ((ix = version.indexOf(' ')) !== -1) { version = version.substring(0, ix); }
        if ((ix = version.indexOf(')')) !== -1) { version = version.substring(0, ix); }

        majorVersion = parseInt('' + version, 10);
        if (isNaN(majorVersion)) {
            version = '' + parseFloat(navigator.appVersion);
            majorVersion = parseInt(navigator.appVersion, 10);
        }

        return  browser + ' ' + majorVersion + ' (' + version + ')';
    }

    public operatingSystem(): string {
        const userAgent = navigator.userAgent;
        const appVersion = navigator.appVersion;

        let os = 'unknown';
        const clientStrings = [
            {s: 'Windows 10', r: /(Windows 10.0|Windows NT 10.0)/},
            {s: 'Windows 8.1', r: /(Windows 8.1|Windows NT 6.3)/},
            {s: 'Windows 8', r: /(Windows 8|Windows NT 6.2)/},
            {s: 'Windows 7', r: /(Windows 7|Windows NT 6.1)/},
            {s: 'Windows Vista', r: /Windows NT 6.0/},
            {s: 'Windows Server 2003', r: /Windows NT 5.2/},
            {s: 'Windows XP', r: /(Windows NT 5.1|Windows XP)/},
            {s: 'Windows 2000', r: /(Windows NT 5.0|Windows 2000)/},
            {s: 'Windows ME', r: /(Win 9x 4.90|Windows ME)/},
            {s: 'Windows 98', r: /(Windows 98|Win98)/},
            {s: 'Windows 95', r: /(Windows 95|Win95|Windows_95)/},
            {s: 'Windows NT 4.0', r: /(Windows NT 4.0|WinNT4.0|WinNT|Windows NT)/},
            {s: 'Windows CE', r: /Windows CE/},
            {s: 'Windows 3.11', r: /Win16/},
            {s: 'Android', r: /Android/},
            {s: 'Open BSD', r: /OpenBSD/},
            {s: 'Sun OS', r: /SunOS/},
            {s: 'Chrome OS', r: /CrOS/},
            {s: 'Linux', r: /(Linux|X11(?!.*CrOS))/},
            {s: 'iOS', r: /(iPhone|iPad|iPod)/},
            {s: 'Mac OS X', r: /Mac OS X/},
            {s: 'Mac OS', r: /(Mac OS|MacPPC|MacIntel|Mac_PowerPC|Macintosh)/},
            {s: 'QNX', r: /QNX/},
            {s: 'UNIX', r: /UNIX/},
            {s: 'BeOS', r: /BeOS/},
            {s: 'OS/2', r: /OS\/2/},
            {s: 'Search Bot', r: /(nuhk|Googlebot|Yammybot|Openbot|Slurp|MSNBot|Ask Jeeves\/Teoma|ia_archiver)/}
        ];
        for (let id in clientStrings) {
            let cs = clientStrings[id];
            if (cs.r.test(userAgent)) {
                os = cs.s;
                break;
            }
        }

        let osVersion = 'unknown';

        if (/Windows/.test(os)) {
            osVersion = /Windows (.*)/.exec(os)[1];
            os = 'Windows';
        }

        switch (os) {
            case 'Mac OS':
            case 'Mac OS X':
            case 'Android':
                osVersion = /(?:Android|Mac OS|Mac OS X|MacPPC|MacIntel|Mac_PowerPC|Macintosh) ([\.\_\d]+)/.exec(userAgent)[1];
                break;

            case 'iOS':
                const osVersionReg = /OS (\d+)_(\d+)_?(\d+)?/.exec(appVersion);
                const v1: number =  parseInt(osVersionReg[1], 10);
                const v2: number =  parseInt(osVersionReg[2], 10);
                const v3: number =  parseInt(osVersionReg[3], 10);
                osVersion = v1 + '.' + v2 + '.' + v3;
                break;
        }
        return os + ' ' + osVersion;
    }

    public track(feature: string ): void {
        const newTrack = new UserAuditLogModel(
            this.operatingSystem(),
            this.browserVersion(),
            this.screenSize(),
            navigator.userAgent,
            feature,
            new Date()
        );
        this._featureTrackList.push(newTrack);
    }

    public syncToServer(): void {

        // No need to sync to server if there is no historical log
        if (this._featureTrackList.length === 0) {
            return;
        }
        // clone current tracking list
        const uploadData = Object.assign([], this._featureTrackList);
        this._httpService.create(`${environment.apiUrl}/users/current/syncUserTracking`, uploadData, false).subscribe(
            () => {
                // If success, then clear current list;
                this._featureTrackList = [];
            }
        );
    }

    public start(): void {
        // track and trace user login
        const interval = environment.userTrackTraceInterval;
        const startAfter = 5000;
        let numbers: Observable<number>;
        if (interval > 0) {
            // after 5 seconds, fire request every <interval> time to sync data to server
            numbers = timer(startAfter, interval);
        } else {
            // after 5 seconds, fire request only once
            numbers = timer(startAfter);
        }
        const sub = numbers.subscribe(x => this.syncToServer());
        this._subscriptions.push(sub);
    }

    public stop(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
