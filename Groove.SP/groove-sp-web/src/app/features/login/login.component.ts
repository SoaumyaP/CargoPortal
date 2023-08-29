import { Component, OnInit, OnDestroy } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Subscription } from 'rxjs';
import { take } from 'rxjs/operators';
import { UserContextService } from 'src/app/core/auth';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

    private _subscriptions: Array<Subscription> = [];
    constructor(
        public oidcSecurityService: OidcSecurityService,
        private route: ActivatedRoute,
        private router: Router,
        private userContext: UserContextService) {
    }
    ngOnInit(): void {
        const sub = combineLatest([
            this.route.queryParams,
            this.oidcSecurityService.getIsAuthorized().pipe(take(1))
        ])
        .subscribe(([routeParams, isAuthorized]) => {
            if (isAuthorized) {
                // Redirect it to signed-in page for futher redirect.
                this.router.navigate(['/signed-in']);
            } else {
                this.userContext.login(routeParams['type']);
            }
        });
        this._subscriptions.push(sub);
    }

    ngOnDestroy() {
        this._subscriptions.map(x => {x.unsubscribe(); });
    }
}
