import { RoleStatus } from '../enums/enums';
import { ModelBase } from '../model-base.model';

export class RoleModel extends ModelBase {
    id: number;
    name: string;
    description: string;
    activated: boolean;
    isInternal: boolean;
    status: RoleStatus;
    isOfficial: boolean;
    statusName: string;
    permissionIds: number[];
}