import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpService, FormService, POFulfillmentStageType, UserContextService, EventLevelMapping, ModeOfTransport, ModeOfTransportType } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { BulkFulfillmentTabModel } from '../models/bulk-fulfillment-tab.model';
import { tap } from 'rxjs/operators';
import { NoteModel } from 'src/app/core/models/note.model';
import { OrganizationReferenceDataModel, UserOrganizationProfileModel } from 'src/app/core/models/organization.model';
import { BulkFulfillmentModel } from '../models/bulk-fulfillment.model';
import { MovementTypes } from 'src/app/core/models/constants/app-constants';

@Injectable()
export class BulkFulfillmentFormService extends FormService<any> {

    private _organizationsCached: Array<OrganizationReferenceDataModel> = [];
    public currentUser: any;

    constructor(httpService: HttpService, private userContext: UserContextService) {
        super(httpService, `${environment.apiUrl}/bulkFulfillments`);

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
    createNavigation(initNav: boolean, isAddMode: boolean = true, isEditMode = false, bookingStage?: POFulfillmentStageType, modeOfTransport?: string, movementType?: string): Array<BulkFulfillmentTabModel> {
        const allSections = [
            {
                text: 'label.general',
                sectionId: 'general',
                selected: false,
                readonly: false
            },
            {
                text: 'label.plannedSchedule',
                sectionId: 'plannedSchedule',
                selected: false,
                readonly: false
            },
            {
                text: 'label.contact',
                sectionId: 'contact',
                selected: false,
                readonly: false
            },
            {
                text: 'label.cargoDetails',
                sectionId: 'cargoDetails',
                selected: false,
                readonly: false
            },
            {
                text: 'label.loadDetails',
                sectionId: 'loadDetails',
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
            },
            {
                text: 'label.dialog',
                sectionId: 'dialog',
                selected: false,
                readonly: false
            }
        ];

        if (initNav) {
            return allSections;
        }

        let hiddenSectionIds = [];
        // Add mode
        if (isAddMode) {
            hiddenSectionIds = ['plannedSchedule', 'loadDetails', 'activity', 'dialog'];
            return allSections.filter(
                section => !hiddenSectionIds.includes(section.sectionId));
        }

        // Other mode
        switch (bookingStage) {
            case POFulfillmentStageType.Draft:
                hiddenSectionIds = ['plannedSchedule', 'loadDetails'];
                if (isEditMode) {
                    hiddenSectionIds.push('dialog');
                }
                break;
            case POFulfillmentStageType.ForwarderBookingRequest:
                hiddenSectionIds = ['plannedSchedule', 'loadDetails'];
                if (isEditMode) {
                    hiddenSectionIds.push('dialog');
                }
                break;
            default:
                if (isEditMode) {
                    hiddenSectionIds = ['dialog'];
                }
                break;
        }

        // Load Details only displays on Sticky menu if Mode of Transport is Sea and Movement Type is CY
        if (!(modeOfTransport === ModeOfTransportType.Sea && movementType === MovementTypes.CYUnderscoreCY)) {
            hiddenSectionIds.push('loadDetails');
        }

        return allSections.filter(section => !hiddenSectionIds.includes(section.sectionId));
    }

    submitBooking(id: number): Observable<any> {
        return this.httpService.create(`${this.apiUrl}/${id}/bookingRequests`);
    }

    createNew(model: any): Observable<any> {
        return this.httpService.create<any>(`${environment.apiUrl}/bulkFulfillments?updateOrganizationPreferences=true&updateOrgContactPreferences=true`, model);
    }

    update(id: string, model: any): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}?updateOrganizationPreferences=true&updateOrgContactPreferences=true`, model);
    }

    cancelBulkFulfillment(id: number, reason: string): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}/cancel`, { reason });
    }

    planToShip(id: number): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}/planToShip`);
    }

    reload(id: string, model: any): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}/re-load`, model);
    }

    amendBooking(id: number): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}/amend`);
    }

    /**
     * Fetch organization reference information. Lookup in cache before calling request to server
     */
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

    getOrganizationByCode(code: string): Observable<OrganizationReferenceDataModel> {
        return this.httpService.getWithCache<OrganizationReferenceDataModel>(`${environment.commonApiUrl}/organizations/${code}/orgReferenceData`);
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

    getAllLocations(): Observable<any[]> {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }

    getNotes(fulfillmentId: number): Observable<NoteModel[]> {
        return this.httpService.get(`${this.apiUrl}/${fulfillmentId}/notes`);
    }

    getMasterNotes(fulfillmentId: number) {
        return this.httpService.get(`${this.apiUrl}/${fulfillmentId}/masterDialogs`);
    }

    getOwnerOrgInfo(username: string): Observable<UserOrganizationProfileModel> {
        return this.httpService.get<UserOrganizationProfileModel>(`${environment.apiUrl}/users/${username}/organization`);
    }

    getPlannedSchedule(fulfillmentId: number): Observable<BulkFulfillmentModel> {
        return this.httpService.get(`${this.apiUrl}/${fulfillmentId}/planned-schedule`);
    }

    getPOFulfillment(fulfillmentId: number): Observable<BulkFulfillmentModel> {
        if (!this.currentUser) {
            return of(null);
        }
        let url = `${this.apiUrl}/${fulfillmentId}`;
        let {isInternal, affiliates} = this.currentUser;
        if (!isInternal) {
            url += `?affiliates=${affiliates}`
        }
        return this.httpService.get(url);
    }
}