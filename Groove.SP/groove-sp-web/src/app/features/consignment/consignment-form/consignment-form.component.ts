import { Component, ViewChild, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormComponent, UserContextService, DATE_FORMAT, DateHelper, DropDowns, StringHelper, ModeOfTransportType, ConsolidationStage, ConsignmentStatus } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { ConsignmentFormService } from './consignment-form.service';
import { faTrashAlt, faPencilAlt, faEllipsisV, faPlus, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { RowArgs } from '@progress/kendo-angular-grid';
import { Subscription } from 'rxjs';
import { ConsignmentItineraryFormComponent } from '../../shipment/consignment-itinerary-form/consignment-itinerary-form.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { AfterViewChecked } from '@angular/core';
import { DefaultValue2Hyphens, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@Component({
    selector: 'app-consignment-form',
    templateUrl: './consignment-form.component.html',
    styleUrls: ['./consignment-form.component.scss']
})
export class ConsignmentFormComponent extends FormComponent implements OnDestroy, AfterViewChecked {
    modelName = 'consignments';
    modeOfTransportType = ModeOfTransportType;
    stringHelper = StringHelper;
    DATE_FORMAT = DATE_FORMAT;
    readonly AppPermissions = AppPermissions;
    readonly ConsolidationStage = ConsolidationStage;
    readonly ConsignmentStatus = ConsignmentStatus;
    defaultValue = DefaultValue2Hyphens;
    StringHelper = StringHelper;

    validationRules = {
        sequence: {
            required: 'label.sequence'
        },
        modeOfTransport: {
            required: 'label.modeOfTransport'
        },
        executionAgent: {
            required: 'label.executionAgent',
            alreadyUsed: 'validation.executionAgentAlreadyUsed'
        },
        consignmentDate: {
            required: 'label.consignmentDates'
        },
        shipFromETDDate: {
            required: 'label.shipFromETDDates'
        },
        shipToETADate: {
            required: 'label.shipToETADates'
        }
    };

    containerList = [];
    consolidationList = [];
    cargoDetailList = [];
    contactList = [];

    faTrashAlt = faTrashAlt;
    faPencilAlt = faPencilAlt;
    faEllipsisV = faEllipsisV;
    faPlus = faPlus;
    faPowerOff = faPowerOff;

    currentUser: any;

    isFullLoadShipment: boolean = true;
    canClickOnHouseBL: boolean = true;
    canClickOnMasterBL: boolean = true;

    openCargoDescriptionDetailPopup: boolean = false;
    cargoDescriptionDetail: string;

    @ViewChild(ConsignmentItineraryFormComponent, { static: true }) consignmentItineraryFormComponent: ConsignmentItineraryFormComponent;
    itineraryFormOpened: boolean;
    itineraryList: any[];
    itineraryFormMode = 'view';
    itineraryDetails: any = {};

    executionAgentOptions: any[];
    executionAgentName = '';
    allLocationOptions: any;
    filteredExecutionAgentOptions: any[];
    filteredLocationOptions: any[];
    modeOfTransportOptions = DropDowns.ModeOfTransportStringType;
    addConsolidationFormOpened: boolean = false;

    private _subscriptions: Array<Subscription> = [];

    constructor(
        private cdr: ChangeDetectorRef,
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: ConsignmentFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private _gaService: GoogleAnalyticsService) {
        super(route, service, notification, translateService, router);
        let sub = this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }

                this.canClickOnHouseBL = this.currentUser?.permissions?.some(c => c.name === AppPermissions.BillOfLading_HouseBLDetail);
                this.canClickOnMasterBL = this.currentUser?.permissions?.some(c => c.name === AppPermissions.BillOfLading_MasterBLDetail);
            }
        });
        this._subscriptions.push(sub);

        sub = this.service.getOrganizations().subscribe(rsp => {
            this.executionAgentOptions = rsp;
            this.executionAgentOptions.forEach(item => {
                item.label = item.code + ' - ' + item.name;
            });

        });
        this._subscriptions.push(sub);

        sub =  this.service.getAllLocations().subscribe(rsp => {
            this.allLocationOptions = rsp;
        });
        this._subscriptions.push(sub);
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }

    showTextField(name) {
        return this.model[name] ? this.model[name] : this.defaultValue;
    }

    showShipmentTextField(name) {
        return this.model.shipment && this.model.shipment[name] ? this.model.shipment[name] : this.defaultValue;
    }

    onInitDataLoaded(data): void {
        const sub = this.service.getOrganization(data[0].executionAgentId).subscribe(execution => {
            this.executionAgentName = execution ? (execution.code + ' - ' + execution.name) : this.defaultValue;
            this.model.executionAgentName = this.executionAgentName ? this.executionAgentName : '';
        });
        this._subscriptions.push(sub);
        if (this.model.shipment) {
            this.getContainerTab();
            this.getCargoDetailTab();
            this.getContactTab();
            this.getItineraryTab();
        }
    }

    getContainerTab() {
        let sub = this.isFCL ? this.service.getContainers(this.model.shipment.id).subscribe((list: any) => {
            if (list) {
                this.containerList = list;
            }
        }) : this.service.getConsolidations(this.model.id).subscribe((list: any) => {
            if (list) {
                this.consolidationList = list;
            }
        });
        this._subscriptions.push(sub);

        sub = this.service.checkFullLoadShipment(this.model.shipment.id).subscribe((rsp:boolean) => {
            this.isFullLoadShipment = rsp;
        });
        this._subscriptions.push(sub);
    }

    get isFCL() {
        return this.model.movement === DropDowns.MovementStringType.find(x => x.value === 'CY_CFS').label
            || this.model.movement === DropDowns.MovementStringType.find(x => x.value === 'CY_CY').label;
    }

    getCargoDetailTab() {
        const sub = this.service.getCargoDetail(this.model.shipment.id).subscribe((list: any) => {
            if (list) {
                this.cargoDetailList = list;
            }
        });
        this._subscriptions.push(sub);
    }

    getContactTab() {
        const sub = this.service.getContact(this.model.shipment.id).subscribe((list: any) => {
            if (list) {
                this.contactList = list;
            }
        });
        this._subscriptions.push(sub);
    }

    getItineraryTab() {
        const sub = this.service.getItineraries(this.model.id).subscribe(list => {
            if (list) {
                this.itineraryList = list;
            }
        });
        this._subscriptions.push(sub);
    }

    idKey(context: RowArgs): string {
        return context.dataItem.id;
    }

    backList() {
        this.router.navigate(['/consignments']);
    }

    onAddItineraryClick() {
        this.itineraryFormMode = 'add';
        this.itineraryFormOpened = true;
        this.itineraryDetails = {};
    }

    onEditItineraryClick(itinerary) {
        this.itineraryFormMode = 'edit';
        this.itineraryFormOpened = true;
        this.itineraryDetails = Object.assign({}, itinerary);
        this.itineraryDetails.departureDates = this.itineraryDetails.etdDate;
    }

    onDeleteItineraryClick(itinerary) {
        if (this.itineraryList.length === 1) {
            this.notification.showErrorPopup('msg.deleteLastItineraryNotification', 'label.itinerary');
            return;
        }

        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteItineraryConfirmation', 'label.itinerary');

        const affiliates = this.currentUser.affiliates;

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.service.deleteItinerary(this.model.id, itinerary.id, affiliates).subscribe(
                        data => {
                            this.itineraryList = this.itineraryList.filter(el => {
                                return el.id !== itinerary.id;
                            });

                            this.notification.showSuccessPopup('msg.itineraryDeleteSuccessfullyNotification', 'label.itinerary');
                            this._gaService.emitAction('Delete Itinerary', GAEventCategory.Consignment);

                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.itinerary');
                        });
                }
            });
        this._subscriptions.push(sub);
    }

    openItineraryPopup(itinerary) {
        this.itineraryFormMode = 'view';
        this.itineraryFormOpened = true;
        this.itineraryDetails = itinerary;
        this.itineraryDetails.departureDates = this.itineraryDetails.etdDate;
    }

    onItineraryAdded(itinerary) {
        this.itineraryFormOpened = false;
        const affiliates = this.currentUser.affiliates;

        const sub = this.service.createItinerary(this.model.id, DateHelper.formatDate(itinerary), affiliates).subscribe(
            data => {
                this._gaService.emitAction('Add Itinerary', GAEventCategory.Consignment);
                this.notification.showSuccessPopup('save.sucessNotification', 'label.itinerary');
                this.ngOnInit();
                this.getItineraryTab();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.itinerary');
            });
        this._subscriptions.push(sub);
    }

    onItineraryEdited(itinerary) {
        this.itineraryFormOpened = false;
        itinerary.consignmentId = this.model.id;
        const affiliates = this.currentUser.affiliates;
        const sub = this.service.updateItinerary(this.model.id, itinerary.id, DateHelper.formatDate(itinerary), affiliates).subscribe(
            data => {
                this._gaService.emitAction('Edit Itinerary', GAEventCategory.Consignment);
                this.notification.showSuccessPopup('save.sucessNotification', 'label.itinerary');
                let clonedItineraries = [...this.itineraryList];
                clonedItineraries.forEach((el, i) => {
                    if (el.id === itinerary.id) {
                        clonedItineraries[i] = data;
                    }
                });
                this.itineraryList = clonedItineraries;

                this.ngOnInit();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.itinerary');
            });
        this._subscriptions.push(sub);
    }

    onItineraryFormClosed() {
        this.itineraryFormOpened = false;
    }

    editConsignment() {
        this.router.navigate([`/consignments/edit/${this.model.id}`]);
    }

    deleteConsignment() {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteConsignmentConfirm', 'label.consignment');

        let sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    sub = this.service.moveToTrash(this.model.id).subscribe(
                        data => {
                            this._gaService.emitAction('Delete', GAEventCategory.Consignment);
                            this.notification.showSuccessPopup('msg.deleteConsignmentSuccessfully', 'label.consignment');
                            this.backList();
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.consignment');
                        });
                        this._subscriptions.push(sub);
                }
            });
        this._subscriptions.push(sub);
    }

    cancelEditingConsignment() {
        const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.consignment');

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.router.navigate([`/consignments/view/${this.model.id}`]);
                    this.ngOnInit();
                }
            });
        this._subscriptions.push(sub);
    }

    saveConsignment() {
        if (this.mainForm.valid) {
            this.updateModel = {
                id: this.model.id,
                sequence: this.model.sequence,
                consignmentDate: this.model.consignmentDate,
                confirmedDate: this.model.confirmedDate,
                agentReferenceNumber: this.model.agentReferenceNumber,
                consignmentHouseBL: this.model.consignmentHouseBL,
                consignmentMasterBL: this.model.consignmentMasterBL,
                serviceType: this.model.serviceType,
                modeOfTransport: this.model.modeOfTransport,
                executionAgentId: this.model.executionAgentId,
                shipFrom: this.model.shipFrom,
                shipTo: this.model.shipTo,
                shipFromETDDate: this.model.shipFromETDDate,
                shipToETADate: this.model.shipToETADate,
                consignmentType: this.model.consignmentType
            };

            const sub = this.service.update(this.model.id, DateHelper.formatDate(this.updateModel)).subscribe(
                data => {
                    this._gaService.emitAction('Edit', GAEventCategory.Consignment);
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.consignment');
                    this.router.navigate([`/consignments/view/${this.model.id}`]);
                    this.ngOnInit();
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.consignment');
                });
            this._subscriptions.push(sub);
        } else {
            this.validateAllFields(false);
        }
    }

    createConsolidation() {
        //Not allow to consolidate when Booking is not confirmed yet
        if (!this.model.shipment?.isItineraryConfirmed) {
            this.notification.showInfoDialog(
                'msg.confirmItineraryBeforeConsolidating',
                'label.consignment'
            );
            return;
        }

        this.router.navigate(['/consolidations/add/0'], { queryParams: {'selectedconsignment': this.modelId }});
    }

    executionAgentFilterChange(value: string) {
        this.filteredExecutionAgentOptions = [];
        if (value.length >= 3) {
            this.filteredExecutionAgentOptions = this.executionAgentOptions.filter((s) => s.label.toLowerCase()
                .indexOf(value.toLowerCase()) !== -1);
        }
    }

    executionAgentValueChange(value) {
        if (value.length <= 0) {
            return;
        }
        this.setValidControl('executionAgent');
        this.model.executionAgentId = null;
        const executionAgentOrganization = this.executionAgentOptions.find(x => x.label.toLowerCase() === value.toLowerCase());
        if (executionAgentOrganization) {
            this.model.executionAgentId = executionAgentOrganization.id;
            this.model.executionAgentName = executionAgentOrganization.label;
            this.executionAgentName = executionAgentOrganization.name;

            // Cannot add multiple consignments for the same Execution Agent
            if (!this.isValidExecutionAgent(value)) {
                this.setInvalidControl('executionAgent', 'alreadyUsed');
            }
        } else {
            this.setInvalidControl('executionAgent', 'required');
        }
    }

    locationFilterChange(value) {
        this.filteredLocationOptions = [];
        if (value.length >= 3) {
            this.filteredLocationOptions = this.allLocationOptions.filter((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    /** Return false if the Execution Agent has already been used */

    private isValidExecutionAgent(agentName: string) {
        if (StringHelper.isNullOrEmpty(agentName)) {
            return true;
        }
        const selectedExecutionAgents = this.model.shipment.consignments?.filter(x => x.id != this.modelId)
            .map(x => x.executionAgentId);
        return !selectedExecutionAgents?.includes(this.model.executionAgentId);

    }

    private canDeleteItinerary(itineraryId: number): boolean {
        const currentItinerary = this.itineraryList.find(el => el.id === itineraryId);

        /* Not allow to delete itinerary if it is imported from API
        */
        if (currentItinerary.isImportFromApi) {
            return false;
        }

        /* Not allow to delete Itinerary if Shipment is already linked to Master BL.
        */
        if (this.model.shipment?.masterBillNos?.length > 0) {
            return false;
        }

        const otherSequences = this.itineraryList?.filter(i => i.id !== itineraryId)?.map(i => i.sequence) ?? [];

        return !otherSequences.some(el => el > currentItinerary.sequence);
    }

    public get isInactiveConsignment(): boolean {
        return StringHelper.caseIgnoredCompare(this.model.status, ConsignmentStatus.Inactive);
    }

    get isDisabledAddItineraryButton() {
        if (this.model?.shipment) {
            const isAirShipment = StringHelper.caseIgnoredCompare(this.model.shipment.modeOfTransport, ModeOfTransportType.Air)
            const hasHAWB = this.model.shipment.billOfLadingNos?.length > 0 || false;
            // If the AIR Shipment does not link to HAWB# yet
            if (isAirShipment && !hasHAWB) {
                return true;
            }
        }
        return false;
    }

    seeMoreCargoDescription(description: string) {
        this.openCargoDescriptionDetailPopup = true;
        this.cargoDescriptionDetail = description;
    }
    onCargoDescriptionDetailPopupClosed() {
        this.openCargoDescriptionDetailPopup = false;
    }

    ngAfterViewChecked(): void {
        this.cdr.detectChanges();
        // need to call method on FormComponent base to do further processing
        super.ngAfterViewChecked();
    }
}
