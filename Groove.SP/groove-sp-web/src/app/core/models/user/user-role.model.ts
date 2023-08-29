import { ModelBase } from '../model-base.model';
import { RoleModel } from '../role/role.model';

export class UserRoleModel extends ModelBase {
    userId: number;
    roleId: number;
    role: RoleModel;
}