import { Component, ElementRef, HostListener, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { forkJoin, Observable, of, Subject } from 'rxjs';
import { MovementTypes } from 'src/app/core/models/constants/app-constants';
import { ModeOfTransportType, POFulfillmentStageType, EquipmentType, Movement, ModeOfTransport, RoutingOrderStatus, RoutingOrderStageType, EntityType } from 'src/app/core/models/enums/enums';
import { DropdownListModel, FormComponent, StringHelper } from 'src/app/core';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { RoutingOrderFormService } from './routing-order-form.service';
import { UserOrganizationProfileModel } from 'src/app/core/models/organization.model';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { RoutingOrderModel, RoutingOrderNoteModel, RoutingOrderTabModel } from 'src/app/core/models/routing-order.model';
import { ActivityMilestone } from 'src/app/ui/activity-timeline/activity-timeline.service';
import { map } from 'rxjs/operators';

@Component({
    selector: 'app-routing-order-form',
    templateUrl: './routing-order-form.component.html',
    styleUrls: ['./routing-order-form.component.scss']
})

export class RoutingOrderFormComponent extends FormComponent {
    modelName = "routingOrders";
    entityType: EntityType = EntityType.RoutingOrder;

    integration$: Subject<IntegrationData> = new Subject();
    allLocationOptions = [];
    RoutingOrderStatus = RoutingOrderStatus;
    EquipmentType = EquipmentType;
    MovementTypes = MovementTypes;
    ModeOfTransportType = ModeOfTransportType;

    tabs: Array<RoutingOrderTabModel> = [
        {
            text: 'label.general',
            sectionId: 'general',
            selected: false,
            readonly: false
        },
        {
            text: 'label.contact',
            sectionId: 'contact',
            selected: false,
            readonly: false
        },
        {
            text: 'label.cargoDetails',
            sectionId: 'cargoDetails',
            selected: false,
            readonly: false
        },
        {
            text: 'label.fulfillment',
            sectionId: 'booking',
            selected: false,
            readonly: false
        },
        {
            text: 'label.activity',
            sectionId: 'activity',
            selected: false,
            readonly: true
        },
        {
            text: 'label.dialog',
            sectionId: 'dialog',
            selected: false,
            readonly: false
        }
    ];

    milestones = [
        {
            stage: RoutingOrderStageType.Released,
            title: 'label.released',
            class: 'n-released',
            active: false,
            current: false
        },
        // {
        //     stage: RoutingOrderStageType.RateAccepted,
        //     title: 'label.rateAccepted',
        //     class: 'n-rateAccepted',
        //     active: false,
        //     current: false
        // },
        // {
        //     stage: RoutingOrderStageType.RateConfirmed,
        //     title: 'label.rateConfirmed',
        //     class: 'n-rateConfirmed',
        //     active: false,
        //     current: false
        // },
        {
            stage: RoutingOrderStageType.ForwarderBookingRequest,
            title: 'label.forwarderBookingRequest',
            class: 'n-forwarderBookingRequest',
            active: false,
            current: false
        },
        {
            stage: RoutingOrderStageType.ForwarderBookingConfirmed,
            title: 'label.forwarderBookingConfirmed',
            class: 'n-forwarderBookingConfirmed',
            active: false,
            current: false
        },
        {
            stage: RoutingOrderStageType.ShipmentDispatch,
            title: 'label.shipmentDispatch',
            class: 'n-shipmentDispatch',
            active: false,
            current: false
        },
        {
            stage: RoutingOrderStageType.Closed,
            title: 'label.closed',
            class: 'n-closed',
            active: false,
            current: false
        }
    ];

    actMilestones: ActivityMilestone[] = [
        {
            activityCode: '1071',
            milestone: 'label.closed',
            occurDate: null,
            hasLinked: false
        },
        {
            activityCode: '7001',
            milestone: 'label.shipmentDispatch',
            occurDate: null,
            hasLinked: false
        },
        {
            activityCode: '1061',
            milestone: 'label.forwarderBookingConfirmed',
            occurDate: null,
            hasLinked: false
        },
        {
            activityCode: '1051',
            milestone: 'label.forwarderBookingRequest',
            occurDate: null,
            hasLinked: false
        }
        // {
        //     activityCode: '0',
        //     milestone: 'label.rateConfirmed',
        //     occurDate: null,
        //     hasLinked: false
        // },
        // {
        //     activityCode: '0',
        //     milestone: 'label.rateAccepted',
        //     occurDate: null,
        //     hasLinked: false
        // }
    ];

    filterActOptions: DropdownListModel<string>[] = [
        {
            label: 'label.fulfillmentNumber',
            value: 'BookingNo'
        },
        {
            label: 'label.shipmentNo',
            value: 'ShipmentNo'
        },
        {
            label: 'label.containerNo',
            value: 'ContainerNo'
        },
        {
            label: 'label.vesselName',
            value: 'VesselName'
        }
    ]

    private isManualScroll: boolean = true;

    model: RoutingOrderModel;
    /**Owner's Organization info of the booking */
    createdByOrganization: UserOrganizationProfileModel;

    // Data for Dialog Tab
    noteList: RoutingOrderNoteModel[];

    stringHelper = StringHelper;

    // Control saving message popup
    saveBookingFailed = false;
    saveBookingErrors: Array<string> = [];

    @ViewChild('headerBar', { static: false }) headerBarElement: ElementRef;
    @ViewChild('stickyBar', { static: false }) stickyBarElement: ElementRef;
    @ViewChild('sectionContainer', { static: false }) sectionContainerElement: ElementRef;
    @ViewChild('general', { static: false }) generalElement: ElementRef;
    @ViewChild('contact', { static: false }) contactElement: ElementRef;
    @ViewChild('cargoDetails', { static: false }) cargoDetailsElement: ElementRef;
    @ViewChild('booking', { static: false }) bookingElement: ElementRef;
    @ViewChild('activity', { static: false }) activityElement: ElementRef;
    @ViewChild('dialog', { static: false }) dialogElement: ElementRef;
    
    constructor(
        protected route: ActivatedRoute,
        public service: RoutingOrderFormService,
        public notification: NotificationPopup,
        public translateService: TranslateService,
        public router: Router,
        private _gaService: GoogleAnalyticsService) {
        super(route, service, notification, translateService, router);
        
        const currentUser = service.currentUser;
        this.currentUser = currentUser;
    }

    // Please use this method to make sure data is already initialized
    getAllLocationOptions(): Observable<any> {
        // not initialized data
        if (this.allLocationOptions.length === 0) {
            return this.service.getAllLocations().map(locations => {
                this.allLocationOptions = locations;
                return this.allLocationOptions;
            });
        } else {
            // after data initialized
            return of(this.allLocationOptions);
        }
    }

    onInitDataLoaded() {
        if (this.model) {
            this.model.statusName = RoutingOrderStatus[this.model.status].toString();

            this.actMilestones.push(
                {
                    activityCode: '',
                    milestone: 'label.released',
                    occurDate: this.model.createdDate,
                    hasLinked: false
                }
            );
        }

        this.getAllLocationOptions().subscribe(
            x => {
                this.model.shipFromName = (this.allLocationOptions.find(x => x.id === this.model.shipFromId))?.locationDescription;
                this.model.shipToName = (this.allLocationOptions.find(x => x.id === this.model.shipToId))?.locationDescription;
            }
        );

        // init default value
        this.formErrors = {};

        setTimeout(() => {
            this.initTabLink();
        }, 500); // make sure UI has been rendered

        this.bindingNoteTab();
        this.service.getOwnerOrgInfo(this.model.createdBy).subscribe(
            response => this.createdByOrganization = response
        )
    }

    /**Link UI element to tabs object
    Must make sure that it is correct order */
    private initTabLink(): void {
        this.tabs.map(tab => {
            switch (tab.sectionId) {
                case 'general':
                    tab.sectionElementRef = this.generalElement;
                    break;
                case 'contact':
                    tab.sectionElementRef = this.contactElement;
                    break;
                case 'cargoDetails':
                    tab.sectionElementRef = this.cargoDetailsElement;
                    break;
                case 'booking':
                    tab.sectionElementRef = this.bookingElement;
                    break;
                case 'activity':
                    tab.sectionElementRef = this.activityElement;
                    break;
                case 'dialog':
                    tab.sectionElementRef = this.dialogElement;
                    break;
            }
        });
    }

    onClickStickyBar(event, tab: RoutingOrderTabModel) {
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

    get isHiddenLoads() {
        if (this.model.modeOfTransport !== ModeOfTransport.Sea) {
            return true;
        }

        if (this.model.movementType !== Movement.CY_CY) {
            return true;
        }

        return false;
    }

    /**
     * Get tab details/ settings
     * @param sectionId Id of section
     * @returns
     */
    getTabDetails(sectionId: string): RoutingOrderTabModel {
        const result = this.tabs.find(x => x.sectionId === sectionId);
        return result;
    }

    onSubmit() {
        return;
    }

    bindingNoteTab() {
        var noteObs$ = this.service.getNotes(this.model.id)
            .pipe(
                map((res: any) => {
                    return res.map(x => {
                        const newOrderNoteModel = new RoutingOrderNoteModel();
                        newOrderNoteModel.MapFrom(x);
                        return newOrderNoteModel;
                    })
                })
            )

        var masterNote$ = this.service.getMasterNotes(this.model.id)
            .pipe(
                map((res: any) => {
                    return res.map(x => {
                        const newOrderNoteModel = new RoutingOrderNoteModel();
                        newOrderNoteModel.MapFromMasterNote(x);
                        return newOrderNoteModel;
                    })
                })
            )

        forkJoin([noteObs$, masterNote$]).subscribe(
            (note) => {
                this.noteList = note[0].concat(note[1]);
            });
    }

    backToList() {
        this.router.navigate(["/routing-orders"]);
    }
}