import { Observable, throwError } from 'rxjs';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Injectable, Injector } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpEvent, HttpHandler, HttpErrorResponse } from '@angular/common/http';
import { Router} from '@angular/router';
import { catchError, finalize } from 'rxjs/operators';
import { UserContextService } from './user-context.service';
import { LoaderService } from 'src/app/ui/loader/loader.service';
import { StringHelper } from '..';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';


@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    private _oidcSecurityService: OidcSecurityService;
    private _userContext: UserContextService;
    private httpRequestCount = 0;

    constructor(private injector: Injector,
        private router: Router,
        private loaderService: LoaderService,
        private _notification: NotificationPopup) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (req.method === 'POST' || req.method === 'PUT' || req.method === 'DELETE' || req.method === 'PATCH') {
            const userContext = this._getUserContext();

            // All create/update/delete action will be forbidden in user role switch mode
            const isUserRoleSwitch = userContext.currentUser?.isUserRoleSwitch;
            if (isUserRoleSwitch) {
                this._notification.showInfoDialog(
                    'msg.forbiddenActionInSwitchMode',
                    'label.unauthorized'
                  );
                return throwError (`${req.url} is forbidden in switch mode.`);
            }
            // If intend to set reportProgress = false, it is fired in background, not need to count
            if (req.reportProgress === false) {
            } else {
                this.httpRequestCount++;

                if (this.httpRequestCount >= 1) {
                    this.loaderService.show();
                }
            }
        }

        return next.handle(this._setHeaders(req))
            .pipe(
                catchError((error: HttpErrorResponse) => {
                    return this.handleError(error);
                }),
                finalize(() => {
                    const currentReq = req;
                    if (currentReq.method === 'POST' || currentReq.method === 'PUT' || currentReq.method === 'DELETE') {
                        this.httpRequestCount--;
                        if (this.httpRequestCount <= 0) {
                            this.loaderService.hide();
                            if (this.httpRequestCount < 0) {
                                this.httpRequestCount = 0;
                            }
                        }
                    }
                    return;
                })
            );
    }

    private handleError(error: HttpErrorResponse) {

        if (error.status === 403) {
            this.router.navigate(['/error/401']);
        } else if (error.status === 401) {
            const userContext = this._getUserContext();

            // store current url, then redirect in signed-in page
            localStorage.setItem(
                'business_call_back_url', `${window.location.pathname}${window.location.search}`
            );
            const type = localStorage.getItem('login_type');

            // must to call logoff to clean somethings on library storage.
            this._oidcSecurityService.logoff(() => {
                this.router.navigate(['/login'], { queryParams: { type: userContext.loginType } });
            });
        }
        return throwError(error);
    }

    private _setHeaders(req: HttpRequest<any>): HttpRequest<any> {
        let headers = req.headers
            //TODO: Can not upload file
            // .set('Content-Type', 'application/json')
            .set('Accept', 'application/json');

        if (localStorage.getItem('lang') != null) {
            headers = headers.set('Culture', `c=${localStorage.getItem('lang')}|uic=null`);
        }

        // Ignore setting Authorization
        const isCallingToReportingServer = req.headers.get('CSPortal-IgnoreAuthInterceptor');
        if (isCallingToReportingServer !== 'true') {
            if (this._oidcSecurityService === undefined) {
                this._oidcSecurityService = this.injector.get(OidcSecurityService);
            }
            if (this._oidcSecurityService !== undefined) {
                const token = this._oidcSecurityService.getToken();
                if (token !== '') {
                    headers = headers.set('Authorization', 'Bearer ' + token);
                }
            }
        }

        return req.clone({
            headers: headers
        });
    }

    private _getUserContext(): UserContextService {
        if (this._userContext === undefined) {
            this._userContext = this.injector.get(UserContextService);
        }
        return this._userContext;
    }
}
