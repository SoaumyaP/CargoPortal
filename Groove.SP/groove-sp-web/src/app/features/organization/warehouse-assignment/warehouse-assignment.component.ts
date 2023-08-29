import {
  Component,
  Input,
  OnInit,
  ViewChild
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { StringHelper, UserContextService } from 'src/app/core';
import { WarehouseAssignmentModel } from 'src/app/core/models/warehouse-assignment/warehouse-assignment.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { OrganizationFormService } from '../organization-form/organization-form.service';

@Component({
  selector: 'app-warehouse-assignment',
  templateUrl: './warehouse-assignment.component.html',
  styleUrls: ['./warehouse-assignment.component.scss']
})
export class WarehouseAssignmentComponent implements OnInit {
  /**
   * Customer organization id.
   */
  @Input() customerId: number;

  /**
   * Can not add/edit if value is false.
   */
  @Input() isEditable: boolean;

  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  warehouseAssignments: Array<WarehouseAssignmentModel> = [];

  warehouseLocationOptions: any[] = [];
  filteredWarehouseLocationOptions: any[] = [];

  currentUser: any;

  constructor(
    private _orgFormService: OrganizationFormService,
    private _userCtxService: UserContextService,
    public _notification: NotificationPopup,
    public _translateService: TranslateService) {
    _userCtxService.getCurrentUser().subscribe((user: any) => {
      if (user) {
        this.currentUser = user;
      }
    })
  }

  ngOnInit() {
    if (this.customerId) {
      this._orgFormService.getWarehouseAssignments(this.customerId).subscribe((res: any) => {
        this.warehouseAssignments = res;
      });
    }
    this._orgFormService.getWarehouseLocationDropdownOptions().subscribe((res: any) => {
      this.warehouseLocationOptions = res;
    });
  }

  addBlank(): void {
    this.warehouseAssignments.push(
      {
        warehouseLocation: {
          id: null,
          code: null,
          name: null,
          address: null,
          contactPerson: null,
          contactPhone: null,
          contactEmail: null,
          locationId: null,
          organizationId: null
        },
        contactEmail: null,
        contactPerson: null,
        contactPhone: null,
        warehouseLocationId: null,
        organizationId: null,
        isAddLine: true
      });

    // filter available warehouse options.
    this.filteredWarehouseLocationOptions = this.warehouseLocationOptions.filter(
      (x) => this.warehouseAssignments
        .filter(x => !x.isAddLine)
        .map(x => x.warehouseLocation)
        .findIndex(y => y.id === x.value) === -1
    );
  }

  onWarehouseCodeChanged(value, index) {
    const warehouse = this.warehouseLocationOptions.find(x => x.value === value);
    if (warehouse) {
      this.warehouseAssignments[index].warehouseLocation.code = warehouse.label;
      this.warehouseAssignments[index].warehouseLocation.name = warehouse.name;
      this.warehouseAssignments[index].warehouseLocationId = warehouse.value;
      this.warehouseAssignments[index].contactPerson = warehouse.contactPerson;
      this.warehouseAssignments[index].contactPhone = warehouse.contactPhone;
      this.warehouseAssignments[index].contactEmail = warehouse.contactEmail;
    }
  }

  assignWarehouse(dataItem): void {
    this._orgFormService.createWarehouseAssignment(this.customerId, dataItem).subscribe(
      (res) => {
        dataItem.isAddLine = false;
        this._notification.showSuccessPopup('save.sucessNotification', 'label.organization')
      },
      (err) => this._notification.showErrorPopup('save.failureNotification', 'label.organization')
    );
  }

  removeWarehouse(dataItem, rowIndex) {
    const warehouseAssignment = this.warehouseAssignments[rowIndex];
    if (warehouseAssignment.isAddLine) {
      this.warehouseAssignments.splice(rowIndex, 1);
      return;
    }

    if (!warehouseAssignment.warehouseLocation.id) {
      return;
    }

    const confirmDlg = this._notification.showConfirmationDialog('delete.saveConfirmation', 'label.organization');
    confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          this._orgFormService.deleteWarehouseAssignment(this.customerId, warehouseAssignment.warehouseLocation.id).subscribe(
            (res) => {
              this.warehouseAssignments.splice(rowIndex, 1);
              this._notification.showSuccessPopup('save.sucessNotification', 'label.organization')
            },
            (err) => this._notification.showErrorPopup('save.failureNotification', 'label.organization')
          );
        }
      });
  }

  // Handler for Contact email

  onContactEmailValueChanged(value: string): void {
    if (StringHelper.isNullOrEmpty(value)) {
      this.mainForm.controls['contactEmail'].setErrors(null);
      return;
    }

    if (StringHelper.validateEmailSeparateByComma(value)) {
      this.mainForm.controls['contactEmail'].setErrors(null);
    } else {
      this.mainForm.controls['contactEmail'].setErrors({ 'incorrect': true });
    }
  }

  // convenience getter for easy access to form fields
  frmControlByName(
    name: string
  ) {
    if (this.mainForm?.controls) {
      return this.mainForm.controls[name];
    }
    return null;
  }

  get isAdding(): boolean {
    return this.warehouseAssignments.findIndex(x => x.isAddLine) !== -1;
  }
}