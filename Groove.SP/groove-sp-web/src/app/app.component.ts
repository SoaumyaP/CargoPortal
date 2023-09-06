import { Component, OnInit, OnDestroy } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { take, filter } from 'rxjs/operators';
import { UserContextService } from './core/auth';
import { Router, RoutesRecognized, ActivationStart, NavigationEnd } from '@angular/router';
import { CustomTranslationService } from './core/translation/custom-translation.service';
import { Title } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { StringHelper, UserTrackTraceService } from './core';
import { environment } from 'src/environments/environment';
import { GoogleAnalyticsService } from './core/services/google-analytics.service';
import { SignalRService } from './core/services/signal-r';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
    title = 'groove-sp-web';

    public notificationOptions = {
        position: ['top', 'right'],
        timeOut: 2000,
        maximumLength: 512,
        maxStack: 13,
        lastOnBottom: true,
        showProgressBar: true,
        pauseOnHover: true,
        clickToClose: false,
        preventDuplicates: false,
        preventLastDuplicates: false,
        animate: 'fromRight'
    };

    isAuthorizeCompleted = false;

    private _subscriptions: Array<Subscription> = [];

    constructor(
        private oidcSecurityService: OidcSecurityService,
        private signalRService: SignalRService,
        private _userContextService: UserContextService,
        private router: Router,
        public customTranslationService: CustomTranslationService,
        private titleService: Title,
        private _userTrackTraceService: UserTrackTraceService,
        private _gaService: GoogleAnalyticsService) {

        this.oidcSecurityService.getIsModuleSetup().pipe(
                filter((isModuleSetup: boolean) => isModuleSetup),
                take(1)
        ).subscribe((isModuleSetup: boolean) => {
                this.doCallbackLogicIfRequired();
        });

        this._userContextService.getCurrentUser().subscribe((user) => {
            if (user != null) {
                this.isAuthorizeCompleted = true;
                // To set user information
                const gaValue = {
                    'user_id': user.email,
                    'user_role': user.role?.name ?? 'N/A',
                    'user_org': user.organizationName ?? 'N/A'
                };
                this._gaService.config(gaValue);
                
                this.signalRService.startConnection();
                this.signalRService.addPushNotificationListener();
            }
        });
    }

    ngOnInit() {
        this.addGAScript();

        this.isAuthorizeCompleted = false;
        this._userContextService.initialize();

        this.router.events.subscribe(val => {
            if (val instanceof RoutesRecognized) {
                if (val.state.root.firstChild.data['allowAnonymous']) {
                    this.isAuthorizeCompleted = true;
                }
            }

            // page name
            if (val instanceof ActivationStart) {
                const data = val['snapshot'].data;
                if (data !== null) {
                    const titleMsg = 'label.shipmentPortal';
                    const detailMsg = 'label.' + data['pageName'];
                    this.customTranslationService.translateService.stream([titleMsg, detailMsg]).subscribe(msgs => {
                        const title = msgs[titleMsg];
                        const detail = msgs[detailMsg];
                        if (title.indexOf('label.') < 0 && detail.indexOf('label.') < 0) {
                            this.titleService.setTitle(title + ' - ' + detail);
                        }
                    });
                }
            }
        });

        this.router.events
        .pipe(filter((rs): rs is NavigationEnd => rs instanceof NavigationEnd))
        .subscribe(event => {
            if (
                event.id === 1 &&
                event.url === event.urlAfterRedirects
            ) {
                // Your code here for when the page is refreshed
                const ignoreTrackingUrls = [
                    '/oidc-callback',
                    'login?type=in',
                    'login?type=ex'
                ];
                if (!ignoreTrackingUrls.some(x => event.urlAfterRedirects.endsWith(x))) {
                    this._gaService.emitPageView(
                        {
                            page_location: environment.spaUrl + event.urlAfterRedirects,
                            page_path: event.urlAfterRedirects
                        }
                    );
                }
            }
        })

        // track and trace user login
        this._userTrackTraceService.start();
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }

    private doCallbackLogicIfRequired() {
        if (window.location.hash) {
            this.oidcSecurityService.authorizedImplicitFlowCallback();
        }
    }

    /** Add Google Analytics Script Dynamically */
    private addGAScript() {
        if (!StringHelper.isNullOrEmpty(environment.gaTrackingId)) {
            const gtagScript: HTMLScriptElement = document.createElement('script');
            gtagScript.async = true;
            gtagScript.src = 'https://www.googletagmanager.com/gtag/js?id=' + environment.gaTrackingId;
            document.head.prepend(gtagScript);
            /** Disable automatic page view hit to fix duplicate page view count  **/
            // gtag('config', environment.gaTrackingId, { send_page_view: false });
            this._gaService.config();
        }
    }
}
