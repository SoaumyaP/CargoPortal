import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { DropDowns, OrganizationNameRole, StringHelper, ValidationDataType } from 'src/app/core';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { WarehouseFulfillmentModel } from '../models/warehouse-fulfillment.model';
import { WarehouseFulfillmentFormService } from '../warehouse-fulfillment-form/warehouse-fulfillment-form.service';

@Component({
  selector: 'app-warehouse-fulfillment-general-info',
  templateUrl: './warehouse-fulfillment-general-info.component.html',
  styleUrls: ['./warehouse-fulfillment-general-info.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class WarehouseFulfillmentGeneralInfoComponent implements OnInit, OnChanges {
  @Input() model: WarehouseFulfillmentModel;

  /**
   * Organization id of the principal contact
   */
  @Input() customerId: string;

  /**
   * Company name of the supplier contact
   */
  @Input() supplierName: string;

  // It is prefix for formErrors and validationRules
  // Use it to detect what tab contains invalid data
  @Input() tabPrefix: string;

  @Input() saveAsDraft: boolean;

  @Input() formErrors: any;
  @Input() isViewMode: boolean;
  @Input() isEditMode: boolean;

  customerName: number;
  incotermTypeOptions = DropDowns.IncotermStringType;
  public defaultDropDownItem: { text: string, label: string, description: string, value: number } =
    {
      text: 'label.select',
      label: 'label.select',
      description: 'select',
      value: null
    };

  constructor(private service: WarehouseFulfillmentFormService) { }

  ngOnInit() {

  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!StringHelper.isNullOrEmpty(changes.customerId?.currentValue)) {
      
      const principalContact = this.model?.contacts?.find(
        (c) => c.organizationRole === OrganizationNameRole.Principal && c.organizationId === this.customerId
      );
      if (principalContact) {
        this.customerName = principalContact.companyName;
        this.service
          .getOrganizationsByIds([principalContact.organizationId])
          .subscribe((data) => {
            if (data.length > 0) {
              this.model.customerPrefix = data[0].customerPrefix;
            }
          });
      }
    }

  }

  public validateBeforeSaving(): ValidationData[] {
    let result: ValidationData[] = [];
    // In case there is any error
    const errors = Object.keys(this.formErrors)?.filter(x => x.startsWith(this.tabPrefix));
    for (let index = 0; index < errors.length; index++) {
      const err = Reflect.get(this.formErrors, errors[index]);
      if (err && !StringHelper.isNullOrEmpty(err)) {
        result.push(new ValidationData(ValidationDataType.Input, false));
      }
    }
    return result;
  }
}