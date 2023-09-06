import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { DropDowns } from 'src/app/core/models/dropDowns/dropDowns';
import { FormComponent } from 'src/app/core/form';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { POFulfillmentFormService } from '../po-fulfillment-form/po-fulfillment-form.service';
import { TranslateService } from '@ngx-translate/core';
import { UserContextService } from 'src/app/core/auth';
import { PackageDimensionUnitType } from 'src/app/core/models/enums/enums';
import { StringHelper } from 'src/app/core';

@Component({
    selector: 'app-po-fulfillment-cargo-detail-form',
    templateUrl: './po-fulfillment-cargo-detail-form.component.html',
    styleUrls: ['./po-fulfillment-cargo-detail-form.component.scss']
})
export class POFulfillmentCargoDetailFormComponent extends FormComponent implements OnInit {
    @Input() public cargoDetailFormOpened: boolean;
    @Input() public model: any;
    @Input() public loadReferenceNumberOptions: [];
    @Input() public isEditing: boolean;
    @Input() public isViewing: boolean;

    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();

    packageUomType = DropDowns.PackageUOMStringType;
    unitUomType = DropDowns.UnitUOMStringType;
    packageDimensionUnitType = DropDowns.PackageDimensionUnitStringType;
    commodityOptions = DropDowns.CommodityString;
    validationRules = {
        loadReferenceNumber: {
            required: 'label.loadRefNo'
        },
        packageQuantity: {
            required: 'label.packageQty'
        },
        packageUomType: {
            required: 'label.packageUOM'
        },
        height: {
            required: 'label.height'
        },
        width: {
            required: 'label.width'
        },
        length: {
            required: 'label.length'
        },
        dimensionUnitType: {
            required: 'label.unit'
        },
        unitQuantity: {
            required: 'label.totalUnitQty'
        },
        unitUomType: {
            required: 'label.unitUOM'
        },
        volume: {
            required: 'label.totalVolume'
        },
        grossWeight: {
            required: 'label.totalGrossWeight'
        },
        netWeight: {
            required: 'label.totalNetWeight',
            mustNotGreaterThan: 'label.grossWeight'
        }
    };

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: POFulfillmentFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);
    }

    ngOnInit() {
    }

    onCargoDetailFormClose() {
        this.close.emit();
    }

    onAddClick() {
        this.validateAllFields(false);
        if (this.mainForm.valid) {
            this.toFixedNumber();
            this.add.emit(this.model);
        }
    }

    onEditClick() {
        if (this.mainForm.valid) {
            this.toFixedNumber();
            this.edit.emit(this.model);
        }
    }

    toFixedNumber() {
        this.model.volume = parseFloat(this.model.volume.toFixed(3));
        this.model.grossWeight = parseFloat(this.model.grossWeight.toFixed(2));
        this.model.netWeight = parseFloat(this.model.netWeight.toFixed(2));
    }

    onPackageQuantityChange() {
        this.calculateVolume();
    }

    onHeightChange() {
        this.calculateVolume();
    }

    onWidthChange() {
        this.calculateVolume();
    }

    onLengthChange() {
        this.calculateVolume();
    }

    onPackageDemensionUnitChange() {
        this.calculateVolume();
    }

    onGrossWeightChange(event: any) {
        this.checkNetWeightValidate();
    }

    onNetWeightChange(event: any) {
        this.checkNetWeightValidate();
    }

    checkNetWeightValidate() {
        if (StringHelper.isNullOrEmpty(this.model.netWeight)) {
            return;
        } else {
            if (!StringHelper.isNullOrEmpty(this.model.grossWeight)
                && this.model.grossWeight < this.model.netWeight) {
                this.setInvalidControl('netWeight', 'mustNotGreaterThan');
            } else {
                this.setValidControl('netWeight');
            }
        }
    }

    private calculateVolume() {
        if (+this.model.dimensionUnit === PackageDimensionUnitType.CM) {
            this.model.volume = parseFloat(((this.model.length * this.model.width * this.model.height) / 1000000 * this.model.packageQuantity).toFixed(2));
        } else {
            this.model.volume = parseFloat(((this.model.length * this.model.width * this.model.height) * 0.0005787 * this.model.packageQuantity).toFixed(2));
        }
    }
}
