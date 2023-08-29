import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { HttpService, FormService, POFulfillmentStageType, UserContextService, StringHelper, ModeOfTransportType } from 'src/app/core';
import { OrganizationReferenceDataModel, UserOrganizationProfileModel } from 'src/app/core/models/organization.model';
import { environment } from 'src/environments/environment';
import { WarehouseFulfillmentTabModel } from '../models/warehouse-fulfillment-tab.model';

@Injectable()
export class WarehouseFulfillmentFormService extends FormService<any> {

    private _organizationsCached: Array<OrganizationReferenceDataModel> = [];
    public currentUser: any;

    constructor(httpService: HttpService, private userContext: UserContextService) {
        super(httpService, `${environment.apiUrl}/warehouseFulfillments`);

        this.userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.affiliateCodes = user.affiliates;
                }
            }
        });
    }

    /**
     * To get list of tabs available on UI
     * @param isAddMode If it is add mode
     * @param bookingStage Current booking stage
     * @returns
     */
    createNavigation(initNav: boolean, isAddMode: boolean = true, isEditMode = false, bookingStage?: POFulfillmentStageType): Array<WarehouseFulfillmentTabModel> {
        const allSections = [
            {
                text: 'label.general',
                sectionId: 'general',
                selected: false,
                readonly: false
            },
            {
                text: 'label.warehouse',
                sectionId: 'warehouse',
                selected: false,
                readonly: true
            },
            {
                text: 'label.contact',
                sectionId: 'contact',
                selected: false,
                readonly: false
            },
            {
                text: 'label.customerPO',
                sectionId: 'customerPO',
                selected: false,
                readonly: false
            },
            {
                text: 'label.attachment',
                sectionId: 'attachment',
                selected: false,
                readonly: false
            },
            {
                text: 'label.activity',
                sectionId: 'activity',
                selected: false,
                readonly: true
            }
        ];

        if (initNav) {
            return allSections;
        }

        let hiddenSectionIds = [];
        // Add mode
        if (isAddMode) {
            hiddenSectionIds = ['activity'];
            return allSections.filter(
                section => !hiddenSectionIds.includes(section.sectionId));
        }

        // Other mode
        switch (bookingStage) {
            case POFulfillmentStageType.Draft:
                hiddenSectionIds = ['warehouse'];
                break;
            case POFulfillmentStageType.ForwarderBookingRequest:
                hiddenSectionIds = ['warehouse'];
                break;
            default:
                break;
        }

        return allSections.filter(section => !hiddenSectionIds.includes(section.sectionId));
    }

    // Override default method to add flag updateOrganizationPreferences
    public createNew(model: any): Observable<any> {
        return this.httpService.create(`${this.apiUrl}?updateOrganizationPreferences=true`, model);
    }

    public cargoReceive(warehouseFulfillmentId: number, model: any): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${warehouseFulfillmentId}/cargoReceive`, model);
    }

    getActivities(id: number, request: any): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/globalidactivities/get-by-poff/${id}`, request);
    }

    getPurchaseOrderReplacedByOrgPreferences(poId: number) {
        return this.httpService.get<any>(`${environment.apiUrl}/purchaseorders/${poId}?replacedByOrganizationReferences=true&affiliates=${this.affiliateCodes}`);
    }

    getOrganizationsByIds(idList: Array<number>): Observable<OrganizationReferenceDataModel[]> {
        // Distinct the values
        idList = idList.filter(this._onlyUnique);

        // Check if available in cache
        const cachedData = this._organizationsCached.filter(y => idList.indexOf(y.id) >= 0);
        if (cachedData.length === idList.length) {
            return of(cachedData);
        }

        // Call to server
        return this.httpService.getWithCache<OrganizationReferenceDataModel[]>(`${environment.commonApiUrl}/organizations/orgReferenceData`, { idList: idList });
    }

    getOrganizations(): Observable<OrganizationReferenceDataModel[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/organizations/orgReferenceData/active`)
            .pipe(
                tap((data: Array<OrganizationReferenceDataModel>) => {
                    this._organizationsCached = data;
                })
            );
    }

    getOrganizationRoles(): Observable<any[]> {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/organizationroles`);
    }

    getAffiliateCodes(id: number): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}/affiliatecodes`);
    }

    getOwnerOrgInfo(username: string): Observable<UserOrganizationProfileModel> {
        return this.httpService.get<UserOrganizationProfileModel>(`${environment.apiUrl}/users/${username}/organization`);
    }

    concatenateAddressLines(address: string, addressLine2?: string, addressLine3?: string, addressLine4?: string) {
        let concatenatedAddress = address;
        if (!StringHelper.isNullOrEmpty(addressLine2)) {
            concatenatedAddress += '\n' + addressLine2;
        }
        if (!StringHelper.isNullOrEmpty(addressLine3)) {
            concatenatedAddress += '\n' + addressLine3;
        }
        if (!StringHelper.isNullOrEmpty(addressLine4)) {
            concatenatedAddress += '\n' + addressLine4;
        }
        return concatenatedAddress;
    }

    cancelWarehouseFulfillment(id: number, reason: string): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}/cancel`, { reason });
    }

    approve(model: any): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${model.id}/approve`, model);
    }

    reject(model: any) {
        return this.httpService.update(`${this.apiUrl}/${model.id}/reject`, model);
    }

    getDefaultMilestone() {
        let milestone =
            [
                {
                    activityCode: '1051',
                    milestone: 'label.forwarderBookingRequest'
                },
                {
                    activityCode: '1061',
                    milestone: 'label.forwarderBookingConfirmed'
                },
                {
                    activityCode: '1063',
                    milestone: 'label.cargoReceived'
                }
            ];

        return milestone.reverse();
    }
}