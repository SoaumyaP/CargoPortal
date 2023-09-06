import { Component, OnDestroy, OnInit } from '@angular/core';
import { faArrowAltCircleRight, faArrowAltCircleLeft, faRetweet, faUserFriends } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { translateAggregateResults } from '@progress/kendo-data-query';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { LocalStorageService, Roles, StringHelper } from 'src/app/core';
import { UserContextService } from 'src/app/core/auth/user-context.service';
import { Separator } from 'src/app/core/models/constants/app-constants';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { OrganizationSwitchModel, UserRoleSwitchModel } from './user-role-switch.model';
import { UserRoleSwitchServer } from './user-role-switch.service';

@Component({
    selector: 'app-user-role-switch',
    templateUrl: './user-role-switch.component.html',
    styleUrls: ['./user-role-switch.component.scss']
})
export class UserRoleSwitchComponent implements OnInit, OnDestroy {

    // alias
    Roles = Roles;
    semicolon = Separator.Semicolon;
    tilde = Separator.TILDE;

    // key to store recent organizations into local storage
    localStorageKey: string = 'UserRoleSwitch_recentValues';
    faUserFriends = faUserFriends;
    faArrowAltCircleRight = faArrowAltCircleRight;
    faArrowAltCircleLeft = faArrowAltCircleLeft;

    // component data model
    model: UserRoleSwitchModel = {
        switchOn: false
    };
    // to define if the component is enabled
    isComponentDisabled: boolean = true;

    // to define popup is opening
    isPopupOpening: boolean = false;

    // data sources
    organizationDataSource: Array<OrganizationSwitchModel> = [];
    recentOrganizationDataSource: Array<string> = [];
    searchTermKeyup$ = new Subject<string>();
    searchOrganizationName: string = '';
    isSearching: boolean = false;

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    constructor(
        private _oidcSecurityService: OidcSecurityService,
        private _userContext: UserContextService,
        private _service: UserRoleSwitchServer,
        private _notification: NotificationPopup
    ) {
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {

                // Initialize component
                const enabledUserRoleSwitch = (user.role?.id === Roles.System_Admin || user.role?.id === Roles.CSR || user.isUserRoleSwitch);
                if (enabledUserRoleSwitch != null && enabledUserRoleSwitch) {
                    this.model = {
                        ...this.model,
                        switchOn: user.isUserRoleSwitch,
                        organizationId: user.organizationId,
                        organizationName: user.organizationName,
                        roleId: user.role?.id,
                        roleName: user.role?.name
                    };
                    this.isComponentDisabled = false;
                } else {
                }
            }
        });

        // Register handler for key input on search term
        const sub = this.searchTermKeyup$.pipe(
            debounceTime(1000),
            tap((searchTerm: string) => {
                if (StringHelper.isNullOrEmpty(searchTerm) || searchTerm.length === 0 || searchTerm.length >= 3) {
                    this._fetchOrganizationDataSource(searchTerm);
                }
            }
            )).subscribe();
        this._subscriptions.push(sub);

        // Fetch data for Recent values
        const storedValue = LocalStorageService.read<string>(this.localStorageKey);
        if (!StringHelper.isNullOrEmpty(storedValue)) {
            this.recentOrganizationDataSource = storedValue.split(this.semicolon);
        }
    }

    ngOnInit() {
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }

    /**
     * To open popup
     */
    openPopupContext(): void {
        this.isPopupOpening = !this.isPopupOpening;
    }

    /**
     * To switch to user role from searching result
     * @param roleId
     * @param organizationId
     * @param organizationName
     */
    switchToUserRole(roleId: number, organizationId: number, organizationName: string): void {
        const sub = this._service.switchToUserRole$(roleId, organizationId).subscribe(
            x => {
                this._oidcSecurityService.resetAuthorizationData(false);
                this._storeRecentOrganizations(roleId, organizationId, organizationName);
                window.location.reload();
            },
            error => {
                this._notification.showInfoDialog(
                    'label.titleUnauthorized',
                    'label.unauthorized'
                  );
            }
        );
        this._subscriptions.push(sub);
    }

    /**
     * To switch to user role from recent values
     * @param item
     */
    switchToRecent(item: string): void {
        const splitted = item.split(this.tilde, 3);
        const organizationName: string = splitted[0];
        const organizationId: number = Number.parseInt(splitted[1], 10);
        const roleId: number = Number.parseInt(splitted[2], 10);

        const sub = this._service.switchToUserRole$(roleId, organizationId).subscribe(
            x => {
                this._oidcSecurityService.resetAuthorizationData(false);
                this._storeRecentOrganizations(roleId, organizationId, organizationName);
                window.location.reload();
            },
            error => {
                this._notification.showInfoDialog(
                    'label.titleUnauthorized',
                    'label.unauthorized'
                  );
            }
        );
        this._subscriptions.push(sub);
    }

    /**
     * To switch off / exit pretending user role
     */
    switchOffUserRole(): void {
        const sub = this._service.switchOffUserRole$().subscribe(
            x => {
                this._oidcSecurityService.resetAuthorizationData(false);
                window.location.reload();
            },
            error => {
                this._notification.showInfoDialog(
                    'label.titleUnauthorized',
                    'label.unauthorized'
                  );
            }
        );
        this._subscriptions.push(sub);
    }

    /**
     * To stored selected value to local storage
     * @param roleId
     * @param organizationId
     * @param organizationName
     */
    private _storeRecentOrganizations(roleId: number, organizationId: number, organizationName: string) {

        const toAddValue = `${organizationName}~${organizationId}~${roleId}`;
        const storedValue = LocalStorageService.read<string>(this.localStorageKey);
        let toStoreValue = '';
        let toStoreValues: Array<string> = [];
        if (StringHelper.isNullOrEmpty(storedValue)) {
            toStoreValue = '';
        } else {
            const splitted = storedValue.split(this.semicolon);
            if (splitted != null && splitted.length > 0) {
                toStoreValues = splitted.filter(x => x !== toAddValue);
            }
        }
        toStoreValues.unshift(toAddValue);
        // Only store the last 5 values
        toStoreValue = toStoreValues.slice(0, 5).join(this.semicolon);
        LocalStorageService.write(this.localStorageKey, toStoreValue);
    }

    /**
     * To search by organization name by server-side
     * @param searchTerm Text to search by organization name
     */
    private _fetchOrganizationDataSource(searchTerm: string): void {
        if (!StringHelper.isNullOrEmpty(searchTerm) && searchTerm.length >= 3 ) {
            this.isSearching = true;
            const sub = this._service.searchOrganizationByName$(searchTerm).subscribe(
                orgs => {
                    this.organizationDataSource = orgs;
                    this.isSearching = false;
                }
            );
            this._subscriptions.push(sub);
        }
    }

}

