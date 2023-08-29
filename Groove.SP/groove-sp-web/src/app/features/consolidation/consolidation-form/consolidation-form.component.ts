import { Component, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { ConsolidationFormService, InputConsolidationModel, UpdateConsolidationModel } from './consolidation-form.service';
import { Observable, of, Subscription } from 'rxjs';
import { ContainerHelper } from 'src/app/core/helpers/container-helper';
import { DateHelper, DATE_FORMAT } from 'src/app/core/helpers/date.helper';
import { ConsolidationStage, DropDowns, FormComponent } from 'src/app/core';
import { StringHelper } from 'src/app/core/helpers/string.helper';
import { ConsolidationModel } from '../models/consolidation.model';
import { ConsolidationService } from '../consolidation.service';
import { map } from 'rxjs/operators';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@Component({
  selector: 'app-consolidation-form',
  templateUrl: './consolidation-form.component.html',
  styleUrls: ['./consolidation-form.component.scss']
})
export class ConsolidationFormComponent extends FormComponent implements OnDestroy {

  modelName = 'consolidation';
  DATE_FORMAT = DATE_FORMAT;

  // for add mode
  model: ConsolidationModel = new ConsolidationModel();

  validationRules = {
    originCFSControl: {
      required: 'label.originCFS'
    },
    cfsCutoffDateControl: {
      required: 'label.cfsCutoffDates'
    },
    equipmentTypeControl: {
      required: 'label.equipmentType'
    },
    modeOfTransportControl: {
      required: 'label.modeOfTransport'
    },
    containerNumberControl: {
      containerNumberInvalid: 'validation.containerNumberInvalid'
    },
    sealNumberControl: {},
    carrierSONumberControl: {
      duplicatedWithContainerNumber: 'validation.duplicatedOnCarrierSONoAndContainerNo'
    }
  };

  equipmentTypeOptions = DropDowns.ContainerType;
  organizationOptions: any[];
  allLocationOptions: any;
  filteredLocations: any[];

  /**Set as container info will be optional for saving (containerNumber, sealNumber, carrierSONumber) */
  isEditAsDraft: boolean = true;

  private _subscriptions: Array<Subscription> = [];

  constructor(protected route: ActivatedRoute,
    public notification: NotificationPopup,
    public router: Router,
    public service: ConsolidationFormService,
    public consolidationService: ConsolidationService,
    public translateService: TranslateService,
    private _gaService: GoogleAnalyticsService) {
    super(route, service, notification, translateService, router);
    if (!consolidationService.currentUser.isInternal) {
      service.affiliateCodes = consolidationService.currentUser.affiliates;
    }
    const sub = this.service.getAllLocations().subscribe(data => {
      this.allLocationOptions = data;
    });
    this._subscriptions.push(sub);

    const qParamSub = this.route.queryParams.subscribe(params => {
      const consignmentId = params['selectedconsignment'];
      this.model.consignmentId = Number.parseInt(!consignmentId ? 0 : consignmentId, 10);

      if (params['state'] && params['state'] === 'confirmfailed') {
        this.isEditAsDraft = false;
      }
    });
    this._subscriptions.push(qParamSub);
  }

  onInitDataLoaded() {
    if (this.isAddMode) {
      if (!this.model.consignmentId) {
        this.router.navigate(['/error/404']);
      }
      this.service.getDefaultOriginCFS(this.model.consignmentId).subscribe(
        rsp => this.model.originCFS = rsp.text
      )
    } else {
      if (this.model.stage !== ConsolidationStage.New) {
        this.router.navigate(['/error/404']);
      }
      if (!this.isEditAsDraft) {
        // Container info are required for confirm consolidation.
        this.validationRules.sealNumberControl['requiredToConfirm'] = 'validation.sealNoIsMandatoryToConfirm';
        this.validationRules.containerNumberControl['requiredToConfirm'] = 'validation.containerNoIsMandatoryToConfirm';
        this.validationRules.carrierSONumberControl['requiredToConfirm'] = 'validation.carrierSONoIsMandatoryToConfirm';
      }
    }
    this.checkDuplicateContainer().subscribe();
  }

  onValueChanged(data?: any) {
    // set validation by default immediately after the system is redirected.
    if (!this.isEditAsDraft) {
      if (StringHelper.isNullOrEmpty(this.model.sealNo) && this.mainForm?.controls['sealNumberControl'] && !this.formErrors['sealNumberControl']) {
        this.setInvalidControl('sealNumberControl', 'requiredToConfirm');
        this.mainForm.controls['sealNumberControl'].markAsPristine();
        this.mainForm.controls['sealNumberControl'].markAsDirty();
      }
      if (StringHelper.isNullOrEmpty(this.model.containerNo) && this.mainForm?.controls['containerNumberControl'] && !this.formErrors['containerNumberControl']) {
        this.setInvalidControl('containerNumberControl', 'requiredToConfirm');
        this.mainForm.controls['containerNumberControl'].markAsPristine();
        this.mainForm.controls['containerNumberControl'].markAsDirty();
      }
      if (this.mainForm?.controls['carrierSONumberControl'] && !this.formErrors['carrierSONumberControl']) {
        if (StringHelper.isNullOrEmpty(this.model.carrierSONo)) {
          this.setInvalidControl('carrierSONumberControl', 'requiredToConfirm');
          this.mainForm.controls['carrierSONumberControl'].markAsPristine();
          this.mainForm.controls['carrierSONumberControl'].markAsDirty();
        }
      }
    } else {
      super.onValueChanged(data);
    }
  }

  validateSealNumber() {
    if (this.isEditAsDraft) {
      return;
    }

    if (StringHelper.isNullOrEmpty(this.model.sealNo)) {
      this.setInvalidControl('sealNumberControl', 'requiredToConfirm')
    } else {
      this.setValidControl('sealNumberControl');
    }
  }

  validateContainerNumber() {
    if (this.isEditAsDraft) {
      return;
    }

    if (StringHelper.isNullOrEmpty(this.model.containerNo)) {
      this.setInvalidControl('containerNumberControl', 'requiredToConfirm')
    } else {
      this.setValidControl('containerNumberControl');
    }
  }

  validateCarrierSONumber() {
    if (StringHelper.isNullOrEmpty(this.model.carrierSONo)) {
      if (!this.isEditAsDraft) {
        this.setInvalidControl('carrierSONumberControl', 'requiredToConfirm');
      }
    } else {
      this.setValidControl('carrierSONumberControl');
    }
  }

  originCFSFilterChange(value) {
    this.filteredLocations = [];
    if (value.length >= 3) {
      this.filteredLocations = this.allLocationOptions.filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }
  }

  private checkDuplicateContainer(): Observable<boolean> {
    if (!StringHelper.isNullOrEmpty(this.model.carrierSONo) && !StringHelper.isNullOrEmpty(this.model.containerNo)) {
      return this.service.isDuplicatedContainer(this.model.containerId, this.model.containerNo, this.model.carrierSONo).pipe(
        map(x => {
          if (x) {
            this.setInvalidControl('carrierSONumberControl', 'duplicatedWithContainerNumber');
          } else {
            this.setValidControl('carrierSONumberControl');
          }
          return x;
        }));
    } else {
      return of(false);
    }
  }

  onContainerNumberFocusout() {
    this.checkDuplicateContainer().subscribe();
    this.checkInvalidContainerNumber();
  }

  checkInvalidContainerNumber() {
    if (StringHelper.isNullOrEmpty(this.model.containerNo)) {
      return;
    }
    if (this.model.containerNo.length !== 11) {
      this.setInvalidControl('containerNumberControl', 'containerNumberInvalid');
      return;
    }
    if (!ContainerHelper.checkDigitContainer(this.model.containerNo)) {
      this.setInvalidControl('containerNumberControl', 'containerNumberInvalid');
      return;
    }
    this.setValidControl('containerNumberControl');
  }

  backToList() {
    this.router.navigate(['/consolidations']);
  }

  saveConsolidation() {
    // Validate before saving
    this.checkInvalidContainerNumber();
    this.checkDuplicateContainer().subscribe(
      (res: boolean) => {
        if (!res) {
          if (!this.mainForm.valid) {
            return;
          }
          if (this.isEditMode) {
            const updatingModel = new UpdateConsolidationModel(
              this.modelId,
              this.model.originCFS,
              this.model.cfsCutoffDate,
              this.model.equipmentType,
              this.model.carrierSONo,
              this.model.containerNo,
              this.model.sealNo,
              this.model.sealNo2,
              this.model.loadingDate
            );
            const submitModel = DateHelper.formatDate(updatingModel);
            this.service.update(this.modelId, submitModel).subscribe(
              data => {
                this._gaService.emitAction('Edit', GAEventCategory.Consolidation);
                this.notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
                this.router.navigate([`/consolidations/${this.model.id}`]);
              },
              error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.consolidation');
              }
            );
          } else if (this.isAddMode) {
            const addedModel = new InputConsolidationModel(
              this.model.consignmentId,
              this.model.originCFS,
              this.model.cfsCutoffDate,
              this.model.equipmentType,
              this.model.carrierSONo,
              this.model.containerNo,
              this.model.sealNo,
              this.model.sealNo2,
              this.model.loadingDate
            );
            const submitModel = DateHelper.formatDate(addedModel);
            this.service.create(submitModel).subscribe(
              (data: any) => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
                this.router.navigate([`/consolidations/${data.id}`]);
                this._gaService.emitAction('Add', GAEventCategory.Consolidation);
              },
              error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.consolidation');
              }
            );
          }
        }
      }
    );
  }

  cancelEditingConsolidation() {
    const confirmDlg = this.notification.showConfirmationDialog(
      'edit.cancelConfirmation',
      'label.consolidation'
    );

    confirmDlg.result.subscribe((result: any) => {
      if (result.value) {
        if (this.isAddMode) {
          this.router.navigate(["/consolidations"]);
        } else {
          this.router.navigate([`/consolidations/${this.modelId}`]);
        }
      }
    });
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
