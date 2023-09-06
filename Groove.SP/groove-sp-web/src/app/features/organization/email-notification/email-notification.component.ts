import { Component, Input, OnDestroy, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { AbstractControl, NgForm } from '@angular/forms';
import { faBan, faPencilAlt } from '@fortawesome/free-solid-svg-icons';
import { MultiSelectComponent } from '@progress/kendo-angular-dropdowns';
import { Observable, of, Subscription } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { DropDownListItemModel, StringHelper, UserContextService } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { EmailNotificationService } from './email-notification.service';

@Component({
  selector: 'app-email-notification',
  templateUrl: './email-notification.component.html',
  styleUrls: ['./email-notification.component.scss']
})
export class EmailNotificationComponent implements OnDestroy {

  @Input() emailNotifications: EmailNotificationModel[];
  @Input() organizationId: number;
  @Input() editable: boolean;

  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  _currentUser: any;
  principalDataSource: Array<DropDownListItemModel<number>> = [];
  filteredPrincipalDataSource: Array<DropDownListItemModel<number>> = [];

  countryDataSource: any = [];
  filteredCountryDataSource: any = [];

  locationDataSource: any = [];

  backupEditingItem: EmailNotificationModel;

  faPencilAlt = faPencilAlt;
  faBan = faBan;

  public defaultCountryItem: { label: string, value: number } =
    {
      label: 'label.any',
      value: null
    };

  isAddMode: boolean = false;
  isEditMode: boolean = false;

  private _subscriptions: Array<Subscription> = [];

  constructor(public _userContext: UserContextService,
    public _service: EmailNotificationService,
    public _notification: NotificationPopup
  ) {    
    this._getCurrentUser$.pipe(
      tap(() => {
        this._fetchPrincipalDataSource()
      })
    ).subscribe();

    this._service.getCountries().subscribe((data) => {
      if (data) {
        this.countryDataSource = data;
        this.filteredCountryDataSource = data;
      }
    });

    this._service.getAllLocations()
      .pipe(
        map((data: any) => {
          data.forEach(item => {
            const des = item.description.indexOf('-') >= 0 ? item.description.split('-') : [];
            if (des.length >= 2) {
              item.countryId = des[0];
              item.locationiId = des[1];
            } else {
              item.countryId = null;
              item.locationiId = null;
            }
          });
          return data;
        })
      )
      .pipe(
        tap((data) => {
          this.locationDataSource = data;
          this.bindingPortLocation();
        })
      ).subscribe();
  }

  isPortAgentSelected(description, rowIndex): boolean {
    return this.emailNotifications[rowIndex].portSelectionIds.some(item => item === description);
  }

  private _fetchPrincipalDataSource() {
    const roleId = this._currentUser.role ? this._currentUser.role.id : 0;
    const organizationId = this._currentUser.organizationId;
    const affiliates = this._currentUser.affiliates;
    const sub = this._service
      .getPrincipalDataSource(roleId, organizationId, affiliates)
      .pipe(
        tap((data: Array<DropDownListItemModel<number>>) => {
          this.principalDataSource = data;
          this.filteredPrincipalDataSource = data;
        })
      )
      .subscribe();
    this._subscriptions.push(sub);
  }

  private get _getCurrentUser$(): Observable<any> {
    if (this._currentUser) {
      return of(this._currentUser);
    } else {
      return this._userContext.getCurrentUser().pipe(
        tap((user: any) => {
          this._currentUser = user;
        })
      );
    }
  }

  bindingPortLocation() {
    this.emailNotifications.forEach(el => {
      if (!StringHelper.isNullOrEmpty(el.countryId)) {
        const countryId = el.countryId.toString();
        el.portLocations = this.locationDataSource
          .filter(s => s.locationiId && s.countryId === countryId);
      }
    });
  }

  addBlank(): void {
    if (!this.emailNotifications) {
      this.emailNotifications = [];
    }
    this.emailNotifications.push({
      id: 0,
      organizationId: this.organizationId,
      customerId: null,
      countryId: null,
      portSelectionIds: [],
      portLocations: [],
      adding: true,
      editing: false,
    });
    this.isAddMode = true;
  }

  onDelete(rowIndex): void {
    var item = this.emailNotifications[rowIndex];

    const confirmDlg = this._notification.showConfirmationDialog('delete.saveConfirmation', 'label.organization');
    confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          this._service.deleteEmailNotification(item.id).subscribe(
            data => {
              this._notification.showSuccessPopup('save.sucessNotification', 'label.organization');
              this.emailNotifications.splice(rowIndex, 1);
            },
            error => {
              this._notification.showErrorPopup('save.failureNotification', 'label.organization');
            });
        }
      });
  }

  onCancel(rowIndex): void {
    let item = this.emailNotifications[rowIndex];
    if (item.adding) {
      this.emailNotifications.splice(rowIndex, 1);
      this.isAddMode = false;
    }
    else if (item.editing) {
      this.isEditMode = false;
      // revert changes
      this.emailNotifications[rowIndex] = Object.assign({}, this.backupEditingItem);
    }
  }

  onEdit(rowIndex): void {
    let item = this.emailNotifications[rowIndex];
    // backup for cancellation.
    this.backupEditingItem = Object.assign({}, item);

    // mark item as editing
    item.editing = true;
    // mark form as in editing mode
    this.isEditMode = true;
  }

  onCustomerFilterChange(event, rowIndex): void {
    if (event.length >= 3) {
      this.filteredPrincipalDataSource = this.principalDataSource.filter(
        (s) => s.text.toLowerCase().indexOf(event.toLowerCase()) !== -1
      );
    } else {
      this.filteredPrincipalDataSource = this.principalDataSource;
    }
  }

  onCountryFilterChange(event, rowIndex): void {
    if (event.length >= 3) {
      this.filteredCountryDataSource = this.countryDataSource.filter(
        (s) => s.label.toLowerCase().indexOf(event.toLowerCase()) !== -1
      );
    } else {
      this.filteredCountryDataSource = this.countryDataSource;
    }
  }

  onCountryValueChange(event, rowIndex): void {
    var item = this.emailNotifications[rowIndex];
    const countryId = event?.toString();
    if (countryId) {
      item.portLocations = this.locationDataSource.filter((s) =>
        s.locationiId && s.countryId === countryId
      );
    } else {
      item.portLocations = [];
    }
    item.portSelectionIds = [];
  }

  onPortFilterChange(value, rowIndex) {
    var item = this.emailNotifications[rowIndex];
    if (value.length >= 3) {
      item.portLocations = this.locationDataSource
        .filter(s => s.locationiId && s.countryId === item.countryId.toString() &&
          s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    } else {
      item.portLocations = this.locationDataSource
        .filter(s => s.locationiId && s.countryId === item.countryId.toString());
    }
  }

  validateEmail(event, rowIndex): void {
    if (event.length <= 0) {
      return;
    }
    const isValid = StringHelper.validateEmailSeparateByComma(event);
    let control = this.getFormControl(`email_${rowIndex}`);
    if (isValid) {
      control.setErrors(null);
    } else {
      control.setErrors({ 'invalidEmail': true });
    }
  }

  onSave(dataItem: EmailNotificationModel, rowIndex: number): void {
    if (dataItem.adding) {
      this._service.createEmailNotification(dataItem).subscribe(
        (data: EmailNotificationModel) => {
          if (data) {
            data.adding = false;
            this.emailNotifications[rowIndex] = data;
            this.bindingPortLocation();
          }
          this.isAddMode = false;
          this._notification.showSuccessPopup('save.sucessNotification', 'label.organization');
        },
        (error) => {
          this._notification.showErrorPopup('save.failureNotification', 'label.organization');
        }
      );
    }
    else if (dataItem.editing) {
      this._service.updateEmailNotification(dataItem, dataItem.id).subscribe(
        (data: EmailNotificationModel) => {
          if (data) {
            data.editing = false;
            this.emailNotifications[rowIndex] = data;
            this.bindingPortLocation();
          }
          this.isEditMode = false;
          this._notification.showSuccessPopup('save.sucessNotification', 'label.organization');
        },
        (error) => {
          this._notification.showErrorPopup('save.failureNotification', 'label.organization');
        }
      );
    }
  }

  getFormControl(controlName: string): AbstractControl {
    return this.mainForm?.form?.controls[controlName];
  }

  ngOnDestroy(): void {
    this._subscriptions?.map(x => x.unsubscribe());
  }
}

export interface EmailNotificationModel {
  id: number;
  organizationId: number;
  customerId: number;
  countryId: number;
  portSelectionIds: number[];
  
  portLocations: any[];
  adding: boolean;
  editing: boolean;
}