import { Injectable } from '@angular/core';
import { Roles, OrganizationType } from 'src/app/core';

@Injectable()
export class UsersService {

    public getRequiredOrgTypeByRole(roleId: Roles) {
        let orgType: OrganizationType;
        
        if (roleId === Roles.Principal || roleId === Roles.CruisePrincipal) {
            orgType = OrganizationType.Principal;
        } else if (roleId === Roles.Shipper || roleId === Roles.Factory) {
            orgType = OrganizationType.General;
        }
        if (roleId === Roles.Agent || roleId === Roles.CruiseAgent || roleId === Roles.Warehouse) {
            orgType = OrganizationType.Agent;
        }
        
        return orgType;
    }

    public getOrganizationListByInput(organizations: any[], roleId: Roles, input: string): any[] {
        const requiredOrgType = this.getRequiredOrgTypeByRole(roleId);

        return organizations.filter(x => {
            if (requiredOrgType) {
                return x.orgCodeName.toLowerCase().indexOf(input.toLowerCase()) !== -1
                    && x.organizationType === requiredOrgType;
            }

            return x.orgCodeName.toLowerCase().indexOf(input.toLowerCase()) !== -1;
        });
    }
}