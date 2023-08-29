import { Component, Input, Output, EventEmitter } from '@angular/core';
import { DropDowns, FormComponent, StringHelper, ViewSettingModuleIdType } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { POFulfillmentFormService } from '../po-fulfillment-form/po-fulfillment-form.service';
import { ContainerHelper } from 'src/app/core/helpers/container-helper';
import { POFulfillmentLoadModel } from '../po-fulfillment-load-detail-form/po-fulfillment-load-detail-form.model';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';

@Component({
    selector: 'app-load-container-popup',
    templateUrl: './load-container-popup.component.html',
    styleUrls: ['./load-container-popup.component.scss']
})
export class LoadContainerPopupComponent extends FormComponent {
    @Input() loadList: any[];
    /**Index of selected load in the list. */
    @Input() index: number;
    @Input() public loadContainerFormOpened: boolean = false;
    @Input() public model: POFulfillmentLoadModel;
    @Input() public shipmentId: any;
    @Input() public heightPopup = 530;
    @Input()
    set loadContainerFormMode(mode: string) {
        this.isViewModeLocal = mode === this.formMode.view;
        this.isEditModeLocal = mode === this.formMode.edit;
    }
    @Input() public allEventOptions: any[];
    @Input() viewSettings: ViewSettingModel[];
    public isViewModeLocal: boolean;
    public isEditModeLocal: boolean;

    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();

    modeOfTransportOptions = DropDowns.ModeOfTransportStringType;
    filteredEventOptions: any[];

    allLocationOptions: any[];
    filteredLocationOptions: any[];

    validationRules = {
        'containerNumber': {
            'required': 'label.containerNo',
            'duplicateContainer': 'validation.duplicateContainer',
            'containerNumberInvalid': 'validation.containerNumberInvalid'
        },
        'sealNumber': {
            'required': 'label.sealNo'
        }
    };

    formHelper = FormHelper;
    viewSettingModuleIdType = ViewSettingModuleIdType;

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: POFulfillmentFormService,
        public translateService: TranslateService) {
        super(route, service, notification, translateService, router);

        this.service.getAllLocations().subscribe(data => {
            this.allLocationOptions = data;
        });
    }

    onFormClosed() {
        this.loadContainerFormOpened = false;
        this.close.emit();
    }

    onEditClick() {
        this.validateAllFields(false);

        if (!this.mainForm.valid) {
            return;
        }
        this.edit.emit(this.model);
    }

    isContainerDuplicate(newContainer) {
        for (let i = 0; i < this.loadList.length; i++) {
            if (this.index !== i) {
                if (this.loadList[i].containerNumber === newContainer) {
                    return true;
                }
            }
        }
        return false;
    }

    checkInvalidContainerNumber() {
        if (StringHelper.isNullOrEmpty(this.model.containerNumber)) {
            this.setInvalidControl('containerNumber', 'required');
        } else if (this.model.containerNumber.length !== 11) {
            this.setInvalidControl('containerNumber', 'containerNumberInvalid');
        } else if (!ContainerHelper.checkDigitContainer(this.model.containerNumber)) {
            this.setInvalidControl('containerNumber', 'containerNumberInvalid');
        }
    }

    onContainerChange(event) {
        if (this.isContainerDuplicate(event)) {
            this.setInvalidControl('containerNumber', 'duplicateContainer');
        }
    }
}
