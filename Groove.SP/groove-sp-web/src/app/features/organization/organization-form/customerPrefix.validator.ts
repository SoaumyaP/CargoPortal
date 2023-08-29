import { AbstractControl } from '@angular/forms';
import { OrganizationFormService } from './organization-form.service';
import { map} from 'rxjs/operators';

export class ValidateEmailNotTaken {
  static createValidator(organizationService: OrganizationFormService, organizationId) {
    return (control: AbstractControl) => {
      return organizationService.checkCustomerPrefixNotTaken(control.value, organizationId).map(res => {
        return res.notTaken ? null : {customerPrefixTaken: true};
      });
    }
  }
}