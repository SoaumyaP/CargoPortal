import { Injectable } from '@angular/core';
import { FormService } from 'src/app/core/form/form.service';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DropDownListItemModel, DropdownListModel, HttpService } from 'src/app/core';

@Injectable()
export class OrganizationFormService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/organizations`);
    }

    getLocationDropDown(countryId) {
        return this.httpService.get(
            `${environment.commonApiUrl}/countries/${countryId}/Locations/DropDown`);
    }

    getOrganizationCodeOptions(id) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}/otherCodeDropDown`);
    }

    getAffiliateOrganizationDropdown(id) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}/affiliateDropdown`);
    }

    getActiveCodesExcludeIds(excludeOrgIds) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/activeCodesExcludeIds`, {excludeOrgIds: excludeOrgIds});
    }

    getUsers(id) {
        return this.httpService.get(`${environment.apiUrl}/users/byOrganization/${id}`);
    }

    getOrganization(id) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}`);
    }

    getOrgByName(name:string) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/getByName/${name}`);
    }

    getOrgsWithFulltextSearchByName(name:string) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/getByFullTextSearchName/${name}`);
    }

    getAffiliates(id) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}/affiliates`);
    }

    checkCustomerPrefixNotTaken(customerPrefix, organizationId) {
        return this.httpService.create<any>(`${environment.commonApiUrl}/organizations/checkCustomerPrefix`, {
            customerPrefix,
            id: organizationId
        });
    }

    hasBuyerCompliance(organizationId) {
        return this.httpService.get<Array<any>>(`${environment.apiUrl}/buyercompliances`, { organizationId: organizationId }).pipe(
            map(res => {
                return res.length > 0;
            }));
    }

    addAffiliate(organizationId, affiliate: any) {
        return this.httpService.create(`${environment.commonApiUrl}/organizations/${organizationId}/affiliates`, affiliate);
    }

    removeAffiliate(organizationId, affiliateId) {
        return this.httpService.delete(`${environment.commonApiUrl}/organizations/${organizationId}/affiliates/${affiliateId}`);
    }

    updateStatusUsers(organizationId, userStatus) {
        const user = {
            status: userStatus
        };

        return this.httpService.update(`${environment.apiUrl}/users/updateStatusUsers/${organizationId}`, user);
    }

    updateUserStatus(userId, userStatus) {
        const model = {
            status: userStatus
        };

        return this.httpService.update(`${environment.apiUrl}/users/${userId}/status`, model);
    }

    deleteUser(userId) {
        return this.httpService.delete(`${environment.apiUrl}/users/${userId}`);
    }

    resendConnectionToCustomer(supplierId, customerId) {
        return this.httpService.update(`${environment.commonApiUrl}/organizations/${supplierId}/resendConnectionToCustomer/${customerId}`);
    }

    updateAdminUser(model) {
        return this.httpService.update(`${environment.commonApiUrl}/organizations/${model.id}/updateAdminUser/`, model);
    }

    updateUserOrganization(organizationId, organization) {
        const user = {
            organizationName: organization.name,
            organizationType: organization.organizationType
        };

        return this.httpService.update(`${environment.apiUrl}/users/updateOrganization/${organizationId}`, user);
    }

    // Customer Relationship
    getCustomers(id): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}/customers`);
    }

    addCustomer(
        supplierId: number,
        customerId: number,
        connectionType: number
    ): Observable<null> {
        const model = {
            supplierId,
            customerId,
            connectionType
        };
        return this.httpService.create(`${environment.commonApiUrl}/organizations/${supplierId}/customers`, model);
    }

    removeCustomer(
        organizationId: number,
        customerId: number,
    ): Observable<null> {
        return this.httpService.delete(`${environment.commonApiUrl}/organizations/${organizationId}/customers/${customerId}`);
    }
    //end Customer Relationship

    createUser(organizationId, user) {
        return true;
    }

    getUserRoles() {
        return this.httpService.get(`${environment.apiUrl}/roles`);
    }

    checkUserExists(email) {
        return this.httpService.get(`${environment.apiUrl}/users/checkExistsUser?email=${encodeURIComponent(email)}`);
    }

    addUser(user) {
        return this.httpService.create(`${environment.apiUrl}/users/external`, user);
    }

    updateCustomerRefId(customerId: number, supplierId: number, customerRefId: string) {
        const model = {
            customerRefId
        };

        return this.httpService.update(`${environment.commonApiUrl}/supplierRelationships?customerId=${customerId}&supplierId=${supplierId}`, model);
    }

    getWarehouseAssignments(customerOrgId: number) {
        return this.httpService.get(`${this.apiUrl}/${customerOrgId}/warehouseAssignments`);
    }

    getWarehouseLocationDropdownOptions(code: string = '') {
        return this.httpService.get(`${environment.commonApiUrl}/warehouseLocations/dropdown?searchTerm=${code}`);
    }

    createWarehouseAssignment(customerOrgId: number, model: any) {
        return this.httpService.create(`${this.apiUrl}/${customerOrgId}/warehouseAssignments`, model);
    }

    deleteWarehouseAssignment(customerOrgId: number, warehouseLocationId: number) {
        return this.httpService.delete(`${this.apiUrl}/${customerOrgId}/warehouseAssignments/${warehouseLocationId}`);
    }
}
