import { Component, OnInit } from '@angular/core';
import { UserContextService } from 'src/app/core';
import { Router } from '@angular/router';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@Component({
    selector: 'app-signed-out',
    templateUrl: './signed-out.component.html',
    styleUrls: ['./signed-out.component.scss']
})

export class SignedOutComponent implements OnInit {
    constructor(
        private userContext: UserContextService,
        private router: Router,
        private _gaService: GoogleAnalyticsService) {
    }

    ngOnInit() {

        // To reset user information
        this._gaService.config({
                'user_id': 'N/A',
                'user_role': 'N/A',
                'user_org': 'N/A'
            });

        const isInternalUser = this.userContext.loginType === this.userContext.loginTypeEnum.internal;
        if (isInternalUser) {
            this.router.navigate(['/login'], { queryParams: { type: this.userContext.loginTypeEnum.internal }});
        } else {
            this.router.navigate(['/login']);
        }
    }
}
