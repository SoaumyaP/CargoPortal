export interface UserRoleSwitchModel {
    organizationId?: number;
    organizationName?: string;
    roleId?: number;
    roleName?: string;
    switchOn: boolean;
}

export interface OrganizationSwitchModel {
    id: number;
    name: string;
    organizationType: number;
    roleId: number;
}
