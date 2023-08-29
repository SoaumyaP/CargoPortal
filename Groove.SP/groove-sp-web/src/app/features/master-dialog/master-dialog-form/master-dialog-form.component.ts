import { Component, OnDestroy, OnInit } from '@angular/core';
import * as cloneDeep from 'lodash/cloneDeep';
import { ActivatedRoute, Router } from '@angular/router';
import { faPencilAlt, fas } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, map, tap } from 'rxjs/operators';
import { Category, DropDowns, FormComponent, MasterDialogFilterCriteria, MessageDisplayOn, UserContextService } from 'src/app/core';
import { StringHelper } from 'src/app/core/helpers/string.helper';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { MasterDialogFormService } from './master-dialog-form.service';
import { MasterDialogPOListItemModel } from '../models/master-dialog-po-list-item.model';
import { MasterDialogPOListSelectedItemModel } from '../models/master-dialog-po-list-selected-item.model';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
  selector: 'app-master-dialog-form',
  templateUrl: './master-dialog-form.component.html',
  styleUrls: ['./master-dialog-form.component.scss']
})
export class MasterDialogFormComponent extends FormComponent implements OnInit, OnDestroy {
  faPencilAlt = faPencilAlt;
  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  /** Data-source for filter value multi-select  */
  filterValueOptionSource = null;
  filterValueSearchTermKeyUp$ = new Subject<string>();
  listOfPOSearchTermKeyUp$ = new Subject<any>();
  isFilterValueSearching = false;
  isSubmittingMasterDialog = false;
  isLoadingListOfPo = false;
  isShowLoadMoreItems = false;
  itemsSelected: Array<MasterDialogPOListSelectedItemModel> = [];
  currentUser: any;
  isTheFirstTimeInitComponent = true;

  constructor(
    protected route: ActivatedRoute,
    public masterDialogFormService: MasterDialogFormService,
    public notification: NotificationPopup,
    public translateService: TranslateService,
    public router: Router,
    public _userContext: UserContextService) {
    super(route, masterDialogFormService, notification, translateService, router);
  }

  modelName = 'masterDialogs';
  messageOnPageDropdown = DropDowns.MessageDisplayOn;

  filterCriteriaDropdown = DropDowns.MasterDialogFilterCriteria;

  categoryDropdown = DropDowns.Category;

  listOfPOItems = [];
  listOfPOItemFiltered = [];
  listOfPOItemFilteredWithPaging: Array<MasterDialogPOListItemModel> = [];

  isDisabledFilterCriteria = false;

  currentPage = 0;
  numberPoItemOnPerPage = 20;

  validationRules = {
    displayOn: {
      required: 'label.dialogMessageLevel'
    },
    filterCriteria: {
      required: this.searchByLabel
    },
    filterValue: {
      required: this.dialogApplyToLabel
    },
    category: {
      required: 'label.category'
    },
    message: {
      required: 'label.message'
    },
  };

  onInitDataLoaded(data) {
    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
      }
    });

    this._registerEventHandlers();

    if (this.isAddMode) {
      this.setDefaultValueForForm();
    } else {
      this.bindingModelToForm(data);
    }
  }

  setDefaultValueForForm() {
    this.model.category = Category.General;

    this.isDisabledFilterCriteria = true;
    this.model.displayOn = null;
    this.model.filterCriteria = null;
    this.model.poNumber = '';
    this.model.filterValue = null;
  }

  private _registerEventHandlers(): void {
    const sub = this.filterValueSearchTermKeyUp$.pipe(
      debounceTime(1000),
      tap((searchTerm: string) => {
        if (StringHelper.isNullOrEmpty(searchTerm) || searchTerm.length === 0 || searchTerm.length >= 3) {
          this._filterValueFilterChange(searchTerm);
        }
      }
      )).subscribe();

    const sub1 = this.listOfPOSearchTermKeyUp$.pipe(
      debounceTime(500),
      tap((data: any) => {
        if (StringHelper.isNullOrEmpty(data.searchTerm) || data.searchTerm.length === 0 || data.searchTerm.length >= 3) {
          this._listOfPurchaseOrdersFilterChange(data.skip, data.take, data.searchTerm, data.isLoadMoreItems);
        }
      }
      )).subscribe();

    this._subscriptions.push(sub);
    this._subscriptions.push(sub1);
  }

  private _filterValueFilterChange(value: string) {
    // Only call to server after user input >= 3 characters
    if (value.length >= 3) {
      this.isFilterValueSearching = true;
      const sub = this.masterDialogFormService.searchNumberByFilterCriteria(value, this.model.filterCriteria)
        .subscribe(
          (data) => {
            this.filterValueOptionSource = data;
          },
          (error) => {

          },
          () => {
            this.isFilterValueSearching = false;
          }
        );
      this._subscriptions.push(sub);
    }
  }

  private _listOfPurchaseOrdersFilterChange(skip: number, take: number, searchTerm: string = '', isLoadMoreItems: boolean = false) {
    const displayOn = this.model.displayOn;
    const filterCriteria = this.model.filterCriteria;
    const filterValue = this.model.filterValue.map(c => c.value).join(',');

    // Only call to server after user input >= 3 characters
    if ((searchTerm.length >= 3 || StringHelper.isNullOrEmpty(searchTerm))
      && !StringHelper.isNullOrEmpty(displayOn)
      && !StringHelper.isNullOrEmpty(filterCriteria)
      && !StringHelper.isNullOrEmpty(filterValue)
    ) {

      // Reset current page when users change filter value or searching po number
      if (!isLoadMoreItems) {
        this.currentPage = 0;
      }

      let $obs = this.masterDialogFormService.searchListOfPurchaseOrders(displayOn, filterCriteria, filterValue, searchTerm, skip, take);
      if (this.isViewMode || this.isEditMode) {
        $obs = this.masterDialogFormService.searchListOfPurchaseOrdersByMasterDialogId(this.model.id, searchTerm, skip, take);
      }

      const sub = $obs
        .subscribe(
          (data: Array<any>) => {
            this.listOfPOItemFiltered = data;

            if (isLoadMoreItems) {
              this.listOfPOItemFilteredWithPaging.push(...this.listOfPOItemFiltered);
            } else {
              this.listOfPOItemFilteredWithPaging = this.listOfPOItemFiltered;
            }

            this.bindingPOSelectedToPOTree();

            this.isLoadingListOfPo = false;
          },
          (error) => {
            this.isLoadingListOfPo = false;
          },
          () => {
            this.isLoadingListOfPo = false;
          }
        );
      this._subscriptions.push(sub);
    }
  }

  onFilterValueOpened(event) {
    this.filterValueOptionSource = null;
  }

  onFilterValueChanged(value) {
    this.currentPage = 0;
    this.model.poNumber = '';
    this.itemsSelected = [];

    if (value && value.length > 0) {
      this.isLoadingListOfPo = true;
    }

    if (value.length === 0) {
      this.listOfPOItemFiltered = [];
      this.listOfPOItemFilteredWithPaging = [];
    }

    this._listOfPurchaseOrdersFilterChange(0, this.numberPoItemOnPerPage);
  }

  isFilterValueSelected(value: string) {
    return this.model.filterValue?.some(c => c.value === value) ?? false;
  }

  onChangeMessageOnPage() {
    this.model.filterCriteria = null;
    this.model.filterValue = null;
    this.validationRules.filterCriteria['required'] = this.searchByLabel;
    this.resetPOTree();

    if (!this.model.displayOn) {
      this.isDisabledFilterCriteria = true;
      return;
    } else {
      this.isDisabledFilterCriteria = false;
    }

    if (this.model.displayOn === MessageDisplayOn.PurchaseOrders) {
      this.filterCriteriaDropdown = DropDowns.MasterDialogFilterCriteria.filter(c =>
        !c.value ||
        c.value === MasterDialogFilterCriteria.MasterBLNo ||
        c.value === MasterDialogFilterCriteria.HouseBLNo ||
        c.value === MasterDialogFilterCriteria.ContainerNo ||

        c.value === MasterDialogFilterCriteria.PurchaseOrderNo
      );
      return;
    }

    if (this.model.displayOn === MessageDisplayOn.Bookings) {
      this.filterCriteriaDropdown = DropDowns.MasterDialogFilterCriteria.filter(c =>
        !c.value ||
        c.value === MasterDialogFilterCriteria.MasterBLNo ||
        c.value === MasterDialogFilterCriteria.HouseBLNo ||
        c.value === MasterDialogFilterCriteria.ContainerNo ||

        c.value === MasterDialogFilterCriteria.BookingNo
      );
      return;
    }

    if (this.model.displayOn === MessageDisplayOn.Shipments) {
      this.filterCriteriaDropdown = DropDowns.MasterDialogFilterCriteria.filter(c =>
        !c.value ||
        c.value === MasterDialogFilterCriteria.MasterBLNo ||
        c.value === MasterDialogFilterCriteria.HouseBLNo ||
        c.value === MasterDialogFilterCriteria.ContainerNo ||

        c.value === MasterDialogFilterCriteria.ShipmentNo
      );
    }
  }

  onFilterCriteriaValueChanged() {
    this.model.filterValue = null;
    this.validationRules.filterValue['required'] = this.dialogApplyToLabel;
    this.resetPOTree();
  }

  onCheckParentItem(parentItem) {
    parentItem.isChecked = !parentItem.isChecked;
    if (!StringHelper.isNullOrEmpty(parentItem.childrenItems)) {
        parentItem.childrenItems.forEach(e => e.isDisabled = !e.isDisabled);
    }

    this.setSelectedItems(parentItem);
    this.model.selectedItems = '[]';
  }

  onCheckChildrenItem(parentItem, childItem) {
    childItem.isChecked = !childItem.isChecked;
    if (!StringHelper.isNullOrEmpty(parentItem.childrenItems)) {
        parentItem.isDisabled = parentItem.childrenItems.some(c => c.isChecked) ? true : false;
    }

    this.setSelectedItems(childItem);
    this.model.selectedItems = '[]';
  }

  setSelectedItems(item) {
    const itemSelected = this.itemsSelected.find(c => c.value === item.value);

    if (!itemSelected) {
      this.itemsSelected.push({
        dialogItemNumber: item.dialogItemNumber,
        value: item.value,
        parentId: item.parentId
      });
    } else {
      this.itemsSelected = this.itemsSelected.filter(c => c.value !== item.value);
    }
  }

  /**
   * Clear PO tree
   */
  resetPOTree() {
    this.listOfPOItemFiltered = [];
    this.listOfPOItemFilteredWithPaging = [];
    this.itemsSelected = [];
    this.model.selectedItems = '[]';
  }

  onLoadMoreItems() {
    this.currentPage++;
    this.isLoadingListOfPo = true;
    this.isTheFirstTimeInitComponent = false;

    this._listOfPurchaseOrdersFilterChange(
      this.currentPage * this.numberPoItemOnPerPage,
      this.numberPoItemOnPerPage,
      this.model?.poNumber.length >= 3 ? this.model.poNumber : '',
      true
    );

    this.bindingPOSelectedToPOTree();
  }

  bindingPOSelectedToPOTree() {

    if (!this.isAddMode && this.isTheFirstTimeInitComponent) {
      const itemsSelectedFromDatabase = JSON.parse(this.model.selectedItems);

      if (itemsSelectedFromDatabase.length > 0) {
        this.itemsSelected.push(...itemsSelectedFromDatabase);
      }
    }

    for (const parentItem of this.listOfPOItemFilteredWithPaging) {

      if (this.isEditMode) {
        parentItem.isDisabled = false;
        if (!StringHelper.isNullOrEmpty(parentItem.childrenItems)) {
            parentItem.childrenItems.forEach(c => c.isDisabled = false);
        }
      }

      const parentItemSelected = this.itemsSelected.some(c => c.value === parentItem.value);

      if (parentItemSelected) {
        parentItem.isChecked = true;
        parentItem.isDisabled = false;
        if (!StringHelper.isNullOrEmpty(parentItem.childrenItems)) {
            parentItem.childrenItems.forEach(c => c.isDisabled = true);
        }
      } else {
        for (const childItem of (parentItem.childrenItems || [])) {
          childItem.isDisabled = false;

          const childItemSelected = this.itemsSelected.some(c => c.value === childItem.value);
          if (childItemSelected) {
            parentItem.isDisabled = true;
            childItem.isChecked = true;
          }
        }
      }

      // Disabled all checkbox for view/edit mode
      if (this.isViewMode || this.isEditMode) {
        parentItem.isDisabled = true;
        if (!StringHelper.isNullOrEmpty(parentItem.childrenItems)) {
            parentItem.childrenItems.forEach(c => c.isDisabled = true);
        }
      }
    }
  }

  onDeleteClicked() {
    const confirmDlg = this.notification.showConfirmationDialog('msg.deleteNoteConfirm', 'label.message');
    confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          this.service.delete(this.modelId).subscribe(
            rsp => {
              this.notification.showSuccessPopup(
                'save.sucessNotification',
                'label.masterDialog');
              this.router.navigateByUrl('/master-dialogs');
            },
            err => {
              this.notification.showErrorPopup(
                'save.failureNotification',
                'label.masterDialog');
            }
          );
        }
      });
  }

  onSubmit() {
    this.validateAllFields(false);
    if (this.mainForm.form.invalid || this.isSubmittingMasterDialog) {
      return;
    }

    if (this.mainForm.invalid) {
      return;
    }

    if (this.isAddMode) {
      this.addNewMasterDialog();
    }

    if (this.isEditMode) {
      this.updateMasterDialog();
    }
  }

  addNewMasterDialog() {
    this.isSubmittingMasterDialog = true;
    const viewModel = this.convertModelBeforeSave();
    const sub = this.masterDialogFormService.create(viewModel).subscribe(
      r => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.masterDialog');
        this.isSubmittingMasterDialog = false;
        this.backToList();
      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.masterDialog');
        this.isSubmittingMasterDialog = false;
      }
    );

    this._subscriptions.push(sub);
  }

  convertModelBeforeSave() {
    const viewModel = cloneDeep(this.model);
    viewModel.filterValue = viewModel.filterValue.map(c => c.value).join(', ');
    viewModel.owner = this.currentUser.name;
    viewModel.organizationId = this.currentUser.organizationId;
    viewModel.selectedItems = JSON.stringify(this.itemsSelected);
    return viewModel;
  }

  updateMasterDialog() {
    const viewModel = this.convertModelBeforeSave();

    this.isSubmittingMasterDialog = true;
    const sub = this.masterDialogFormService.update(this.model.id, viewModel).subscribe(
      r => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.masterDialog');
        this.isSubmittingMasterDialog = false;
        this.router.navigate([
          `/master-dialogs/view/${this.model.id}`
        ]);
        this.itemsSelected = [];
        this.isTheFirstTimeInitComponent = true;
        this.ngOnInit();
      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.masterDialog');
        this.isSubmittingMasterDialog = false;
      }
    );

    this._subscriptions.push(sub);
  }

  onSearchPONumber(value) {
    if (value.length === 0 || value.length >= 3) {
      this.isLoadingListOfPo = true;
    } else {
      this.isLoadingListOfPo = false;
    }

    this.isTheFirstTimeInitComponent = false;
    this.listOfPOSearchTermKeyUp$.next({ skip: 0, take: this.numberPoItemOnPerPage, searchTerm: value });
  }

  onEditMasterDialog() {
    this.isEditMode = true;
    this.isViewMode = false;
    this.isAddMode = false;
    this.isTheFirstTimeInitComponent = false;
    this.bindingPOSelectedToPOTree();
  }

  bindingModelToForm(data) {
    const listOfFilterValues = data[0].filterValue.split(', ');
    this.model.filterValue = [];
    this.model.poNumber = '';
    this.isLoadingListOfPo = true;

    for (const item of listOfFilterValues) {
      this.model.filterValue.push({
        text: item,
        value: item
      });
    }

    this._listOfPurchaseOrdersFilterChange(0, this.numberPoItemOnPerPage);
  }



  backToList() {
    this.router.navigate(['/master-dialogs']);
  }

  onCancelMassage() {
    const confirmDlg = this.notification.showConfirmationDialog('label.cancelChangeOnMasterDialog', 'label.confirmation');
    const sub = confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          if (this.isEditMode) {
            this.router.navigate([
              `/master-dialogs/view/${this.model.id}`
            ]);
            this.itemsSelected = [];
            this.isTheFirstTimeInitComponent = true;
            this.ngOnInit();
          } else {
            this.router.navigate(['/master-dialogs']);
          }
        }
      });

    this._subscriptions.push(sub);
  }

  private get searchByLabel() {
    return this.translateService.instant('label.dialogSearchBy',
      {
        msgLevel: this.model.displayOn ?
          this.translateService.instant(this.messageOnPageDropdown.find(x => x.value === this.model.displayOn).label) : ''
      });
  }

  private get dialogApplyToLabel() {
    return this.translateService.instant('label.dialogApplyTo',
      {
        name: this.model.filterCriteria ?
          this.translateService.instant(this.filterCriteriaDropdown.find(x => x.value === this.model.filterCriteria).label) : ''
      });
  }

  ngOnDestroy(): void {
    this._subscriptions.map((x) => x.unsubscribe());
  }
}
