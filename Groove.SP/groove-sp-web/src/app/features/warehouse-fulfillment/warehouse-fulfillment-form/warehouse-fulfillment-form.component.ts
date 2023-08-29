import {
  Component,
  ElementRef,
  HostListener,
  ViewChild
} from '@angular/core';
import {
  ActivatedRoute,
  Router
} from '@angular/router';
import {
  faBan,
  faCaretLeft,
  faCheck,
  faPencilAlt,
  faClipboardCheck
} from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import moment from 'moment';
import {
  BuyerApprovalStage,
  DateHelper,
  FormComponent,
  OrganizationNameRole,
  POFulfillmentStageType,
  POFulfillmentStatus,
  Roles,
  RoleSequence,
  StringHelper
} from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { UserOrganizationProfileModel } from 'src/app/core/models/organization.model';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { WarehouseFulfillmentTabModel } from '../models/warehouse-fulfillment-tab.model';
import { WarehouseFulfillmentModel } from '../models/warehouse-fulfillment.model';
import { WarehouseFulfillmentActivityComponent } from '../warehouse-fulfillment-activity/warehouse-fulfillment-activity.component';
import { WarehouseFulfillmentGeneralInfoComponent } from '../warehouse-fulfillment-general-info/warehouse-fulfillment-general-info.component';
import { WarehouseFulfillmentFormService } from './warehouse-fulfillment-form.service';

@Component({
  selector: 'app-warehouse-fulfillment-form',
  templateUrl: './warehouse-fulfillment-form.component.html',
  styleUrls: ['./warehouse-fulfillment-form.component.scss']
})
export class WarehouseFulfillmentFormComponent extends FormComponent {
  modelName = "warehouseFulfillments";

  @ViewChild('headerBar', { static: false }) headerBarElement: ElementRef;
  @ViewChild('stickyBar', { static: false }) stickyBarElement: ElementRef;
  @ViewChild('sectionContainer', { static: false }) sectionContainerElement: ElementRef;
  @ViewChild('general', { static: false }) generalElement: ElementRef;
  @ViewChild('warehouse', { static: false }) warehouseElement: ElementRef;
  @ViewChild('contact', { static: false }) contactElement: ElementRef;
  @ViewChild('customerPO', { static: false }) customerPOElement: ElementRef;
  @ViewChild('activity', { static: false }) activityElement: ElementRef;
  @ViewChild('attachment', { static: false }) attachmentElement: ElementRef;

  @ViewChild(WarehouseFulfillmentGeneralInfoComponent, { static: false }) generalInfoComponent: WarehouseFulfillmentGeneralInfoComponent;
  @ViewChild(WarehouseFulfillmentActivityComponent, { static: false }) activityComponent: WarehouseFulfillmentActivityComponent;

  tabs: Array<WarehouseFulfillmentTabModel> = this.service.createNavigation(true);

  model: WarehouseFulfillmentModel = new WarehouseFulfillmentModel();

  readonly POFulfillmentStatus = POFulfillmentStatus;
  readonly POFulfillmentStageType = POFulfillmentStageType;

  faPencilAlt = faPencilAlt;
  faCaretLeft = faCaretLeft;
  faBan = faBan;
  faCheck = faCheck;
  faClipboardCheck = faClipboardCheck;

  saveAsDraft: boolean = true;
  isCargoReceiveMode: boolean = false;

  private isManualScroll: boolean = true;
  isReloadMode: boolean = false;
  cancelReason = "";
  cancelWarehouseFulfillmentDialog: boolean = false;
  stringHelper = StringHelper;
  isCancelling: boolean = false;
  /**Owner's Organization info of the booking */
  createdByOrganization: UserOrganizationProfileModel;

  isSubmitting: boolean = false;
  readonly AppPermissions = AppPermissions;

  constructor(
    protected route: ActivatedRoute,
    public service: WarehouseFulfillmentFormService,
    public notification: NotificationPopup,
    public translateService: TranslateService,
    public router: Router,
    private _gaService: GoogleAnalyticsService
  ) {
    super(route, service, notification, translateService, router);

  }

  onInitDataLoaded() {
    if (!this.isAddMode) {
      this.model = this.convertToModel(this.model);
    }

    // init default value
    this.formErrors = {};
    this.isCargoReceiveMode = false;

    this.tabs = this.service.createNavigation(false, this.isAddMode, this.isEditMode, this.model.stage);
    setTimeout(() => {
      this.initTabLink();
    }, 500); // make sure UI has been rendered

    this.service.getOwnerOrgInfo(this.model.createdBy).subscribe(
      response => this.createdByOrganization = response
    )
  }

  private convertToModel({ ...data }): WarehouseFulfillmentModel {
    let model = new WarehouseFulfillmentModel(data); // convert to data model
    // map data after convert
    model.createdBy = data.createdBy;
    model.createdDate = data.createdDate;
    return model;
  }

  /**Link UI element to tabs object
    Must make sure that it is correct order */
  private initTabLink(): void {
    this.tabs.map(tab => {
      switch (tab.sectionId) {
        case 'general':
          tab.sectionElementRef = this.generalElement;
          break;
          case 'warehouse':
          tab.sectionElementRef = this.warehouseElement;
          break;
        case 'contact':
          tab.sectionElementRef = this.contactElement;
          break;
        case 'customerPO':
          tab.sectionElementRef = this.customerPOElement;
          break;
        case 'attachment':
          tab.sectionElementRef = this.attachmentElement;
          break;
        case 'activity':
          tab.sectionElementRef = this.activityElement;
          break;
      }
    });
  }

  onClickStickyBar(event, tab: WarehouseFulfillmentTabModel) {
    this.isManualScroll = false;
    for (let i = 0; i < this.tabs.length; i++) {
      const element = this.tabs[i];
      if (element.sectionId === tab.sectionId) {
        element.selected = true;
      } else {
        element.selected = false;
      }
    }
    // If the first section, move to the top
    if (tab.text === 'label.general') {
      window.scrollTo({ behavior: 'smooth', top: 0 });
    } else {
      const headerHeight = this.headerBarElement?.nativeElement?.clientHeight;
      window.scrollTo({ behavior: 'smooth', top: tab.sectionElementRef?.nativeElement?.offsetTop - headerHeight - 36 });
    }

    // After 1s, reset isManualScroll = true -> it scrolls to target position
    setTimeout(() => {
      this.isManualScroll = true;
    }, 1000);
  }

  @HostListener('window:scroll', ['$event'])
  onScroll(event) {

    const currentYPosition = window.scrollY;
    const headerHeight = this.headerBarElement?.nativeElement?.clientHeight;

    // Make header sticky
    if (currentYPosition >= headerHeight - 30) {
      this.headerBarElement?.nativeElement?.style.setProperty('position', 'sticky');
      this.headerBarElement?.nativeElement?.style.setProperty('top', '60px');
    } else {
      this.headerBarElement?.nativeElement?.style.setProperty('position', 'relative');
      this.headerBarElement?.nativeElement?.style.removeProperty('top');
    }

    // Make sticky bar

    if (currentYPosition >= headerHeight + 30) {
      this.stickyBarElement?.nativeElement?.style.setProperty('position', 'sticky');
      this.stickyBarElement?.nativeElement?.style.setProperty('top', headerHeight + 60 + 'px');
      this.stickyBarElement?.nativeElement?.style.removeProperty('display');
    } else {
      this.stickyBarElement?.nativeElement?.style.setProperty('display', 'none');
    }

    //#region Auto update sticky bar status

    // If user clicks on sticky menu, do not update status
    if (!this.isManualScroll) {
      return;
    }

    this.tabs.forEach(c => {
      c.selected = false;
    });

    for (let i = 0; i < this.tabs.length; i++) {
      const element = this.tabs[i];
      // adding 240px to make update sticky bar earlier
      if (currentYPosition + headerHeight + 40 <= element.sectionElementRef?.nativeElement?.offsetTop + element.sectionElementRef?.nativeElement?.clientHeight) {
        element.selected = true;
        break;
      }
    }
    //#endregion
  }

  onSubmit() {
    if (this.isCargoReceiveMode) {
      let isValid: boolean = true;
      if (!this.mainForm.valid) {
        isValid = false;
        this.validateAllFields(false);
      }

      if (!this.validateBookingBeforeSaving()) {
        isValid = false;
      }

      if (!isValid) {
        return;
      }

      // In case there is any error but not belonging to any tab
      const errors = Object.keys(this.formErrors);
      errors.map((key) => {
        const err = Reflect.get(this.formErrors, key);
        if (err && !StringHelper.isNullOrEmpty(err)) {
          return;
        }
      });

      let tempModel: any = new WarehouseFulfillmentModel({ ...this.model });
      tempModel.orders = tempModel.orders?.map(
        order => DateHelper.formatDate(order)) || [];

      this.isSubmitting = true;
      this.service.cargoReceive(this.modelId, tempModel.orders).subscribe(
        res => {
            this._gaService.emitAction('Cargo Receive', GAEventCategory.WarehouseBooking);
            this.notification.showSuccessPopup(
                "save.sucessNotification",
                "label.poFulfillment"
            );
            this.router.navigate([
                `/warehouse-bookings/view/${this.modelId}`
            ]);
            this.isSubmitting = false;
            this.ngOnInit();
            this.activityComponent?.loadActivity();
        },
        err => {
          this.isSubmitting = false;
          this.notification.showErrorPopup(
            "save.failureNotification",
            "label.poFulfillment"
          );
        }
      )
    } else if (this.isEditMode) {
      let isValid: boolean = true;
      if (!this.mainForm.valid) {
        isValid = false;
        this.validateAllFields(false);
      }

      if (!this.validateBookingBeforeSaving()) {
        isValid = false;
      }

      if (!isValid) {
        return;
      }

      // In case there is any error but not belonging to any tab
      const errors = Object.keys(this.formErrors);
      errors.map((key) => {
        const err = Reflect.get(this.formErrors, key);
        if (err && !StringHelper.isNullOrEmpty(err)) {
          return;
        }
      });

      let tempModel: any = new WarehouseFulfillmentModel({ ...this.model });
      tempModel.contacts = tempModel.contacts?.map(
        load => DateHelper.formatDate(load)) || [];

      this.isSubmitting = true;
      this.service.update(this.modelId, tempModel).subscribe(
        res => {
            this._gaService.emitAction('Edit', GAEventCategory.WarehouseBooking);
            this.notification.showSuccessPopup(
                "save.sucessNotification",
                "label.poFulfillment"
            );
            this.router.navigate([
                `/warehouse-bookings/view/${this.modelId}`
            ]);
            this.isSubmitting = false;
            this.ngOnInit();
        },
        err => {
          this.isSubmitting = false;
          this.notification.showErrorPopup(
            "save.failureNotification",
            "label.poFulfillment"
          );
        }
      )
    }
  }

  onCancel() {
    const confirmDlg = this.notification.showConfirmationDialog(
      "edit.cancelConfirmation",
      "label.poFulfillment"
    );

    confirmDlg.result.subscribe((result: any) => {
      if (result.value) {
        if (this.isEditMode) {
          this.router.navigate([
            `/warehouse-bookings/view/${this.model.id}`
          ]);
          this.ngOnInit();
        }
      }
    });
  }

  onCargoReceiveBtnClick(e) {
    this.isCargoReceiveMode = true;
    this.formErrors = {};
    this.router.navigate([
      `/warehouse-bookings/edit/${this.model.id}`
    ]);
  }

  private validateBookingBeforeSaving(): boolean {
    let isValid: boolean = true;
    let firstErrorSectionId = '';

    // Validate general
    let validationResult = this.generalInfoComponent.validateBeforeSaving();
    if (validationResult.length > 0) {
      firstErrorSectionId = 'general';
      isValid = false;
    }

    if (!isValid) {
      const firstTabIndex = this.tabs.findIndex(x => x.sectionId === firstErrorSectionId);
      this.onClickStickyBar(null, this.tabs[firstTabIndex]);
    }
    return isValid;
  }

  get isContactTabEditable() {
    if (this.model.stage > POFulfillmentStageType.Draft) {
      return false;
    }

    if (this.isViewMode) {
      return false;
    }

    return true;
  }

  isPendingStatus(data?: any): boolean {
    let poff = data;
    if (StringHelper.isNullOrEmpty(poff)) {
      // Get from component model data
      poff = this.model;
    }
    return (
      poff.buyerApprovals &&
      poff.buyerApprovals.length > 0 &&
      poff.buyerApprovals.find(
        x => x.stage === BuyerApprovalStage.Pending
      ) &&
      poff.status !== POFulfillmentStatus.Inactive && // cancel
      poff.stage === POFulfillmentStageType.ForwarderBookingRequest
    );
  }

  get isRejectedStatus(): boolean {
    return (
      this.model.isRejected &&
      this.model.status !== POFulfillmentStatus.Inactive
    );
  }

  /**
     * Get tab details/ settings
     * @param sectionId Id of section
     * @returns
     */
  getTabDetails(sectionId: string): WarehouseFulfillmentTabModel {
    const result = this.tabs.find(x => x.sectionId === sectionId);
    return result;
  }

  /**
   * Get customer organization id of the booking.
   * */
  get customerId(): string {
    const principalContact = this.model?.contacts?.find(
      (c) => c.organizationRole === OrganizationNameRole.Principal
    );
    return principalContact?.organizationId || '';
  }

  /**
   * Get supplier company name of the booking.
   * */
  get supplierName(): string {
    const supplier = this.model?.contacts?.find(
      (c) => c.organizationRole === OrganizationNameRole.Supplier
    );
    return supplier?.companyName || '';
  }

  private orgRoleSequenceMapping(role: string): number {
    let result: number = 0;
    switch (role) {
      case OrganizationNameRole.Principal:
        result = RoleSequence.Principal;
        break;
      case OrganizationNameRole.NotifyParty:
        result = RoleSequence.NotifyParty;
        break;
      case OrganizationNameRole.AlsoNotify:
        result = RoleSequence.AlsoNotifyParty;
        break;
      case OrganizationNameRole.Supplier:
        result = RoleSequence.Supplier;
        break;
      case OrganizationNameRole.Pickup:
        result = RoleSequence.PickupAddress;
        break;
      case OrganizationNameRole.BillingParty:
        result = RoleSequence.BillingAddress;
        break;
      case OrganizationNameRole.OriginAgent:
        result = RoleSequence.OriginAgent;
        break;
      default:
        break;
    }

    return result;
  }

  get hiddenBtnCancel() {
    const currentUser = this.service.currentUser;
    if (this.model.stage > POFulfillmentStageType.ForwarderBookingConfirmed) {
      return true;
    }

    if (this.model.status === POFulfillmentStatus.Inactive) {
      return true;
    }

    if (!currentUser.isInternal) {
      if (currentUser.role.id === Roles.Warehouse) {
        return false;
      }

      if (currentUser.role.id !== Roles.Shipper) {
        return true;
      }
      if (currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
        return true;
      }
    }
    return false;
  }

  get hiddenBtnCargoReceive() {
    const currentUser = this.service.currentUser;

    if (this.model.status === POFulfillmentStatus.Inactive) {
      return true;
    }

    if (this.model.stage !== POFulfillmentStageType.ForwarderBookingConfirmed) {
      return true;
    }

    if (!currentUser.isInternal) {
      if (currentUser.role.id !== Roles.Warehouse) {
        return true;
      }
    }

    return false;
  }

  get hiddenBtnEdit() {
    if (this.model.status === POFulfillmentStatus.Inactive) {
        return true;
    }
    return false;
}

  onCancelWarehouseFulfillmentClick() {
    this.cancelWarehouseFulfillmentDialog = true;
  }

  onYesOfCancelDialogClick() {
    this.isCancelling = true;
    this.cancelWarehouseFulfillmentDialog = false;
    this.service.cancelWarehouseFulfillment(this.model.id, this.cancelReason).subscribe(() => {
          this.cancelReason = "";
          this._gaService.emitAction('Cancel', GAEventCategory.WarehouseBooking);
          this.notification.showSuccessPopup(
            "confirmation.cancelSuccessfully",
            "label.poFulfillment"
          );
          this.router.navigate([
            `/warehouse-bookings/view/${this.model.id}`
          ]);
          this.ngOnInit();
          this.isCancelling = false;
          this.activityComponent?.loadActivity();
        },
          () => {
            this.cancelReason = "";
            this.notification.showErrorPopup(
              "confirmation.cancelUnsuccessfully",
              "label.poFulfillment"
            );
            this.isCancelling = false;
          });
  }

  onNoOfCancelDialogClick() {
    this.cancelWarehouseFulfillmentDialog = false;
    this.cancelReason = "";
  }

  onConfirm(){
    const customerId = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Principal)?.organizationId;
    const supplier = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Supplier)?.companyName;
    const expectedDeliveryDate = moment(this.model.expectedDeliveryDate).format('MM-DD-YYYY').toLocaleString();
    const queryParams = {
        bookingNoFrom: this.model.number,
        bookingNoTo: this.model.number,
        expectedHubArrivalDateFrom: expectedDeliveryDate,
        expectedHubArrivalDateTo: expectedDeliveryDate,
        customerId: customerId,
        supplier: supplier
    }
    this.router.navigate(['/warehouse-bookings-confirm', queryParams]);
  }

  get isHiddenConfirmBtn(){
    const currentUser = this.service.currentUser;

    if (this.model.status === POFulfillmentStatus.Inactive) {
      return true;
    }

    if (this.model.stage !== POFulfillmentStageType.ForwarderBookingRequest) {
      return true;
    }
    if (this.model.buyerApprovals?.length > 0) {
      var isPendding = this.model.buyerApprovals.some(c => c.stage === BuyerApprovalStage.Pending)
      return isPendding;
    }
    if (!currentUser.isInternal) {
      return currentUser.role.id != Roles.Warehouse;
    }
    return false;
  }

  backToList() {
    this.router.navigate(["/po-fulfillments"]);
  }
}
