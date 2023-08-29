import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { DropDowns, PackageDimensionUnitType, FormComponent, UserContextService, StringHelper, EquipmentType } from 'src/app/core';
import { extend } from 'webdriver-js-extender';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { POFulfillmentFormService } from '../po-fulfillment-form/po-fulfillment-form.service';
import { TranslateService } from '@ngx-translate/core';
import { POFulfillmentLoadDetailModel, POFulfillmentLoadDetailFormModel } from './po-fulfillment-load-detail-form.model';

@Component({
    selector: 'app-po-fulfillment-load-detail-form',
    templateUrl: './po-fulfillment-load-detail-form.component.html',
    styleUrls: ['./po-fulfillment-load-detail-form.component.scss']
})
export class POFulfillmentLoadDetailFormComponent extends FormComponent {
    @Input() public loadDetailFormOpened: boolean;
    @Input() public model: POFulfillmentLoadDetailFormModel;
    @Input() public isEditing: boolean;

    @Output() closed: EventEmitter<any> = new EventEmitter<any>();
    @Output() edited: EventEmitter<any> = new EventEmitter<any>();

    packageDimensionUnitType = DropDowns.PackageDimensionUnitStringType;
    equipmentTypes = EquipmentType;
    validationRules = {
        loadSequence: {
            required: 'label.loadSequence',
            greaterThan: 'label.zero'
        },
        loadedPackage: {
            required: 'label.loadedPackage'
        },
        loadedQty: {
            required: 'label.loadedQty',
            greaterThan: 'label.zero'
        },
        volume: {
            required: 'label.volume',
            greaterThan: 'label.zero'
        },
        grossWeight: {
            required: 'label.grossWeight',
            greaterThan: 'label.zero'
        },
        netWeight: {
            mustNotGreaterThan : 'label.grossWeight'
        }
    };
    readonly DEFAULT_PACKAGE_UOM = 'Carton';

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: POFulfillmentFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);
    }

    /**Override here to force validate all fields on the form WITHOUT Dirty checking */
    onValueChanged(data?: any) {
        this.validateAllFields(false);
        if (this.mainForm?.controls['loadedPackage'] && !this.formErrors['loadedPackage_custom']) {
            this.validatePackageQuantity();
        }
    }

    onCancelClick() {
        // reset custom form errors
        delete this.formErrors['custom_packageQuantity'];

        this.closed.emit();
    }

    onSaveClick() {
        this.validateAllFields(false);
        if (!this.isFormValid) {
            return;
        }

        if (!this.mainForm.valid) {
            return;
        }

        if (this.isEditing) {
            this.edited.emit(this.model);
        }
    }

    onHeightChange(value) {
        if (value < 0) {
            this.model.height = 1;
        }
        this.calculateVolume();
    }

    onWidthChange(value) {
        if (value < 0) {
            this.model.width = 1;
        }
        this.calculateVolume();
    }

    onLengthChange(value) {
        if (value < 0) {
            this.model.length = 1;
        }
        this.calculateVolume();
    }

    onDimensionUnitChange(value) {
        this.calculateVolume();
    }

    onNetWeightChange(value) {
        if (value < 0) {
            this.model.netWeight = 1;
        }
        this.checkNetWeightValidate();
    }

    onGrossWeightChange(value) {
        if (value < 0) {
            this.model.grossWeight = 1;
        }
        this.checkNetWeightValidate();
    }

    onPackageQuantityChange() {
        this.validatePackageQuantity();
    }

    validatePackageQuantity() {
        /* Temporary comment for PSP-2733

        if (!StringHelper.isNullOrEmpty(this.model.packageQuantity) &&
            this.model.packageQuantity > this.model.customerPO.openQty) {
            this.formErrors['custom_packageQuantity'] = this.translateService.instant('validation.mustNotGreaterThan',
            {
                currentFieldName: this.translateService.instant('label.value'),
                fieldName: this.translateService.instant('label.remainingPkg')
            });
        } else {
            delete this.formErrors['custom_packageQuantity'];
        }
        */
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

    private get isFormValid() {
        // To check default validations
        if (!this.mainForm || !this.mainForm.valid) {
            return false;
        }
        // To check custom validation
        for (const key in this.formErrors) {
            if (!StringHelper.isNullOrEmpty(Reflect.get(this.formErrors, key))) {
                return false;
            }
        }
        return true;
    }

    private calculateVolume() {
        if (this.model.dimensionUnit === PackageDimensionUnitType.CM) {
            this.model.volume = (this.model.length * this.model.width * this.model.height) / 1000000 * this.model.packageQuantity;
            this.model.volume = +this.model.volume.toFixed(3);
        } else {
            this.model.volume = (this.model.length * this.model.width * this.model.height) * 0.0005787 * this.model.packageQuantity;
            this.model.volume = +this.model.volume.toFixed(3);
        }

        // To fix issue on GUI minError
        if (Number.isNaN(this.model.volume)) {
            this.model.volume = 0;
        }

    }
}
