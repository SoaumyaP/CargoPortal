import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core/services/http.service';
import { environment } from 'src/environments/environment';
import { OrganizationSwitchModel } from './user-role-switch.model';

@Injectable()
export class UserRoleSwitchServer {

    constructor(private _httpService: HttpService,
        private _httpClient: HttpClient) {
    }

    /**
     * To search organization by name to switch
     * @param organizationName Search term on Organization name
     * @returns
     */
    searchOrganizationByName$(organizationName: string): Observable<Array<OrganizationSwitchModel>> {
        return this._httpService.get(`${environment.commonApiUrl}/Organizations/UserRoleSwitchMode/Selections?searchTerm=${encodeURIComponent(organizationName)}`);
    }

    /**
     * To switch to specific role in organization
     * @param roleId
     * @param organizationId
     * @returns
     */
    switchToUserRole$(roleId: number, organizationId: number): Observable<Object> {
        const url = `${environment.identityUrl}/Account/SwitchToUserRole?roleId=${roleId}&organizationId=${organizationId}`;

        // Use HttpClient to send ajax with credentials: all related cookies on Identity server will be included
        return this._httpClient.get(url, {
            withCredentials: true
        });
    }

    /**
     * To switch off/exit pretending user role
     * @returns
     */
    switchOffUserRole$(): Observable<Object> {
        const url = `${environment.identityUrl}/Account/SwitchOffUserRole`;
        // Use HttpClient to send ajax with credentials: all related cookies on Identity server will be included
        return this._httpClient.get(url, {
            withCredentials: true
        });
    }
}
