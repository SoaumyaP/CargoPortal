import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { DateHelper, DATE_FORMAT, DropDowns, FormComponent, ModeOfTransportType, StringHelper } from 'src/app/core';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { BillOfLadingFormService } from '../bill-of-lading-form.service';

/**
 * Component to edit House BOL details
 */
@Component({
  selector: 'app-bill-of-lading-form',
  templateUrl: './bill-of-lading-form.component.html',
  styleUrls: ['./bill-of-lading-form.component.scss']
})
export class BillOfLadingFormComponent extends FormComponent implements OnDestroy {
  modelName = 'houseBL';
  DATE_FORMAT = DATE_FORMAT;
  billOfLadingTypes: Array<any> = DropDowns.BillOfLadingType;
  houseBLNo: string
  checkHouseBLEvent$ = new Subject<string>();
  subscriptions = new Subscription();
  modeOfTransport = ModeOfTransportType;
  StringHelper = StringHelper;

  constructor(
    protected route: ActivatedRoute,
    public notification: NotificationPopup,
    public router: Router,
    public billOfLadingFormService: BillOfLadingFormService,
    public translateService: TranslateService,
    private _gaService: GoogleAnalyticsService) {
    super(route, billOfLadingFormService, notification, translateService, router);

  }

  validationRules = {
    billOfLadingNo: {
      required: 'label.houseBLNo'
    },
    billOfLadingType: {
      required: 'label.billOfLadingType'
    },
    issueDate: {
      required: 'label.issueDates'
    }
  };

  onInitDataLoaded() {
    if (this.model !== null) {
      if (new Date(this.model.issueDate) <= new Date(1900, 1, 1)) {
        this.model.issueDate = null;
      }
    }
    this.setDefaultValueHouseBL();
    this.handleCheckHouseBLEvent();
  }

  handleCheckHouseBLEvent() {
    const sub = this.checkHouseBLEvent$.pipe(
      debounceTime(1000),
      tap((houseBLNo: string) => {
        this.onCheckHouseBLNoAlreadyExists(houseBLNo);
      }
      )).subscribe();
    this.subscriptions.add(sub);
  }

  onCheckHouseBLNoAlreadyExists(housBLNo: string) {
    if (housBLNo?.length >= 3 && housBLNo !== this.houseBLNo) {
      this.billOfLadingFormService.checkHouseBLAlreadyExists(
        housBLNo
      ).subscribe((c: any) => {
        if (c) {
          this.getControl('billOfLadingNo').setErrors({ isDuplicated: true });
        } else {
          this.getControl('billOfLadingNo').setErrors(null);
        }
      });
    }
  }

  setDefaultValueHouseBL() {
    this.houseBLNo = this.model.billOfLadingNo;
    const isValidBillOfLadingType = DropDowns.BillOfLadingType.some(c => c.value === this.model.billOfLadingType);
    isValidBillOfLadingType ? this.model.billOfLadingType : this.model.billOfLadingType = '';
  }

  save() {
    if (!this.mainForm.valid) {
      return;
    }

    this.setValueHouseBLBeforeSave();
    const sub = this.billOfLadingFormService.updateHouseBL(this.model.id, this.model).subscribe(
      r => {
        this._gaService.emitAction('Edit', GAEventCategory.HouseBill);
        this.notification.showSuccessPopup('save.sucessNotification', 'label.houseBillOfLading');
        this.router.navigate([`/bill-of-ladings/${this.modelId}`]);
      },
      error => {
        this.notification.showErrorPopup('save.failureNotification', 'label.houseBillOfLading')
      }
    );
    this.subscriptions.add(sub);
  }

  setValueHouseBLBeforeSave() {
    this.model.contacts = null;
    this.model = DateHelper.formatDate(this.model);
  }

  cancel() {
    const confirmDlg = this.notification.showConfirmationDialog(
      'edit.cancelConfirmation',
      'label.houseBL'
    );

    const sub = confirmDlg.result.subscribe((result: any) => {
      if (result.value) {
        this.router.navigate([`/bill-of-ladings/${this.modelId}`]);
      }
    });

    this.subscriptions.add(sub);
  }

  backToList() {
    this.router.navigate(['/bill-of-ladings']);
  }

  getControl(controlName: string): AbstractControl {
    return this.mainForm?.form?.controls[controlName];
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
