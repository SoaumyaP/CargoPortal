import { Component, OnInit, ViewChild } from '@angular/core';
import { UserContextService } from '../core/auth';
import { NavigationComponent } from './navigation/navigation.component';
import { Roles } from '../core';
import { OrganizationFormComponent } from '../features/organization/organization-form/organization-form.component';
import { GoogleAnalyticsService } from '../core/services/google-analytics.service';
import { GAEventCategory } from '../core/models/constants/app-constants';

@Component({
    selector: 'app-layout',
    templateUrl: './layout.component.html',
    styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit {

    public currentUser: any;
    public currentLogo: any;
    public navbarExpand: boolean = false;
    public showMenuPopup: boolean = false;
    public defaultAvatarUrl: string;
    public showOrgName: boolean = false;
    public csPortalLogoDefaultPath = 'assets/images/logo.png';
    readonly gaEventCategory = GAEventCategory;
    @ViewChild(NavigationComponent, { static: true }) navigation: NavigationComponent;
    @ViewChild(OrganizationFormComponent, { static: true }) organizationFormComponent: OrganizationFormComponent;

    private menuPopupTimeoutId;

    constructor(public _userContext: UserContextService, private _gaService: GoogleAnalyticsService) {
    }

    ngOnInit() {
        this._userContext.getCurrentUser().subscribe(
            (userData: any) => {
                if (userData) {
                    this.currentUser = userData;
                    this.showOrgName = this.hasOrgName;
                }
            });

        this._userContext.getCurrentOrgLogo().subscribe((logo) => {
            this.currentLogo = logo;
        });
        // Get avatar url default
        this.defaultAvatarUrl = '../assets/images/icon_avatar_default.svg';
    }

    hideMenuPopupDelay() {
        this.menuPopupTimeoutId = setTimeout(() => this.showMenuPopup = false, 200);
    }

    showMenuPopupDelay() {
        clearTimeout(this.menuPopupTimeoutId);
        this.showMenuPopup = true;
    }

    logout() {
        this._userContext.logout();
    }

    onClickNavbar() {
        this.navbarExpand = !this.navbarExpand;
        if (this.navbarExpand) {
            if (this.navigation.expandedItems.indexOf(this.navigation.lastSelectedItem) === -1) {
                this.navigation.expandedItems.push(this.navigation.lastSelectedItem);
            }
        }
    }

    get hasOrgName() {
        return this.currentUser && !this.currentUser.isInternal &&
                (this.currentUser.role.id === Roles.Shipper ||
                this.currentUser.role.id === Roles.Factory ||
                this.currentUser.role.id === Roles.Agent ||
                this.currentUser.role.id === Roles.CruiseAgent ||
                this.currentUser.role.id === Roles.Principal || 
                this.currentUser.role.id === Roles.CruisePrincipal ||
                this.currentUser.role.id === Roles.Warehouse
                );
    }

    public gaEmitEvent(eventAction: string, category: GAEventCategory) {
        const eventName = eventAction.replace(/\s\s+/g, ' ').replace(/\s/g, '_').toLocaleLowerCase();
        this._gaService.emitEvent(eventName, category, eventAction);
    }
}
