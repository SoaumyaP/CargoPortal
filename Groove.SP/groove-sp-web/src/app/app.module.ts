import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import {
    AuthModule,
    OidcSecurityService,
    OpenIDImplicitFlowConfiguration,
    OidcConfigService,
    AuthWellKnownEndpoints
} from 'angular-auth-oidc-client';
import { SignalRService } from './core/services/signal-r';
import { environment } from '../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core';
import { UiModule } from './ui';
import { SimpleNotificationsModule } from 'angular2-notifications';
import { ChartsModule } from '@progress/kendo-angular-charts';
import 'hammerjs';


export function loadConfig(oidcConfigService: OidcConfigService) {
    return () => oidcConfigService.load_using_stsServer(environment.identityUrl);
}

@NgModule({
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        SimpleNotificationsModule.forRoot(),
        AppRoutingModule,
        AuthModule.forRoot(),

        CoreModule,
        UiModule.forRoot(),
        ChartsModule
    ],
    providers: [
        OidcConfigService,
        OidcSecurityService,
        {
            provide: APP_INITIALIZER,
            useFactory: loadConfig,
            deps: [OidcConfigService],
            multi: true,
        },
        SignalRService
    ],
    bootstrap: [AppComponent]
})

export class AppModule {
    constructor(
        private oidcSecurityService: OidcSecurityService,
        private oidcConfigService: OidcConfigService
    ) {
        const _this = this;
        this.oidcConfigService.onConfigurationLoaded.subscribe(() => {
            const openIDImplicitFlowConfiguration = new OpenIDImplicitFlowConfiguration();

            openIDImplicitFlowConfiguration.stsServer = environment.identityUrl;
            // openIDImplicitFlowConfiguration.redirect_url = environment.spaUrl;
            // redirect back to application with token as hash
            // this page oidc-callback without any processing, just leave for OIDC library handles behind.
            openIDImplicitFlowConfiguration.redirect_url = environment.spaUrl + '/oidc-callback';

            // The Client MUST validate that the aud (audience) Claim contains its client_id value registered
            // at the Issuer identified by the iss (issuer) Claim as an audience.
            // The ID Token MUST be rejected if the ID Token does not list the Client as a valid audience,
            // or if it contains additional audiences not trusted by the Client.
            openIDImplicitFlowConfiguration.client_id = 'spclient';
            openIDImplicitFlowConfiguration.response_type = 'id_token token';
            openIDImplicitFlowConfiguration.scope = 'openid spapi csfecommonapi supplementalapi';
            openIDImplicitFlowConfiguration.post_logout_redirect_uri = `${environment.spaUrl}/signed-out`;
            openIDImplicitFlowConfiguration.start_checksession = false;
            openIDImplicitFlowConfiguration.silent_renew = true;
            openIDImplicitFlowConfiguration.silent_renew_url = `${environment.spaUrl}/assets/static/silent-renew.html`;
            openIDImplicitFlowConfiguration.post_login_route = '/signed-in';

            // HTTP 403
            openIDImplicitFlowConfiguration.forbidden_route = '/error/401';
            // HTTP 401
            openIDImplicitFlowConfiguration.unauthorized_route = '/error/401';
            openIDImplicitFlowConfiguration.log_console_warning_active = !environment.production;
            openIDImplicitFlowConfiguration.log_console_debug_active = !environment.production;
            // id_token C8: The iat Claim can be used to reject tokens that were issued too far away from the current time,
            // limiting the amount of time that nonces need to be stored to prevent attacks.The acceptable range is Client specific.
            // allow within 60 minutes
            openIDImplicitFlowConfiguration.max_id_token_iat_offset_allowed_in_seconds = 3600;
            openIDImplicitFlowConfiguration.auto_clean_state_after_authentication = false;
            // storage
            openIDImplicitFlowConfiguration.storage = localStorage;
            const authWellKnownEndpoints = new AuthWellKnownEndpoints();
            authWellKnownEndpoints.setWellKnownEndpoints(_this.oidcConfigService.wellKnownEndpoints);

            _this.oidcSecurityService.setupModule(
                openIDImplicitFlowConfiguration,
                authWellKnownEndpoints
            );
        });
    }
}
