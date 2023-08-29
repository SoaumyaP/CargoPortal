import { Component, OnInit, OnDestroy } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
@Component({
    selector: 'app-oidc-callback',
    templateUrl: './oidc-callback.component.html',
    styleUrls: ['./oidc-callback.component.scss'],
})
export class OidcCallbackComponent implements OnInit, OnDestroy {

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    constructor(
        public oidcSecurityService: OidcSecurityService,
        private _router: Router
    ) {
    }

    ngOnInit() {
        // after 5 seconds, will try to verify again
        setTimeout(() => {
            const sub = this.oidcSecurityService
            .getIsAuthorized()
            .pipe(switchMap(async (isAuthorized) => isAuthorized))
            .subscribe((isAuthorized) => {
                if (isAuthorized) {
                    // if (
                    //     window.location.pathname.toLowerCase() ===
                    //     '/oidc-callback'
                    // ) {
                    //     this._router.navigate(['/signed-in']);
                    // } else if (
                    //     window.location.pathname.toLowerCase() ===
                    //     '/signed-in'
                    // ) {
                    //     this._router.navigate(['/home']);
                    // }
                } else {
                    this._router.navigate(['/login']);
                }
            });
            this._subscriptions.push(sub);
        }, 5000);
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }

}
