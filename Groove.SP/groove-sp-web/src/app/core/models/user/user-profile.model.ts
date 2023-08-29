import { OrganizationType, UserStatus } from '../enums/enums';
import { PermissionModel } from '../permission/permission.model';
import { RoleModel } from '../role/role.model';
import { UserRoleModel } from './user-role.model';

export class UserProfileModel {
    id: number;
    accountNumber: string;
    username: string;
    email: string;
    name: string;
    title: string;
    department: string;
    phone: string;
    companyName: string;
    profilePicture: string;
    status: UserStatus;
    lastSignInDate: string;
    statusName: string;
    isInternal: boolean;
    countryId: number | null;
    organizationId: number | null;
    organizationRoleId: number | null;
    organizationName: string;
    organizationCode: string;
    organizationType: OrganizationType;
    organizationTypeName: string;
    userRoles: UserRoleModel[];
    role: RoleModel;
    permissions: PermissionModel[];
    identityType: string;
    identityTenant: string;
    affiliates: string;
    isUserRoleSwitch: boolean;
}
