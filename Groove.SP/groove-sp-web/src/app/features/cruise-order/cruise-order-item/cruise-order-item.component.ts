import { AfterViewChecked, ChangeDetectorRef, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { faCaretDown, faPencilAlt, faPlus, faCopy, faTimes, faEllipsisH, faBars, faCommentDots } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { isNumber } from '@progress/kendo-angular-dropdowns/dist/es2015/util';
import { GridComponent } from '@progress/kendo-angular-grid';
import { Subscription } from 'rxjs';
import { DateHelper, Roles, StringHelper, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { CruiseOrderLineItemOptionModel } from 'src/app/core/models/cro-lineitem.model';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { NoteModel } from 'src/app/core/models/note.model';
import { NoteService } from 'src/app/core/notes/note.service';
import { CommonService } from 'src/app/core/services/common.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { CruiseOrderDetailService } from '../cruise-order-detail/cruise-order-detail.service';
import { CruiseOrderLineItemNoteModel } from '../models/cruise-order-item-note.model';
import { CruiseOrderItemModel, ReviseCruiseOrderItemModel } from '../models/cruise-order-item.model';
import { CruiseOrderModel } from '../models/cruise-order.model';
import { EditCruiseOrderItemNoteEventModel, RemoveCruiseOrderItemNoteEventModel } from './cruise-order-item-note-list/cruise-order-item-note-list.component';
import { CruiseOrderItemService } from './cruise-order-item.service';

@Component({
    selector: 'app-cruise-order-item',
    templateUrl: './cruise-order-item.component.html',
    styleUrls: ['./cruise-order-item.component.scss'],
})

export class CruiseOrderItemComponent implements OnInit, OnDestroy, AfterViewChecked {
    @Input() formData: CruiseOrderModel;
    @Input() isEditMode: boolean;
    @Input() tabPrefix: string;
    @Input() formErrors: Array<any>;
    @ViewChild('croItemKendoGrid', { static: true }) croItemKendoGrid: GridComponent;

    // TODO: apply strongly type to these models
    itemDetail: any;
    warehouse: any;
    activities: any;
    attachments: any[];

    noteDetail: CruiseOrderLineItemNoteModel;
    isOpenNotePopup: boolean = false;
    isAddNote: boolean = true;
    selectedNoteEventData: EditCruiseOrderItemNoteEventModel;

    readonly AppPermissions = AppPermissions;
    faPencilAlt = faPencilAlt;
    faPlus = faPlus;
    faCaretDown = faCaretDown;
    faRemove = faTimes;
    faCopy = faCopy;
    faBars = faBars;
    faCommentDots = faCommentDots;

    currentUser: any;

    /** Available options for Cruise Order Item auto-complete */
    itemOptions: Array<string>;

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    itemDetailPopupOpened = false;
    itemCopyPopupOpened = false;
    itemDetailModel: CruiseOrderItemModel;
    loadingGrids = {};

    get model(): Array<CruiseOrderItemModel> {
        return this.formData.items;
    }

    /**
     * To store current cruise order item that are being focusing as clicking "more actions" button
     */
    currentFocusingCruiseItem: CruiseOrderItemModel;

    constructor(
        private cdr: ChangeDetectorRef,
        private _userContext: UserContextService,
        public _notification: NotificationPopup,
        private _noteService: NoteService,
        private _cruiseOrderDetailService: CruiseOrderDetailService,
        private _cruiseOrderItemService: CruiseOrderItemService,
        private _translateService: TranslateService,
        private _gaService: GoogleAnalyticsService
    ) {
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
            }
        });
        this._fetchDataSources();
    }

    ngOnInit() {
        this._initializeItemOptions();
        if (this.model?.length === 1) {
            this.croItemKendoGrid.expandRow(0);
        }
    }

    _initializeItemOptions() {
        this.itemOptions = new Array<string>();
        [...this.model]?.sort((a, b) => a.poLine > b.poLine ? 1 : -1)?.forEach(x => {
            this.itemOptions.push((new CruiseOrderLineItemOptionModel(x.id, x.poLine, x.itemId, x.itemName)).optionText);
        });
    }

    private _fetchDataSources(): void {

    }

    ngAfterViewChecked(): void {
        this.cdr.detectChanges();
    }

    ngOnDestroy(): void {
        this._subscriptions.map((x) => x.unsubscribe());
    }


    /**
     * To set data for current focusing cruise item
     * @param dataItem Data of current cruise item row
     */
     setCurrentFocusingItem(dataItem: CruiseOrderItemModel) {
        this.currentFocusingCruiseItem = dataItem;
    }

    /**
     * To render sub-menu for "More actions" button
     * @param dataItem Data of each cruise order item row
     * @returns Array of menu options
     */
    getMoreActionMenu(dataItem: CruiseOrderItemModel): Array<any> {
        const result = [{
            actionName:  this._translateService.instant('label.edit'),
            icon: 'edit',
            click: () => {
                this.onEditItemDetailClicked(this.currentFocusingCruiseItem);
            }
        }, {
            actionName:  this._translateService.instant('label.copy'),
            icon: 'copy',
            click: () => {
                this.onCopyItemClicked(this.currentFocusingCruiseItem);
            }
        }];

        // check for originalItemId available
        if (this.isAllowToDeleteItem(dataItem)) {
            result.push({
                actionName:  this._translateService.instant('tooltip.delete'),
                icon: 'delete',
                click: () => {
                    this.onDeleteItemClicked(this.currentFocusingCruiseItem);
                }
            });
        }

        return result;

    }

    /**
     * to check if cruise order item is able to remove (hard delete)
     * @param dataItem data of cruise order item
     * @returns boolean
     */
    isAllowToDeleteItem(dataItem: CruiseOrderItemModel): boolean {
        // originalItemId must available and (internal or external user belongs to same organization)
        return !StringHelper.isNullOrEmpty(dataItem?.originalItemId)
                && (this.currentUser.isInternal || dataItem.originalOrganizationId === this.currentUser.organizationId);
    }

    //#region Item Details Popup Handlers
    onEditItemDetailClicked(dataItem) {
        this.itemDetailModel = dataItem;
        this.itemDetailModel.itemUpdates = CruiseItemUpdates.Updated;
        this.itemDetailPopupOpened = true;
    }

    onCopyItemClicked(dataItem) {
        // copy some values into new item
        this.itemDetailModel = Object.assign({}, dataItem);
        this.itemDetailModel.originalItemId = this.itemDetailModel.id;
        this.itemDetailModel.originalOrganizationId = this.currentUser.organizationId;
        this.itemDetailModel.itemUpdates = `${CruiseItemUpdates.Copied}#${this.itemDetailModel.id}`;
        // poLine, requestLine = last sequence + 1
        const poLines = this.model.map(x => x.poLine).filter(x => !isNaN(x));
        this.itemDetailModel.poLine = poLines && poLines.length > 0
            ? Math.max(...poLines) + 1 : 0;

        const requestLines = this.model.map(x => x.requestLine).filter(x => !isNaN(x));
        this.itemDetailModel.requestLine = requestLines && requestLines.length > 0
            ? Math.max(...requestLines) + 1 : 0;

        this.itemDetailModel.id = null;
        this.itemDetailModel.shipmentId = null; // Note that Shipment info is not copied.
        this.itemDetailModel.shipment = null; // Note that Shipment info is not copied.
        this.itemDetailModel.latestDialog = null; // Note that latest dialog is not copied.

        // inputted fields
        this.itemDetailModel.itemId = null;
        this.itemDetailModel.itemName = null;
        this.itemDetailModel.origin = null;
        this.itemDetailModel.destination = null;
        this.itemDetailModel.deliveryPort = null;
        this.itemDetailModel.destinationCountry = null;
        this.itemDetailModel.requestLineShoreNotes = null;
        this.itemDetailModel.shipRequestLineNotes = null;
        this.itemDetailModel.requestQuantity = null;
        this.itemDetailModel.orderQuantity = null;
        this.itemDetailModel.makerReferenceOfItemName2 = null;
        this.itemDetailModel.quantityDelivered = null;
        this.itemDetailModel.netUnitPrice = null;
        this.itemDetailModel.netUSUnitPrice = null;
        this.itemDetailModel.totalOrderPrice = null;

        this.itemCopyPopupOpened = true;
    }

    /**
     * To handle as detail popup closed
     */
    itemDetailPopupClosedHandler(updatedItemData: ReviseCruiseOrderItemModel) {
        this.itemDetailPopupOpened = false;
        // User click on save button
        if (updatedItemData) {
            const sub = this._cruiseOrderItemService.updateCruiseOrderItem(updatedItemData.id, updatedItemData)
                .subscribe(
                    (data) => {
                        this._notification.showSuccessPopup(
                            'save.sucessNotification',
                            'label.cruiseOrder'
                        );

                        // emit notification to cruise order details level to update model data
                        const emitValue: IntegrationData = {
                            name: '[cruise-order-item]onItemDetailsUpdated',
                            content: {
                                'lineItems': data
                            }
                        };
                        this._cruiseOrderDetailService.integration$.next(emitValue);
                        this._gaService.emitEvent('edit_item', GAEventCategory.CruiseOrder, 'Edit Item');

                    },
                    (error) => {
                        this._notification.showErrorPopup(
                            'save.failureNotification',
                            'label.cruiseOrder'
                        );
                    }
                );
            this._subscriptions.push(sub);
        }
    }

    itemCopyPopupClosedHandler(copiedItemData: CruiseOrderItemModel) {
        this.itemCopyPopupOpened = false;
        if (copiedItemData) {
            this._cruiseOrderItemService.createCruiseOrderItem(copiedItemData)
            .subscribe(
                (returnedData) => {
                    this.model.push(returnedData);
                    this._initializeItemOptions();
                    this._notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.cruiseOrder'
                    );
                    this._gaService.emitEvent('copy_item', GAEventCategory.CruiseOrder, 'Copy Item');
                },
                (error) => {
                    this._notification.showErrorPopup(
                        'save.failureNotification',
                        'label.cruiseOrder'
                    );
                });
        }
    }
    //#region Item Details Popup Handlers

    //#region NOTE Handlers

    onAddNoteClicked(dataItem) {
        this.isOpenNotePopup = true;
        this.isAddNote = true;

        // set current item selection by default
        const defaultOptions = new Array<string>();
        defaultOptions.push((new CruiseOrderLineItemOptionModel(dataItem.id, dataItem.poLine, dataItem.itemId, dataItem.itemName)).optionText);
        this.noteDetail = new CruiseOrderLineItemNoteModel(this.currentUser.name, defaultOptions);

        this.noteDetail.cruiseOrderId = this.formData.id;
    }

    onEditNoteClicked(event: EditCruiseOrderItemNoteEventModel) {
        this.isOpenNotePopup = true;
        this.isAddNote = false;
        this.selectedNoteEventData = event;
        this.noteDetail = new CruiseOrderLineItemNoteModel();
        this.noteDetail.MapFrom(event.note);
    }

    onDeleteItemClicked(dataItem: CruiseOrderItemModel) {

        const cruiseItemId: number = dataItem.id;
        const confirmDlg = this._notification.showConfirmationDialog('msg.deleteCruiseItemConfirm', 'label.cruiseOrder');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                   this._cruiseOrderItemService.deleteCruiseOrderItem(cruiseItemId).subscribe(
                       (data) => {
                            this._notification.showSuccessPopup( 'save.sucessNotification', 'label.message');
                            this.formData.items = this.formData.items?.filter(x => x.id !== cruiseItemId);
                            this._gaService.emitEvent('delete_item', GAEventCategory.CruiseOrder, 'Delete Item');

                        },
                       (error) => { this._notification.showErrorPopup( 'save.failureNotification', 'label.message'); }
                   );
                }
            });
    }

    /** Handler as user adding new note */
    onNoteAdded(note: CruiseOrderLineItemNoteModel) {
        this.isOpenNotePopup = false;
        const sub = this._noteService
            .createNote(DateHelper.formatDate(note))
            .subscribe(
                (newNoteModel) => {
                    const updatedLineItems = note.cruiseOrderLineItems.map(x => Number.parseInt(x.split('-')[0].trim()));
                    const emitValue: IntegrationData = {
                        name: '[cruise-order-item]onDialogUpdated',
                        content: {
                            'lineItems': updatedLineItems
                        }
                    };
                    this._cruiseOrderDetailService.integration$.next(emitValue);

                    const cruiseOrderItems = this.model.filter(x => updatedLineItems.includes(x.poLine));
                    cruiseOrderItems.forEach(x => x.latestDialog = newNoteModel.category);

                    this._notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.message'
                    );
                    this._gaService.emitEvent('add_dialog', GAEventCategory.CruiseOrder, 'Add Dialog');
                },
                (error) => {
                    this._notification.showErrorPopup(
                        'save.failureNotification',
                        'label.message'
                    );
                }
            );
        this._subscriptions.push(sub);
    }

    /** Handler as user editing a note */
    onNoteEdited(note: NoteModel) {
        this.isOpenNotePopup = false;

        const sub = this._noteService
            .updateNote(note.id, note)
            .subscribe(
                (data) => {
                    const updatedLineItems = JSON.parse(note.extendedData)?.map(x => Number.parseInt(x.split('-')[0].trim()));
                    const emitValue: IntegrationData = {
                        name: '[cruise-order-item]onDialogUpdated',
                        content: {
                            'lineItems': updatedLineItems
                        }
                    };
                    this._cruiseOrderDetailService.integration$.next(emitValue);

                    const cruiseOrderItem = this.model.find(x => x.id === this.selectedNoteEventData.itemId);
                    if (this.selectedNoteEventData.isLatestNote) {
                        cruiseOrderItem.latestDialog = note.category;
                    }
                    this.selectedNoteEventData = null;

                    this._notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.message'
                    );
                    this._gaService.emitEvent('edit_dialog', GAEventCategory.CruiseOrder, 'Edit Dialog');
                }
            );
        this._subscriptions.push(sub);
    }

    /* Handler as user clicking on Delete button */
    onDeleteNoteClicked(item: RemoveCruiseOrderItemNoteEventModel) {
        const sub = this._noteService.deleteNote(item.note.id).subscribe(
            data => {
                const updatedLineItems = JSON.parse(item.note.extendedData)?.map(x => Number.parseInt(x.split('-')[0].trim()));
                const emitValue: IntegrationData = {
                    name: '[cruise-order-item]onDialogUpdated',
                    content: {
                        'lineItems': updatedLineItems
                    }
                };
                this._cruiseOrderDetailService.integration$.next(emitValue);

                const cruiseOrderItem = this.model.find(x => x.id === item.itemId);
                cruiseOrderItem.latestDialog = item.latestNote;

                this._notification.showSuccessPopup(
                    'msg.deleteNoteSuccessfully',
                    'label.message'
                );
                this._gaService.emitEvent('delete_dialog', GAEventCategory.CruiseOrder, 'Delete Dialog');

            },
            error => {
                this._notification.showErrorPopup(
                    'save.failureNotification',
                    'label.message'
                );
            }
        );
        this._subscriptions.push(sub);
    }

    /** Handler as note popup closed */
    onNotePopupClosed() {
        this.isOpenNotePopup = false;
    }

    //#endregion
}

export enum CruiseItemUpdates {
    Updated= 'ItemUpdatedViaUI',
    Copied = 'CopiedFromItemId'
}
