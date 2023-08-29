import { Component, OnDestroy } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { faPencilAlt, faPlus, faPowerOff, faTrashAlt, faUnlockAlt } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import moment from 'moment';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, map, tap } from 'rxjs/operators';
import { ContractMasterStatus, DateHelper, FormComponent, OrganizationType, StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { OrganizationReferenceDataModel } from 'src/app/core/models/organization.model';
import { CommonService } from 'src/app/core/services/common.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ContractType } from '../models/contract-type.model';
import { ContractModel } from '../models/contract.model';
import { ContractFormService } from './contract-form.service';

@Component({
  selector: 'app-contract-form',
  templateUrl: './contract-form.component.html',
  styleUrls: ['./contract-form.component.scss']
})
export class ContractFormComponent extends FormComponent implements OnDestroy {
  modelName = 'contracts';
  contractMasterStatus = ContractMasterStatus;
  faPencilAlt = faPencilAlt;
  faPowerOff = faPowerOff;
  contractKey: string;
  carriersDataSource: CarrierModel[] = [];
  carriersFiltered: CarrierModel[] = [];
  contractTypeDataSource: ContractType[] = [];
  orgDataSource: OrganizationReferenceDataModel[];
  orgFiltered: OrganizationReferenceDataModel[];
  defaultDropdownItem: { id: number, name: string } = { id: null, name: 'label.select' };
  defaultContractTypeDropdownItem: { name: string } = { name: 'label.select' };
  originContractNo: string;

  contractNoTypingEvent$ = new Subject<string>();
  private _subscriptions: Array<Subscription> = [];

  constructor(
    protected route: ActivatedRoute,
    private contractFormService: ContractFormService,
    public notification: NotificationPopup,
    public translateService: TranslateService,
    public router: Router,
    private commonService: CommonService
  ) {
    super(route, contractFormService, notification, translateService, router);
  }

  onInitDataLoaded(): void {
    this.model.contractHolder = !this.model.contractHolder ? null : +this.model.contractHolder;
    this.contractKey = this.model.carrierContractNo;
    this.getDataSourcesForDropdown();
    this.handleInputEvent();
  }

  getDataSourcesForDropdown() {
    this.commonService.getCarriers().subscribe(response => {
      this.carriersDataSource = response;
      this.carriersFiltered = response;
    });

    this.commonService.getOrganizations()
      .pipe(
        map(c=> 
          { 
            const data = c.filter(s=>s.organizationTypeName === 'Principal' || s.organizationTypeName === 'Agent')
            return data;
          })
      )
      .subscribe(response => {
      this.orgDataSource = response;
      this.orgFiltered = response;
    });

    this.contractFormService.getAllContractTypes().subscribe(response => {
      this.contractTypeDataSource = response;
    });
  }

  handleInputEvent() {
    let sub = this.contractNoTypingEvent$.pipe(
      debounceTime(this.defaultDebounceTimeInput),
      tap((inputedContractNo: string) => {
        this.checkContractAlreadyExists(inputedContractNo);
      })).subscribe();
    this._subscriptions.push(sub);
  }

  checkContractAlreadyExists(inputedContractNo: string) {
    if (StringHelper.isNullOrEmpty(inputedContractNo)) {
      return;
    }

    if (this.contractKey?.toLowerCase() !== inputedContractNo.toLowerCase()) {
      const sub = this.contractFormService.checkAlreadyExists(inputedContractNo).subscribe(
        r => {
          if (r) {
            this.getFormControl('contractNo').setErrors({ isDuplicated: true });
            this.getFormControl('contractNo').markAsTouched();
          } else {
            this.getFormControl('contractNo').setErrors(null);
          }
        }
      )
      this._subscriptions.push(sub);
    }
  }

  onFilterCarrier(input) {
    this.carriersFiltered = this.carriersDataSource.filter(c => c.name.toLowerCase().indexOf(input.toLowerCase()) !== -1);
  }

  onFilterOrg(input) {
    this.orgFiltered = this.orgDataSource.filter(c => c.name.toLowerCase().indexOf(input.toLowerCase()) !== -1);
  }

  onChangeContracType(value: string) {
    if (value === this.defaultContractTypeDropdownItem.name) {
      this.getFormControl('contractType').setErrors({ required: true });
      this.getFormControl('contractType').markAsTouched();
    } else {
      this.getFormControl('contractType').setErrors(null);
    }
  }

  onChangeValidFromDate() {
    this.validateValidToDate();
  }

  onChangeValidToDate() {
    this.validateValidToDate();
  }

  validateValidToDate() {
    if (!this.model.validToDate) {
      return;
    }

    if (this.model.validToDate < this.model.validFromDate) {
      this.getFormControl('validToDate').setErrors({ notEarlierThan: true });
      this.getFormControl('validToDate').markAsTouched();
    } else {
      this.getFormControl('validToDate').setErrors(null);
    }
  }

  save() {
    FormHelper.ValidateAllFields(this.currentForm);
    if (!this.mainForm.valid) {
      return;
    }

    const model = DateHelper.formatDate(this.model);
    if (this.isAddMode) {
      this.contractFormService.createNew(model).subscribe(
        r => {
          this.notification.showSuccessPopup('save.sucessNotification', 'label.contract');
          this.backToList();
        },
        err => {
          this.notification.showErrorPopup('save.failureNotification', 'label.contract');
        },
      );
    } else {
      this.contractFormService.updateContract(model).subscribe(
        r => {
          this.notification.showSuccessPopup('save.sucessNotification', 'label.contract');
          this.router.navigate([`contracts/view/${this.model.id}`]);
          this.ngOnInit();
        },
        err => {
          this.notification.showErrorPopup('save.failureNotification', 'label.contract');
        }
      );
    }
  }

  onUpdateStatus(model: ContractModel, status: ContractMasterStatus) {
    var data = { ...model };
    data.status = status;

    if (status === ContractMasterStatus.Active) {
      if (moment(data.validToDate).startOf('day').toDate() < moment().startOf('day').toDate()) {
        this.notification.showInfoDialog('validation.contractActivation', 'label.contract');
        return
      }
    }

    const sub = this.contractFormService.updateStatus(model.id, data).subscribe(
      r => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.contract');
        this.router.navigate([`contracts/view/${this.model.id}`]);
        this.ngOnInit();
      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.contract');
      }
    );
    this._subscriptions.push(sub);
  }

  cancel() {
    if (this.isAddMode) {
      this.backToList();
    } else {
      this.router.navigate([`contracts/view/${this.model.id}`]);
      this.ngOnInit();
    }
  }

  backToList() {
    this.router.navigate(['/contracts']);
  }

  editContract() {
    this.router.navigate([`contracts/edit/${this.model.id}`])
  }

  getFormControl(controlName: string): AbstractControl {
    return this.mainForm?.form?.controls[controlName];
  }

  ngOnDestroy(): void {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
