import { Injectable } from '@angular/core';
import { CanLoad } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Injectable({
    providedIn: 'root'
})
export class AuthorizationCanGuard implements CanLoad {

    constructor(private oidcSecurityService: OidcSecurityService
    ) {}

    canLoad(): boolean {
        return this.oidcSecurityService.moduleSetup;
    }
}
