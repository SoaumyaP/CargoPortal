import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, CanActivateChild } from '@angular/router';
import { Observable, of } from 'rxjs';
import { concatMap, filter, map, tap, delay, take } from 'rxjs/operators';
import { AppPermissions } from './auth-constants';
import { UserContextService } from './user-context.service';
import { UserStatus, RoleStatus } from '../models/enums/enums';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { StringHelper } from '..';

@Injectable({
    providedIn: 'root'
})
export class AuthorizationGuard implements CanActivate, CanActivateChild {
    private type: string;

    constructor(
        private router: Router,
        private _userContextService: UserContextService,
        private _oidcSecurityService: OidcSecurityService) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        this.type = this._userContextService.loginType;
        return this._oidcSecurityService.getIsAuthorized().pipe(take(1))
        .pipe(
            map((isSignIn: boolean) => {
                if (!window.location.hash && !isSignIn) {
                    localStorage.setItem(
                        'business_call_back_url', `${window.location.pathname}${window.location.search}`
                    );
                    const type = localStorage.getItem('login_type');
                    // user not sign in redirect to login page
                    // this.router.navigate(['/login'], { queryParams: { type: this.type } });
                        this.router.navigate(['/login'], { queryParams: { type: this.type } });
                        // this._userContextService.login(type);
                    return false;
                }
                return true;
            }),
            concatMap(isGranted => {
            return this._userContextService.getCurrentUser().pipe(
                filter(currentUser => currentUser != null),
                map((currentUser) => {
                    if (currentUser.status === UserStatus.Inactive) {
                        this.router.navigate(['inactive-account']);
                        return false;
                    }

                    if (currentUser.role && currentUser.role.status === RoleStatus.Inactive) {
                        this.router.navigate(['inactive-role']);
                        return false;
                    }

                    return isGranted;
                })
            );
        }),
            concatMap(isGranted => {
                if (!isGranted) {
                    return of(false);
                }

                if (!route.data || !route.data['permission']) {
                    // user sign in but router not required permission
                    return of(true);
                }

                const mode = route.params['mode'] != null ? route.params['mode'].toLowerCase() : null;
                const permission = mode ? route.data['permission'][mode] : route.data['permission'];

                return this._userContextService.isGranted(permission).map(allowed => {
                    if (allowed) {
                        return true;
                    }
                    // this.router.navigate([this.selectBestRoute()]);
                    this.router.navigate(['/error/401']);
                    return false;
                });
            }));
    }

    public canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        return this.canActivate(route, state);
    }

    selectBestRoute(): string {
        if (this._userContextService.isGranted(AppPermissions.Dashboard)) {
            return '/home';
        }
        return '';
    }
}

