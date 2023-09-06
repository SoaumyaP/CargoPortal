import {
    Component,
    OnInit,
    Input,
    OnChanges,
    OnDestroy,
    SimpleChanges
} from '@angular/core';
import {
    DropDowns,
    UserContextService,
    StringHelper,
} from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { OrganizationNameRole, EquipmentType, ModeOfTransport, Movement } from 'src/app/core/models/enums/enums';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { filter } from 'rxjs/operators';
import * as appConstants from 'src/app/core/models/constants/app-constants';
import { CommonService } from 'src/app/core/services/common.service';
import { ControlContainer, NgForm } from '@angular/forms';
import { RoutingOrderFormService } from '../routing-order-form/routing-order-form.service';
import { RoutingOrderModel } from 'src/app/core/models/routing-order.model';

@Component({
    selector: 'app-routing-order-general',
    templateUrl: './routing-order-general.component.html',
    styleUrls: ['./routing-order-general.component.scss'],
    viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class RoutingOrderGeneralComponent implements OnInit, OnChanges, OnDestroy {
    StringHelper = StringHelper;
    ModeOfTransport = ModeOfTransport;
    @Input() parentIntegration$: Subject<IntegrationData>;
    @Input() model: RoutingOrderModel;
    @Input() formErrors: any;
    @Input() isViewMode: boolean;
    @Input() isEditMode: boolean;
    @Input() isAddMode: boolean;
    @Input() allLocationOptions: any;
    @Input() validationRules: any;

    // It is prefix for formErrors and validationRules
    // Use it to detect what tab contains invalid data
    @Input() tabPrefix: string;

    public defaultDropDownItem: { text: string, label: string, description: string, value: number } =
        {
            text: 'label.select',
            label: 'label.select',
            description: 'select',
            value: null
        };

    modeOfTransportOptions = DropDowns.ModeOfTransport.filter(c => c.value === ModeOfTransport.Sea || c.value === ModeOfTransport.Air);
    movementTypeOptions = DropDowns.CustomMovementType;
    incotermTypeOptions = DropDowns.IncotermType;
    mapLocationOptions = [];
    vesselsFiltered: Array<any> = [];
    vesselsSource: Array<any> = [];
    allCarrierOptions: any;
    carrierOptions = [];
    equipmentTypes = EquipmentType;
    locationOptions: any;

    _formValidationKeys = {
        shipFromName: 'shipFromName',
        shipToName: 'shipToName',
        carrier: 'carrier',
        cargoReadyDate: 'cargoReadyDate',
        expectedShipDate: 'expectedShipDate',
        expectedDeliveryDate: 'expectedDeliveryDate'
    };

    // default values are for Sea
    literalLabels = {
        'shipFrom': 'label.shipFrom',
        'shipTo': 'label.shipTo'
    };

    selectedCarrierName: string = null;

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    constructor(
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: RoutingOrderFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private commonService: CommonService
    ) { }

    ngOnInit(): void {
        this.commonService.searchRealActiveVessels("").subscribe(
            r => {
                this.vesselsSource = r;
                this.vesselsFiltered = r;
            }
        )

        this.updateCarrierValue();
    }


    get logisticServiceTypeOptions() {
        if (this.model?.modeOfTransport === ModeOfTransport.Air) {
            return DropDowns.AirLogisticServiceType;
        }

        return DropDowns.LogisticsServiceType;
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes.saveAsDraft?.currentValue === false) {
            this.validationRules[this.tabPrefix + 'modeOfTransport'] = {
                'required': 'label.modeOfTransport'
            };
            this.validationRules[this.tabPrefix + 'incoterm'] = {
                'required': 'label.incoterm'
            };
            this.validationRules[this.tabPrefix + 'logisticsService'] = {
                'required': 'label.logisticsServiceType'
            };
            this.validationRules[this.tabPrefix + 'movementType'] = {
                'required': 'label.movementType'
            };
            this.validationRules[this.tabPrefix + 'shipFromName'] = {
                'required': 'label.shipFrom'
            };
            this.validationRules[this.tabPrefix + 'shipToName'] = {
                'required': 'label.shipTo'
            };
            this.validationRules[this.tabPrefix + 'expectedShipDate'] = {
                'required': 'label.expectedShipDates'
            };
            this.validationRules[this.tabPrefix + 'expectedDeliveryDate'] = {
                'required': 'label.expectedDeliveryDates'
            };
            this.validationRules[this.tabPrefix + 'cargoReadyDate'] = {
                'required': 'label.bookingCargoReadyDates'
            };
        }
        if (changes.model?.currentValue.carrierId) {
            // get carrier from Booking
            if (!StringHelper.isNullOrEmpty(this.model.carrierId)) {
                const carrier = this.carrierOptions?.find((x) => +x.id === +this.model.carrierId);
                this.selectedCarrierName = carrier && carrier.name;
            }
        }

        this.mapLocationOptions = this.allLocationOptions.map((l) => ({
            id: l.id,
            label: l.locationDescription,
        }));

        this.updateLiteralLabels(this.model?.modeOfTransport);
    }

    updateCarrierValue() {
        this.commonService.getCarriers()
            .map((data) => {
                this.allCarrierOptions = data;
                this.carrierOptions = this.allCarrierOptions;

                // get carrier from Booking
                if (!StringHelper.isNullOrEmpty(this.model.carrierId)) {
                    const carrier = this.carrierOptions.find((x) => +x.id === +this.model.carrierId);
                    this.selectedCarrierName = carrier && carrier.name;
                }
            }).subscribe();
    }

    get supplierName() {
        const supplier = this.model.contacts.find(
            (c) => c.organizationRole === OrganizationNameRole.Supplier
        );
        return supplier && supplier.companyName;
    }

    onFilterCarrier(value) {
        this.carrierOptions = [];
        if (value?.length >= 3) {
            this.carrierOptions = this.allCarrierOptions.filter(
                (s) => s.name.toLowerCase().indexOf(value.toLowerCase()) !== -1
                    && s.modeOfTransport === this.model.modeOfTransport
            );
        }
    }

    updateLiteralLabels(mode) {
        switch (mode) {
            case ModeOfTransport.Sea:
                this.literalLabels.shipFrom = 'label.shipFrom';
                this.literalLabels.shipTo = 'label.shipTo';
                break;
            case ModeOfTransport.Air:
                this.literalLabels.shipFrom = 'label.origin';
                this.literalLabels.shipTo = 'label.destination';
                break;
            default:
                this.literalLabels.shipFrom = 'label.shipFrom';
                this.literalLabels.shipTo = 'label.shipTo';
                break;
        }
    }


    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
