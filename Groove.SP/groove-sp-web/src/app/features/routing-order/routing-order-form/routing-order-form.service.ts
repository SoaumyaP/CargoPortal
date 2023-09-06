import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpService, FormService, UserContextService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { tap } from 'rxjs/operators';
import { NoteModel } from 'src/app/core/models/note.model';
import { OrganizationReferenceDataModel, UserOrganizationProfileModel } from 'src/app/core/models/organization.model';

@Injectable()
export class RoutingOrderFormService extends FormService<any> {

    private _organizationsCached: Array<OrganizationReferenceDataModel> = [];
    public currentUser: any;

    constructor(httpService: HttpService, private userContext: UserContextService) {
        super(httpService, `${environment.apiUrl}/routingOrders`);

        this.userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.affiliateCodes = user.affiliates;
                }
            }
        });
    }

    update(id: string, model: any): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}?updateOrganizationPreferences=true&updateOrgContactPreferences=true`, model);
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

    getNotes(routingOrderId: number): Observable<NoteModel[]> {
        return this.httpService.get(`${this.apiUrl}/${routingOrderId}/notes`);
    }

    getMasterNotes(routingOrderId: number) {
        return this.httpService.get(`${this.apiUrl}/${routingOrderId}/masterDialogs`);
    }

    getOwnerOrgInfo(username: string): Observable<UserOrganizationProfileModel> {
        return this.httpService.get<UserOrganizationProfileModel>(`${environment.apiUrl}/users/${username}/organization`);
    }
}