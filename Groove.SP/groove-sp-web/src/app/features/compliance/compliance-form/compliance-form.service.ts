import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService, FormService, DropDownListItemModel } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class ComplianceFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/buyercompliances`);
    }

    getOrganizations() {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/organizations/activeCodes`);
    }

    getAgentOrganization() {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/AgentOrganization`);
    }

    checkOrganizationExists(id, organizationId) {
        return this.httpService.get(`${environment.apiUrl}/buyercompliances/${id}/isOrganizationExists/${organizationId}`);
    }

    checkAgentOrgHasContactEmail(organizationId) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${organizationId}/CheckContactEmail`);
    }

    checkOrganizationHasCustomerPrefix(organizationId) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${organizationId}/HasCustomerPrefix`);
    }

    getAllLocations() {
        return this.httpService.get(`${environment.commonApiUrl}/countries/AllLocations`);
    }

    getAllCarriers() {
        return this.httpService.get(`${environment.commonApiUrl}/carriers/DropDown`);
    }

    activate(model) {
        return this.httpService.update(`${environment.apiUrl}/buyercompliances/${model.id}/activate`, model);
    }

    updateOrganizationBuyer(model) {
        return this.httpService.update(`${environment.commonApiUrl}/organizations/${model.id}/updateBuyer`, model);
    }

    updateOrganization(model) {
        return this.httpService.update(`${environment.commonApiUrl}/organizations/${model.id}`, model);
    }

    getCountries() {
        return this.httpService.get(`${environment.commonApiUrl}/countries/DropDown`);
    }

    getWarehouseCustomerDropdown(): Observable<Array<DropDownListItemModel<number>>> {
        return this.httpService.get<Array<DropDownListItemModel<number>>>(`${environment.apiUrl}/buyercompliances/dropdown/warehouse-service-type`);
    }

    times: string[]= ['12:00'];
    initializeTime() {
        const lastTime = this.times[this.times.length - 1]
        const hour = +lastTime.split(':')[0];
        const minute = lastTime.split(':')[1];
        let newTime = '';
        
        if (minute === '30') {
            newTime = `${(hour + 1) ===13 ? 1: hour + 1}:00`
        } else {
            newTime = `${hour}:30`
        }
        
        if (this.times.some(c => c.startsWith(newTime)) || this.times.length > 100) {
            return [].concat(this.times.map(c=>`${c} AM`),this.times.map(c=>`${c} PM`));
        }

        this.times.push(newTime);
        return this.initializeTime();
    }
}
