import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, filter, tap } from 'rxjs/operators';
import { ConsolidationStage, DropDownListItemModel, FormComponent, StringHelper, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { Carton, CubicMeter, GAEventCategory, Kilograms } from 'src/app/core/models/constants/app-constants';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ConsignmentDropdownItemModel, ConsolidationConsignmentFormService } from './consolidation-consignment-form.service';

@Component({
  selector: 'app-consolidation-consignment-form',
  templateUrl: './consolidation-consignment-form.component.html',
  styleUrls: ['./consolidation-consignment-form.component.scss']
})
export class ConsolidationConsignmentFormComponent extends FormComponent implements OnInit, OnDestroy {
  @Input('model') consignmentList: any[];
  @Input() consolidationId: number;
  @Input() stage: ConsolidationStage;
  @Input() parentIntegration$: Subject<IntegrationData>;
  readonly AppPermissions = AppPermissions;
  ConsolidationStage = ConsolidationStage;

  currentUser: any;

  /**Shipment number selection declarations */
  shipmentNumberOptionsSource: DropDownListItemModel<number>[];
  shipmentNumberSearchTermKeyUp$ = new Subject<string>();
  isShipmentNumberSearching: boolean = false;

  /**Execution agent selection declarations */
  executionAgentOptionsSource: ConsignmentDropdownItemModel[];
  isDisableExecutionAgentSelection: boolean = true;

  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  validationRules = {
    shipmentNumber: {
      required: 'label.shipmentNo',
      invalid: 'msg.invalidShipmentNumber'
    },
    executionAgent: {
      required: 'label.executionAgent'
    }
  };

  constructor(private _service: ConsolidationConsignmentFormService,
    protected route: ActivatedRoute,
    public notification: NotificationPopup,
    public router: Router,
    public translateService: TranslateService,
    public _userContext: UserContextService,
    private _gaService: GoogleAnalyticsService) {
    super(route, _service, notification, translateService, router);
  }

  ngOnInit() {
    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
      }
    });

    this._registerEventHandlers();
  }

  private _registerEventHandlers(): void {

    /* Register handler for key input on shipment number search term
    */
    const sub1 = this.shipmentNumberSearchTermKeyUp$.pipe(
      debounceTime(500),
      tap((searchTerm: string) => {
        this.shipmentNumberOptionsSource = [];
        if (StringHelper.isNullOrEmpty(searchTerm) || searchTerm.length === 0 || searchTerm.length >= 3) {
          this._shipmentNumberFilterChange(searchTerm);
        }
      }
      )).subscribe();

    this._subscriptions.push(sub1);

    /* Handle when the loaded cargo detail list has been updated successfully.
    */
    const sub2 = this.parentIntegration$.pipe(
      filter((eventContent: IntegrationData) =>
        eventContent.name === '[consolidation-cargo-detail-list]cargoDetailListUpdated'
      )).subscribe((eventContent: IntegrationData) => {
        const updatedList = eventContent.content;
        this.consignmentList.forEach(cs => {
          const updatedListByShipment = updatedList.filter(c => c.shipmentId === cs.shipmentId);
          cs.package = updatedListByShipment.map(x => x.package).reduce(function (a, b) { return a + b; }, 0);
          cs.volume = updatedListByShipment.map(x => x.volume).reduce(function (a, b) { return a + b; }, 0);
          cs.grossWeight = updatedListByShipment.map(x => x.grossWeight).reduce(function (a, b) { return a + b; }, 0);

        });
      });

    this._subscriptions.push(sub2);

    /* Handle when the cargo detail(s) has been loaded successfully.
    */
    const sub3 = this.parentIntegration$.pipe(
      filter((eventContent: IntegrationData) =>
        eventContent.name === '[load-cargo-detail-popup]cargoDetailLoaded'
      )).subscribe((eventContent: IntegrationData) => {
        const loadedList = eventContent.content;
        this.consignmentList.forEach(cs => {
          const loadedListByShipment = loadedList.filter(c => c.shipmentId === cs.shipmentId);
          cs.package = loadedListByShipment.map(x => x.package).reduce(function (a, b) { return a + b; }, 0);
          cs.volume = loadedListByShipment.map(x => x.volume).reduce(function (a, b) { return a + b; }, 0);
          cs.grossWeight = loadedListByShipment.map(x => x.grossWeight).reduce(function (a, b) { return a + b; }, 0);
        });
      });

    this._subscriptions.push(sub3);
  }

  _shipmentNumberFilterChange(value: string) {
    // Only call to server after user input >= 3 characters
    if (value.length >= 3) {
      this.isShipmentNumberSearching = true;
      const sub = this._service.searchShipmentNumberSelectionOptions(this.consolidationId, value)
        .subscribe(
          (data: Array<DropDownListItemModel<number>>) => {
            this.shipmentNumberOptionsSource = data;
          },
          (error) => {

          },
          () => {
            this.isShipmentNumberSearching = false;
          }
        );
      this._subscriptions.push(sub);
    }
  }

  _shipmentNumberValueChange(value: string) {
    this.executionAgentOptionsSource = [];
    const newConsignment = this.consignmentList.find(x => x.isAddLine);
    newConsignment.id = null;
    newConsignment.executionAgentName = null;
    this.checkValidShipmentNumber(value);
  }

  _executionAgentValueChange(value: number) {
    const selectedExecutionAgent = this.executionAgentOptionsSource.find(x => x.value === value);
    const newConsignment = this.consignmentList.find(x => x.isAddLine);
    newConsignment.executionAgentName = selectedExecutionAgent.text;
  }

  checkValidShipmentNumber(number: string) {
    if (StringHelper.isNullOrEmpty(number)) {
      this.setInvalidControl('shipmentNumber', 'required');
      this.isDisableExecutionAgentSelection = true;
    } else {
      const selectedIndex = this.shipmentNumberOptionsSource?.findIndex(x => x.text === number) ?? -1;
      const isValid = selectedIndex !== -1;
      if (!isValid) {
        this.setInvalidControl('shipmentNumber', 'invalid');
        this.isDisableExecutionAgentSelection = true;
      } else {
        this.setValidControl('shipmentNumber');
        this._service.getAgentDropdown(this.shipmentNumberOptionsSource[selectedIndex].value)
          .subscribe(
            (data: Array<ConsignmentDropdownItemModel>) => {
              this.executionAgentOptionsSource = data;
              let defaultExecutionAgent = null;
              // Set default execution Agent
              if (this.currentUser.isInternal) {
                this.isDisableExecutionAgentSelection = false;
                defaultExecutionAgent = this.executionAgentOptionsSource[0];
              } else {
                defaultExecutionAgent = this.executionAgentOptionsSource?.find(x => x.executionAgentId === this.currentUser.organizationId);
              }
              const newConsignment = this.consignmentList.find(x => x.isAddLine);
              newConsignment.id = defaultExecutionAgent?.value;
              newConsignment.executionAgentName = defaultExecutionAgent?.text;
            }
          );
      }
    }
  }

  addBlankRow() {
    this.isDisableExecutionAgentSelection = true;
    this.consignmentList.push({
      isAddLine: true,
      shipment: {
        shipmentNo: ''
      },
      executionAgentName: '',
      package: 0,
      packageUOM: Carton,
      volume: 0,
      volumeUOM: CubicMeter,
      grossWeight: 0,
      grossWeightUOM: Kilograms
    });
  }

  onRemoveClicked(dataItem, rowIndex) {
    if (dataItem.isAddLine) {
      this.consignmentList.splice(rowIndex, 1);

      // Clear formErrors
      const formErrorNames = Object.keys(this.validationRules);

      Object.keys(this.formErrors)
        .filter(x => formErrorNames.some(y => x.startsWith(y)))
        .map(x => {
          delete this.formErrors[x];
        });

      return;
    }
    const confirmDlg = this.notification.showConfirmationDialog('msg.removeShipment', 'label.consolidation');
    confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          this._service.removeLinkingConsignment(this.consolidationId, dataItem.id).subscribe(
            success => {
              this.notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
              this.consignmentList.splice(rowIndex, 1);
              // emit an event to notify subscribers about the update.
              const emitValue = {
                name: '[consolidation-consignment-form]consignmentDeleted',
                content: dataItem
              };
              this.parentIntegration$.next(emitValue);
              this._gaService.emitAction('Delete Consignment', GAEventCategory.Consolidation);
            },
            err => this.notification.showErrorPopup('save.failureNotification', 'label.consolidation')
          );
        }
      });
  }

  onSaveClicked(dataItem, rowIndex) {
    dataItem.shipmentId = this.shipmentNumberOptionsSource.find(x => x.text === dataItem.shipment.shipmentNo).value;
    this._service.createLinkingConsignment(this.consolidationId, dataItem.id).subscribe(
      success => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
        dataItem.isAddLine = false;
        this._gaService.emitAction('Add Consignment', GAEventCategory.Consolidation);
      },
      err => this.notification.showErrorPopup('save.failureNotification', 'label.consolidation')
    );
  }

  get isAddingConsignment() {
    return this.consignmentList?.findIndex(x => x.isAddLine) !== -1;
  }

  get isDisableDeleteButton() {
    return this.consignmentList.findIndex(x => x.isAddLine) !== -1;
  }

  rowCallback(args) {
    // Deleted row will be marked with removed property.
    return { 'hide-row': args.dataItem.removed };
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }

}
