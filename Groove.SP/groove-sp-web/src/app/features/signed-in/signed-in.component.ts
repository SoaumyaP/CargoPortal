import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Router } from '@angular/router';
import { HttpService, StringHelper, UserContextService, UserTrackTraceService } from 'src/app/core';

@Component({
    selector: 'app-signed-in',
    templateUrl: './signed-in.component.html',
    styleUrls: ['./signed-in.component.scss'],
})
export class SignedInComponent implements OnInit {
    constructor(
        private httpService: HttpService,
        private router: Router,
        private _userTrackTraceService: UserTrackTraceService,
        private _userContext: UserContextService
        ) {}

    ngOnInit() {
        const isUserRoleSwitch = this._userContext.currentUser?.isUserRoleSwitch;

        // Will not log/track-trace if current user is in switch mode
        if (StringHelper.isNullOrEmpty(isUserRoleSwitch) || !isUserRoleSwitch) {
            this._userTrackTraceService.track('Login');

            // to this page only after logged-in successfully
            this.updateLastSignInDate().subscribe();
        }

        // redirect back to previous page but ignore login & oidc-callback
        const businessCallbackUrl = localStorage.getItem(
            'business_call_back_url'
        );
        const ignoreCallbackUrls: Array<string> = ['/login', '/oidc-callback'];
        if (!StringHelper.isNullOrEmpty(businessCallbackUrl) && !ignoreCallbackUrls.some(x => businessCallbackUrl.toLowerCase().startsWith(x))) {
            this.router.navigateByUrl(businessCallbackUrl);
        } else {
            this.router.navigate(['/home']);
        }
        localStorage.removeItem('business_call_back_url');
    }

    updateLastSignInDate() {
        return this.httpService.update(
            `${environment.apiUrl}/users/current/lastSignIn`
        );
    }
}
