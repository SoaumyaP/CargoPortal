import { Injectable } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { take, filter, concatMap, map } from 'rxjs/operators';
import { StringHelper } from '../helpers';
import { environment } from 'src/environments/environment';
import { OrganizationStatus, Roles } from '../models/enums/enums';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { HttpService } from '../services/http.service';
import { UserTrackTraceService } from '../services/user-track-trace.service';
import { UserProfileModel } from '../models/user/user-profile.model';
import { GAEventCategory } from '../models/constants/app-constants';
import { GoogleAnalyticsService } from '../services/google-analytics.service';

@Injectable({
    providedIn: 'root'
})
export class UserContextService {
    private _currentUserSubject: BehaviorSubject<any> = new BehaviorSubject(null);
    private _currentOrgLogoSubject: BehaviorSubject<any> = new BehaviorSubject(null);
    private LOGIN_TYPE_KEY = 'login_type';

    public loginTypeEnum = {
        internal: 'in',
        external: 'ex'
    };

    get loginType(): string {
        return localStorage.getItem(this.LOGIN_TYPE_KEY) || '';
    }
    set loginType(type) {
        const loginType = type === this.loginTypeEnum.internal
            ? this.loginTypeEnum.internal
            : this.loginTypeEnum.external;
        localStorage.setItem(this.LOGIN_TYPE_KEY, loginType);
    }

    set organizationLogo(logo: any) {
        this._currentOrgLogoSubject.next(logo);
    }

    public getCurrentOrgLogo(): Observable<any> {
        return this._currentOrgLogoSubject.asObservable();
    }

    get currentUser(): any {
        return this._currentUserSubject.getValue();
    }

    /**
     * To get current login type, it works on normal and user role switch mode
     */
    get currentUserLoginType(): string {
        const currentUser = this.currentUser;
        return currentUser && (currentUser.isInternal || currentUser.isUserRoleSwitch)
            ? this.loginTypeEnum.internal
            : this.loginTypeEnum.external;
    }

    constructor(
        private _oidcSecurityService: OidcSecurityService,
        private _httpService: HttpService,
        public notification: NotificationPopup,
        private translateService: TranslateService,
        private router: Router,
        private _userTrackTraceService: UserTrackTraceService,
        private _gaService: GoogleAnalyticsService) { }

    public getCurrentUser(): Observable<UserProfileModel> {
        return this._currentUserSubject.asObservable().pipe(
            map((user: any) => (user ? { ...user } : null))
        );
    }

    public initialize(): void {
        if (this.currentUser) {
            return;
        }

        this.queryCurrentUser();
    }

    public queryCurrentUser() {
        this._oidcSecurityService.getIsAuthorized().pipe(
            filter((isAuthorized: boolean) => isAuthorized === true),
            take(1),
            concatMap(() => {
                return this._httpService.get<any>(`${environment.apiUrl}/users/current`);
            }),
            concatMap(currentUser => {
                if (StringHelper.isNullOrEmpty(currentUser)) {
                    // window.location.reload();
                    localStorage.setItem(
                        'business_call_back_url', `${window.location.pathname}${window.location.search}`
                    );
                    this._oidcSecurityService.logoff(() => {
                        this.router.navigate(['/login']);
                    });
                }
                if (!currentUser || !currentUser.organizationId) {
                    return of(currentUser);
                }

                return this._httpService.get<any>(`${environment.commonApiUrl}/organizations/${currentUser.organizationId}/AffiliateCodes`)
                    .pipe(map(result => {
                        const data = result || [];
                        currentUser.affiliates = JSON.stringify(data);
                        return currentUser;
                    }));
            }),
            concatMap(currentUser => {
                if (!currentUser || !currentUser.organizationId) {
                    return of(currentUser);
                }

                return this._httpService.get<any>(`${environment.commonApiUrl}/organizations/${currentUser.organizationId}/OrganizationLogo`)
                    .pipe(map(result => {
                        const data = result || [];
                        currentUser.companyLogo = data.organizationLogo;
                        return currentUser;
                }));
            }),
            concatMap((currentUser: any) => {
                if (!currentUser || !currentUser.organizationId || currentUser.isInternal || currentUser.role.id !== Roles.Shipper ||
                    !StringHelper.isNullOrWhiteSpace(localStorage.getItem('isCheckConfirmConnection'))) { // check in first time login
                    return of(currentUser);
                }

                localStorage.setItem('isCheckConfirmConnection', 'checked');
                return this._httpService.update(`${environment.commonApiUrl}/organizations/${currentUser.organizationId}/updateStatus`)
                    .pipe(map((customerList: any) => {
                        if (customerList !== null && customerList.length > 0) {
                            this.confirmConnectPrincipal(currentUser.organizationId, customerList);
                        }

                        return currentUser;
                    }));
            }),
            concatMap(async (currentUser: any) => {
                if (currentUser && currentUser.role && (currentUser.role.id === Roles.Shipper || currentUser.role.id === Roles.Factory)) {
                    currentUser.customerRelationships = await this.getCustomerRelationships(currentUser).toPromise();
                    return currentUser;
                } else {
                    return await currentUser;
                }
            })
        ).subscribe(result => {
            return this._currentUserSubject.next(result);
        });
    }

    public getCustomerRelationships(currentUser){
        return this._httpService.get<any>(`${environment.commonApiUrl}/organizations/CustomerRelationShips?affiliates=${currentUser.affiliates}&connectionType=1`)
        .map(result => {
            const data = result || [];
            let paramValue: string = ';';
            if (data.length > 0) {
                data.forEach(x => {
                    paramValue += `${x.supplierId},${x.customerId};`;
                });
            }
            return paramValue;
        });
    }

    private async confirmConnectPrincipal(supplierId, customerList) {
        for (let i = 0; i < customerList.length; i++) {
            const msg = this.translateService
            .instant('msg.acceptConnectPrincipal',
            {
                principalOrgName: customerList[i].name
            });
            const confirmDlg = this.notification.showConfirmationDialog(msg, 'label.organizations');
            const isConfirm: any = await confirmDlg.result.toPromise();
            if (!StringHelper.isNullOrEmpty(isConfirm.value)) {
                await this._httpService.update
                (`${environment.commonApiUrl}/organizations/${supplierId}/confirmConnectionType/${customerList[i].id}?isConfirm=${isConfirm.value}`)
                .toPromise();
            }
        }
    }

    public login(type: string) {
        if (!StringHelper.isNullOrEmpty(type) && type.toLowerCase() === 'in') {
            type = 'in';
        } else {
            type = 'ex';
        }
        this.loginType = type;
        if (localStorage.getItem('lang') != null) {
            this._oidcSecurityService.setCustomRequestParameters({ 'culture': localStorage.getItem('lang') });
        }
        this._oidcSecurityService.setCustomRequestParameters({ 'type': type });

        // this delay is required. I'm not entirely sure why
        setTimeout(() => {
            this._oidcSecurityService.authorize();
        }, 500);
    }

    public logout() {
        // Track as user intends to logout then sync to server.
        this._userTrackTraceService.stop();
        // this._userTrackTraceService.track('Logout');
        this._userTrackTraceService.syncToServer();

        // Remove State filter for all screen (just apply for list screens)
        for (const key in localStorage) {
            if (key.startsWith('GridState_')) {
                localStorage.removeItem(key);
            }
        }
        localStorage.removeItem('isCheckConfirmConnection');
        this._httpService.get(`${environment.apiUrl}/users/ResetPermissionCache`).subscribe(
            response => {
                this._oidcSecurityService.setCustomRequestParameters({ 'type': this.currentUserLoginType });
                this._oidcSecurityService.logoff(url => {
                    const type = this.currentUserLoginType;
                    const connector = url.includes('?') ? '&' : '?';
                    window.location.href = `${url}${connector}type=${type}`;
                    this._gaService.emitEvent('log_out', GAEventCategory.UserProfile, 'Log Out');
                });
            }
        );
    }

    public isGranted(permissionName: string): Observable<boolean> {
        return this.getCurrentUser().pipe(
            filter(test => test != null && test.permissions != null),
            take(1),
            concatMap(u => {
                const allowed = u.permissions.filter(s => s.name === permissionName);
                if (allowed.length > 0) {
                    return of(true);
                }
                return of(false);
            }));
    }
}
