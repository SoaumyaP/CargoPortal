import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { PanelBarComponent } from '@progress/kendo-angular-layout';
import { Subscription } from 'rxjs';
import { DateHelper } from 'src/app/core';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { POProgressCheckFilterFormComponent } from '../po-progress-check-filter-form/po-progress-check-filter-form.component';
import { POProgressCheckFilterFormService } from '../po-progress-check-filter-form/po-progress-check-filter-form.service';
import { POProgressCheckListFormComponent } from '../po-progress-check-list-form/po-progress-check-list-form.component';
import { POProgressCheckFilterModel, POProgressCheckModel, POProgressCheckQueryModel } from '../po-progress-check.model';
import { POProgressCheckFormService } from './po-progress-check-form.service';

@Component({
  selector: 'app-po-progress-check-form',
  templateUrl: './po-progress-check-form.component.html',
  styleUrls: ['./po-progress-check-form.component.scss']
})
export class POProgressCheckFormComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('filterProgressCheck', { read: ElementRef, static: false }) elementView: ElementRef;
  @ViewChild(POProgressCheckListFormComponent, { static: false }) poProgressCheckListFormComponent: POProgressCheckListFormComponent;
  @ViewChild(POProgressCheckFilterFormComponent, { static: false }) poProgressCheckFilterFormComponent: POProgressCheckFilterFormComponent;

  _subscriptions: Array<Subscription> = [];
  poProgressCheckList: POProgressCheckModel[] = [];
  isInitDataLoaded: boolean = true;
  isInSearchingProgress: boolean = false;
  isSavePOProgressCheck = false;
  heightToReduceOfPOGrid: string = '';
  totalHeightOfOtherElements: number = 245;

  constructor(
    private _service: POProgressCheckFormService,
    private notification: NotificationPopup,
    private router: Router,
    private poProgressCheckFilterFormService: POProgressCheckFilterFormService,
    private cd: ChangeDetectorRef,
    private _gaService: GoogleAnalyticsService
  ) { }

  ngOnInit() {
    const sub = this.poProgressCheckFilterFormService.resetFilterEvent$.subscribe(
      r => {
        this.poProgressCheckList = [];
      }
    )
    this._subscriptions.push(sub);
  }

  ngAfterViewInit(): void {
    this.heightToReduceOfPOGrid = `${this.totalHeightOfOtherElements + this.elementView.nativeElement.offsetHeight}px`;
    this.cd.detectChanges();
  }

  searchPO(data: POProgressCheckFilterModel,isNavigateFromEmail:boolean) {
    this.isInSearchingProgress = true;
    let model: POProgressCheckQueryModel = new POProgressCheckQueryModel();
    model.SelectedCustomerId = data.selectedCustomerId;
    model.SelectedSupplierId = data.selectedSupplierId;
    model.PONoFrom = data.poNoFrom;
    model.PONoTo = data.poNoTo;
    if (data.cargoReadyDateFrom) {
      model.CargoReadyDateFrom = model.convertToQueryDateString(data.cargoReadyDateFrom);
    }
    if (data.cargoReadyDateTo) {
      model.CargoReadyDateTo = model.convertToQueryDateString(data.cargoReadyDateTo);
    }

    this._service.searchPOProgressCheck(model.buildToQueryParams).subscribe(
      data => {
        this.poProgressCheckList = data;
        this.isInSearchingProgress = false;
      }
    )
  }

  searchFromEmail(data:any){
    this.isInSearchingProgress = true;
    this._service.searchPOProgressCheckFromEmail(data.buyerComplianceId, data.poIds).subscribe(
      data => {
        this.poProgressCheckList = data;
        this.isInSearchingProgress = false;
      },
      err => {
        this.isInSearchingProgress = false;
        this.notification.showErrorPopup('label.titleInvalidSearch', 'label.progressCheck');
      }
    )
  }

  savePOProgressCheck() {
    this.poProgressCheckFilterFormComponent.validateFormControls();
    if (this.poProgressCheckList.length === 0 || this.isSavePOProgressCheck || this.poProgressCheckListFormComponent.isInvalidPOList) {
      return;  
    }

    this.isSavePOProgressCheck = true;
    const models = this.poProgressCheckList.map(c => DateHelper.formatDate(c));
    const sub = this._service.savePOProgressCheck(models).subscribe(
      r => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.progressCheck');
        this.router.navigate(['/purchase-orders']);
        this.isSavePOProgressCheck = false;
        this._gaService.emitAction('Progress Check', GAEventCategory.PurchaseOrder);

      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.progressCheck');
        this.isSavePOProgressCheck = false;
      }
    )
    this._subscriptions.push(sub);
  }

  cancelPOProgressCheck() {
    this.router.navigate(['/purchase-orders'])
  }

  backToPoList() {
    this.router.navigate(['/purchase-orders']);
  }

  onPanelChange() {
    // Need set timeout is 250ms to get Expantion's height exactly, because animation of expand or collapse approximately ~250ms
    setTimeout(() => {
      this.heightToReduceOfPOGrid = `${this.totalHeightOfOtherElements + this.elementView.nativeElement.offsetHeight}px`;
    }, 250);
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
