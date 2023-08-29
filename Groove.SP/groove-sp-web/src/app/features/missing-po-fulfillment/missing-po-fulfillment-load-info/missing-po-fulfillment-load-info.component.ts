import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { UserContextService, DropDowns, POFulfillmentLoadStatus, POFulfillmentStageType, StringHelper, EquipmentType, ValidationDataType, POType, ModeOfTransportType, ViewSettingModuleIdType } from 'src/app/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { MissingPOFulfillmentFormService } from '../missing-po-fulfillment-form/missing-po-fulfillment-form.service';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { TranslateService } from '@ngx-translate/core';
import { faPencilAlt } from '@fortawesome/free-solid-svg-icons';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { Subject, Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import * as appConstants from 'src/app/core/models/constants/app-constants';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
    selector: 'app-missing-po-fulfillment-load-info',
    templateUrl: './missing-po-fulfillment-load-info.component.html',
    styleUrls: ['./missing-po-fulfillment-load-info.component.scss'],
    viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class MissingPOFulfillmentLoadInfoComponent implements OnInit, OnDestroy {

    @Input('data') loadList: any[];
    // It is prefix for formErrors and validationRules
    // Use it to detect what tab contains invalid data
    @Input()
    tabPrefix: string;
    @Input() formErrors: any;

    @Input() validationRules: any;
    @Input() customerPOs: any[];
    @Input() isShowCargoDetailsTab = false;
    @Input() cargoList: any[];
    @Input()
    isEditable: boolean;
    @Input()
    isViewMode: boolean;
    @Input()
    isEditMode: boolean;
    @Input()
    stage: POFulfillmentStageType;
    @Input()
    modeOfTransport: string;
    @Input()
    orders: any;
    @Input()
    viewSettings: any;
    @Input()
    movementType: string;
    @Input()
    parentIntegration$: Subject<IntegrationData>;
    @Input()
    canUpdateContainer = false;

    /**Data from 2 sources:
     * 1. Add mode: PO.pOType
     * 2. Edit mode: POFF.fulfilledFromPOType
     * */
    @Input()
    poType: POType;

    faPencilAlt = faPencilAlt;

    equipmentTypes = EquipmentType;
    equipmentTypeOptions = DropDowns.EquipmentStringType;
    filteredEquipmentTypeOptions: any[];
    POFulfillmentLoadStatus = POFulfillmentLoadStatus;
    poFulfillmentLoadStatusOptions = DropDowns.POFulfillmentLoadStatus;
    packageUOMOptions = DropDowns.PackageUOMStringType;

    viewSettingModuleIdType = ViewSettingModuleIdType;
    
    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];
    // Store all validation results (business, input,...), then should return to validate
    private _validationResults: Array<ValidationData> = [];

    constructor(
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: MissingPOFulfillmentFormService,
        private userContext: UserContextService,
        public translateService: TranslateService) {
        const sub = this.userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
            }
        });
        this._subscriptions.push(sub);
    }


    ngOnInit(): void {
        this._handleModeOfTransportChanged();
        this._registerEventHandlers();
        this._registerContainerTypeChangedHandler();
        this._registerPOFFOrderChangedHandler();
    }

    private _registerEventHandlers() {
        // Handler for value changing on Mode of Transport and Movement Type
        // fired from tab General po-fulfillment-general-info
        let sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]modeOfTransportValueChanged'
            )).subscribe((eventContent: IntegrationData) => {
                this.modeOfTransport = eventContent.content['modeOfTransport'];
                this._handleModeOfTransportChanged();
            });
        this._subscriptions.push(sub);

        // Handler for value changing on poType
        sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-form]poTypeChanged'
            )).subscribe((eventContent: IntegrationData) => {
                this.poType = eventContent.content['poType'];
                this._handlePOTypeChanged(this.poType);
            });
        this._subscriptions.push(sub);

        sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-customer]lastCustomerPOEdited'
            )).subscribe((eventContent: IntegrationData) => {
                if (this._filterLoadList.length >= 1) {
                    /* clone the first load then push it to keep package value
                    */
                    const firstLoadItem = {...this._filterLoadList[0]};
                    for (let index = 0; index < this.loadList.length; index++) {
                        if (!this.loadList[index].removed) {
                            this.onDeleteLoad(index);
                        }
                    }
                    this.loadList.push(firstLoadItem);
                    this.populateFirstLoadItem(true);
                    this.validateGrid(this.loadList.length - 1);
                } else {
                    this.addBlankLoad();
                } 
            });
        this._subscriptions.push(sub);
    }

    private _registerContainerTypeChangedHandler() {
        // Handler for value changing on Container Type value
        // fired from [po-fulfillment-customer] and [po-fulfillment-form]
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-form]containerTypeChanged' ||
                eventContent.name === '[po-fulfillment-customer]containerTypeChanged' ||
                eventContent.name === '[po-fulfillment-general-info]containerTypeChanged'
            )).subscribe((eventContent: IntegrationData) => {
                if (!StringHelper.isNullOrEmpty(eventContent.content['containerType'])) {
                    if (this._filterLoadList.length >= 1) {
                        /* clone the first load then push it to keep package value
                        */
                        const firstLoadItem = {...this._filterLoadList[0]};
                        for (let index = 0; index < this.loadList.length; index++) {
                            if (!this.loadList[index].removed) {
                                this.onDeleteLoad(index);
                            }
                        }
                        this.loadList.push(firstLoadItem);
                        this.populateFirstLoadItem();
                        this.validateGrid(this.loadList.length - 1);
                    } else {
                        this.addBlankLoad();
                    }
                }
            });
        this._subscriptions.push(sub);
    }

    _registerPOFFOrderChangedHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-customer]poffOrderChanged'
            )).subscribe((eventContent: IntegrationData) => {
               this.populateFirstLoadItem(true);
               this.validateGrid(this.loadList.length - 1);
            });
        this._subscriptions.push(sub);
    }

    _handlePOTypeChanged(poType: POType) {
        this._handleModeOfTransportChanged();
    }


    _handleModeOfTransportChanged() {
        this.filteredEquipmentTypeOptions = [];
        if (!StringHelper.isNullOrEmpty(this.modeOfTransport)) {
            if (this.modeOfTransport === ModeOfTransportType.Air) {
                this.filteredEquipmentTypeOptions = DropDowns.EquipmentStringType.filter(x => x.value === this.equipmentTypes.AirShipment);
            } else if (this.modeOfTransport === ModeOfTransportType.Road) {
                this.filteredEquipmentTypeOptions = DropDowns.EquipmentStringType.filter(x => x.value === this.equipmentTypes.Truck);
            } else if (this.modeOfTransport === ModeOfTransportType.Sea) {
                // If poType is Blanket + modeOfTransport is Sea, Equipment Type can only be "LCL"
                if (this.poType === POType.Blanket) {
                    this.filteredEquipmentTypeOptions = DropDowns.EquipmentStringType.filter(x => x.value === this.equipmentTypes.LCLShipment);
                } else {
                this.filteredEquipmentTypeOptions = DropDowns.EquipmentStringType.filter(x =>
                        [this.equipmentTypes.LCLShipment, this.equipmentTypes.TwentyGP,
                        this.equipmentTypes.FourtyGP, this.equipmentTypes.TwentyRF,
                        this.equipmentTypes.FourtyRF, this.equipmentTypes.TwentyNOR,
                        this.equipmentTypes.FourtyNOR, this.equipmentTypes.TwentyHC,
                        this.equipmentTypes.FourtyHC, this.equipmentTypes.FourtyFiveHC].includes(x.value));
                }
            } else {
                this.filteredEquipmentTypeOptions = this.equipmentTypeOptions;
            }
        }
        for (let index = 0; index < this.loadList.length; index++) {
            const equipmentType = this.loadList[index].equipmentType;
            if (this.filteredEquipmentTypeOptions.map(x => x.value).indexOf(equipmentType) === -1
                && this.loadList[index].isAddLine && !this.loadList[index].removed) {
                this.loadList[index].equipmentType = null;
            }
        }
    }

    labelFromEnum(arr, value) {
        return EnumHelper.convertToLabel(arr, value);
    }

    loadedPackageQty(loadReferenceNumber) {
        if (this.isShowCargoDetailsTab) {
            return this.cargoList.filter(item => item.loadReferenceNumber === loadReferenceNumber)
                .reduce((sum, current) => sum += current.packageQuantity, 0);
        } else {
            return this.loadList.filter(item => item.loadReferenceNumber === loadReferenceNumber)
                .reduce((sum, current) => sum += current.subtotalPackageQuantity, 0);
        }
    }

    addBlankLoad() {
        if (this.loadList != null) {
            // As adding the first row, then auto sum-up Volume, Gross Weight, Net Weight, Packaged Qty
            const newItem: any = {
                isAddLine: true,
                status: POFulfillmentLoadStatus.Active
            };
            this.loadList.push(newItem);
            this.populateFirstLoadItem();
            this.validateGrid(this.loadList.length - 1);
        }
    }

    /**Populate data for the first load item
     * by calculating total Volume/Gross Weight/Net weight from the Booking Orders */
    private populateFirstLoadItem(
        keepContainerType: boolean = false
    ): void {
        if (this._filterLoadList.length === 1) {
            const loadItem = this._filterLoadList[0];
            const currentPOList = this.customerPOs;

            if (!keepContainerType) {

                const containerTypes = [...new Set(this.orders.map(x => x.poContainerType)
                    .filter(x => !StringHelper.isNullOrEmpty(x)))];
                    
                if (containerTypes && containerTypes.length === 1) {
                    loadItem.equipmentType = this.poType === POType.Blanket ? EquipmentType.LCLShipment :  containerTypes[0];
                    const otherEquipmentTypes = [
                        this.equipmentTypes.AirShipment, this.equipmentTypes.Truck
                    ];
                    if (loadItem.equipmentType === this.equipmentTypes.LCLShipment) {
                        this.movementType = appConstants.MovementTypes.CFSUnderscoreCY;
                    } else if (appConstants.CYEquipmentTypes.indexOf(loadItem.equipmentType) !== -1) {
                        this.movementType = appConstants.MovementTypes.CYUnderscoreCY;
                    } else if (otherEquipmentTypes.indexOf(loadItem.equipmentType) !== -1) {
                        this.movementType = null;
                    }
                    // Notify subscribers about the change.
                    const emitValue: IntegrationData = {
                        name: '[po-fulfillment-load-info]equipmentTypeValueChanged',
                        content: {
                            'movementType': this.movementType
                        }
                    };
                    this.parentIntegration$.next(emitValue);
                }
            }

            let tmpSubTotal = 0;
            currentPOList.filter(x => x.volume && x.volume > 0).map(x => tmpSubTotal += x.volume);
            loadItem.plannedVolume = tmpSubTotal;

            tmpSubTotal = 0;
            currentPOList.filter(x => x.grossWeight && x.grossWeight > 0).map(x => tmpSubTotal += x.grossWeight);
            loadItem.plannedGrossWeight = tmpSubTotal;

            tmpSubTotal = 0;
            const netWeightValues = currentPOList.filter(x => !StringHelper.isNullOrEmpty(x.netWeight));
            if (netWeightValues && netWeightValues.length > 0) {
                currentPOList.filter(x => x.netWeight && x.netWeight > 0).map(x => tmpSubTotal += x.netWeight);
                loadItem.plannedNetWeight = tmpSubTotal;
            } else {
                loadItem.plannedNetWeight = '';
            }

            tmpSubTotal = 0;
            currentPOList.map(x => tmpSubTotal += (x.bookedPackage ?? 0));
            loadItem.plannedPackageQuantity = tmpSubTotal;
        }
    }

    public rowCallback(args) {
        // Deleted row will be marked with removed property.
        return { 'hide-row': args.dataItem.removed };
    }

    private get _filterLoadList() {
        // exclude call row removed
        return this.loadList.filter(x => StringHelper.isNullOrEmpty(x.removed) || !x.removed);
    }

    private get firstRowIndex() {
        for (let index = 0; index < this.loadList.length; index++) {
            if (!this.loadList[index].removed) {
                return index;
            }
        }
    }

    private validateGrid(rowIndex) {
        // Do not validate if row is removed
        if (!StringHelper.isNullOrEmpty(this.loadList[rowIndex]) && !this.loadList[rowIndex].removed) {
            this.validationRules[this.tabPrefix + 'equipmentType_' + rowIndex] = {
                'required': 'label.equipmentType'
            };
            this.validationRules[this.tabPrefix + 'plannedVolumeCBM_' + rowIndex] = {
                'required': 'label.plannedVolumeCBM'
            };
            this.validationRules[this.tabPrefix + 'plannedGrossWeightKGS_' + rowIndex] = {
                'required': 'label.plannedGrossWeightKGS'
            };
            this.validationRules[this.tabPrefix + 'plannedPackageQty_' + rowIndex] = {
                'required': 'label.plannedPackageQty'
            };
            this.validationRules[this.tabPrefix + 'packageUOM_' + rowIndex] = {
                'required': 'label.packageUOM'
            };

            this.onValidateVolume(null, rowIndex);
            this.onValidateGrossWeight(null, rowIndex);
            this.onValidateNetWeight(null, rowIndex);
            this.onValidatePackageQty(null, rowIndex);
        }
    }

    onDeleteLoad(rowIndex: number) {
        // Update properties for current contact row
        const rowData = this.loadList[rowIndex];
        rowData.removed = true;

        // Reset formErrors and validationRules for this row
        this._resetFormErrorsAtRow(rowIndex);
        this._resetValidationRulesAtRow(rowIndex);

        // Call other business
        for (let index = 0; index < this.loadList.length; index++) {
            const load = this.loadList[index];
            if (StringHelper.isNullOrEmpty(load.removed) || !load.removed) {
                this.equipmentTypeChanged(index);
                break;
            }
        }
    }

    private _resetFormErrorsAtRow(rowIndex: number) {
        const formErrorNames = this.keyNamesByIndex(rowIndex);

        Object.keys(this.formErrors)
        .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
        .map(x => {
            delete this.formErrors[x];
        });
    }

    private _resetValidationRulesAtRow(rowIndex: number) {
        const ruleNames = this.keyNamesByIndex(rowIndex);

        Object.keys(this.validationRules)
        .filter(x => ruleNames.some(y => x.startsWith(this.tabPrefix + y)))
        .map(x => {
            delete this.validationRules[x];
        });
    }

    /**Return array of all formError key names */
    private keyNamesByIndex(index: number): Array<string> {
        return [
            `plannedVolumeCBM_${index}`,
            `plannedVolumeCBM_${index}_custom`,
            `plannedGrossWeightKGS_${index}`,
            `plannedGrossWeightKGS_${index}_custom`,
            `plannedNetWeightKGS_${index}`,
            `plannedNetWeightKGS_${index}_custom`,
            `plannedPackageQty_${index}`,
            `plannedPackageQty_${index}_custom`,
            `equipmentType_${index}`,
            `packageUOM_${index}`];
    }

    private _clearLoadRowData(loadData) {
        loadData.equipmentType = -1;
        loadData.packageUOM = -1;
    }

    private _validateAllGridInputs() {
        for (let i = 0; i < this.loadList.length; i++) {
            if (StringHelper.isNullOrEmpty(this.loadList[i].removed) || !this.loadList[i].removed) {
                this.validateGrid(i);
            } else {
                this._resetFormErrorsAtRow(i);
                this._resetValidationRulesAtRow(i);
            }
        }
    }

    onValidateGrossWeight($event, rowIndex) {
        const plannedGrossWeight = this.loadList[rowIndex].plannedGrossWeight;
        const formErrorName = `plannedGrossWeightKGS_${rowIndex}_custom`;
        const fieldName = this.translateService.instant('label.plannedGrossWeightKGS');

        // If failed then return, no need to validate more
        const isValid = this.validateGreaterThenZero(plannedGrossWeight, fieldName, formErrorName);

        if (!isValid) {
            return;
        }
        this.validateGrossNetWeight(rowIndex);

    }

    onValidateVolume($event, rowIndex) {
        const plannedVolume = this.loadList[rowIndex].plannedVolume;
        const formErrorName = `plannedVolumeCBM_${rowIndex}_custom`;

        const fieldName = this.translateService.instant('label.plannedVolumeCBM');

        this.validateGreaterThenZero(plannedVolume, fieldName, formErrorName);
    }

    onValidateNetWeight($event, rowIndex) {
        this.validateGrossNetWeight(rowIndex);
    }

    onValidatePackageQty($event, rowIndex) {
        const plannedPackageQuantity = this.loadList[rowIndex].plannedPackageQuantity;
        const formErrorName = `plannedPackageQty_${rowIndex}_custom`;
        const fieldName = this.translateService.instant('label.plannedPackageQty');

        this.validateGreaterThenZero(plannedPackageQuantity, fieldName, formErrorName);
    }

    private validateGrossNetWeight(rowIndex) {
        // Check Planned Gross Weight (KGS) vs Planned Net Weight (KGS)
        // Only check if both contain values
        const plannedGrossWeight = this.loadList[rowIndex].plannedGrossWeight;
        const plannedNetWeight = this.loadList[rowIndex].plannedNetWeight;
        if (!StringHelper.isNullOrEmpty(plannedGrossWeight) &&
            !StringHelper.isNullOrEmpty(plannedNetWeight) &&
            plannedGrossWeight < plannedNetWeight) {
                this.formErrors[this.tabPrefix + 'plannedNetWeightKGS_' + rowIndex + '_custom'] = this.translateService.instant('validation.mustNotGreaterThan', {
                currentFieldName : StringHelper.toUpperCaseFirstLetter(this.translateService.instant('label.netWeight')),
                fieldName: this.translateService.instant('label.grossWeight').toLowerCase()
            });
            return false;
        } else {
            this.formErrors[this.tabPrefix + 'plannedNetWeightKGS_' + rowIndex + '_custom'] = null;
            return true;
        }
    }

    private validateGreaterThenZero(value: number, fieldName: string, formErrorName: any) {
        const currentValue = value;
        if (!StringHelper.isNullOrEmpty(currentValue) && currentValue <= 0) {
            this.formErrors[this.tabPrefix + formErrorName] = this.translateService.instant('validation.mustGreaterThan', {
                currentFieldName: fieldName,
                fieldName: this.translateService.instant('label.zero').toLowerCase()
            });
            return false;
        } else {
            this.formErrors[this.tabPrefix + formErrorName] = null;
            return true;
        }
    }

    equipmentTypeChanged(rowIndex) {
        if (rowIndex === this.firstRowIndex) {
            const otherEquipmentTypes = [
                this.equipmentTypes.AirShipment, this.equipmentTypes.Truck
            ];
            const equipmentType = this.loadList[rowIndex].equipmentType;
            if (equipmentType === this.equipmentTypes.LCLShipment) {
                this.movementType = appConstants.MovementTypes.CFSUnderscoreCY;
            } else if (appConstants.CYEquipmentTypes.indexOf(equipmentType) !== -1) {
                this.movementType = appConstants.MovementTypes.CYUnderscoreCY;
            } else if (otherEquipmentTypes.indexOf(equipmentType) !== -1) {
                this.movementType = null;
            }
            const emitValue: IntegrationData = {
                name: '[po-fulfillment-load-info]equipmentTypeValueChanged',
                content: {
                    'movementType': this.movementType
                }
            };
            this.parentIntegration$.next(emitValue);
        }
    }

    get isDisableAddMoreButton() {
        if (this.modeOfTransport === ModeOfTransportType.Sea &&
            (this.movementType === 'CFS_CY' || this.movementType === 'CFS_CFS') &&
            this._filterLoadList.map(x => x.equipmentType).filter(x => x === this.equipmentTypes.LCLShipment).length >= 1) {
            return true;
        }
        return false;
    }

    // validate Equipment Types based on <mode of transport> & <movement type>
    _validateEquipmentTypes(): boolean {
        const loadList = this._filterLoadList;
        const result = new ValidationData(ValidationDataType.Business, true);
        if (loadList) {
            let filteredEquipmentTypes = [];
            let isCFSBooking = false;

            if (this.modeOfTransport === ModeOfTransportType.Air) {
                filteredEquipmentTypes = [this.equipmentTypes.AirShipment];
            } else if (this.modeOfTransport === ModeOfTransportType.Road) {
                filteredEquipmentTypes = [this.equipmentTypes.Truck];
            } else if (this.modeOfTransport === ModeOfTransportType.Sea) {
                if (this.movementType === 'CY_CY' || this.movementType === 'CY_CFS') {
                    filteredEquipmentTypes = appConstants.CYEquipmentTypes;
                } else if (this.movementType === 'CFS_CY' || this.movementType === 'CFS_CFS') {
                    isCFSBooking = true;
                    filteredEquipmentTypes = appConstants.CFSEquipmentTypes;
                }
            } else {
                this._validationResults.push(result);
                return;
            }

            for (let index = 0; index < loadList.length; index++) {
                if (!filteredEquipmentTypes.some(val => val === loadList[index].equipmentType)) {
                    result.status = false;
                    result.message = this.translateService.instant('validation.invalidEquipmentType');
                    this._validationResults.push(result);
                    return;
                }
            }

            if (isCFSBooking && loadList.map(x => x.equipmentType).length > 1) {
                result.status = false;
                result.message = this.translateService.instant('validation.moreThanLCLLoad');
                this._validationResults.push(result);
                return;
            }
        }
        this._validationResults.push(result);
    }

    _validateLoadItemData() {
        const result = new ValidationData(ValidationDataType.Business, true);
        const customerPOs = this.customerPOs;
        const loads = this._filterLoadList;

        if ((!StringHelper.isNullOrEmpty(customerPOs) && customerPOs.filter(x => x.purchaseOrderId !== 0).length > 0) && (StringHelper.isNullOrEmpty(loads) || loads.length === 0)) {
            result.status = false;
            result.message = this.translateService.instant('validation.atLeastContainerLoad');
            this._validationResults.push(result);
            return;
        }

        // validate total of Volume, Gross Weight, Net Weight, Package Qty in Customer PO against Load tab
        // validate only if there is any record in Customer PO or Load tabs

        // leftValue is from CustomerPO tab, rightValue is from Load tab
        let leftValue = 0, rightValue = 0;
        if ((customerPOs && customerPOs.length > 0) || (loads && loads.length > 0)) {

            // validate Volume
            leftValue = 0, rightValue = 0;
            customerPOs && customerPOs.length > 0 ? customerPOs.filter(x => x.volume && x.volume > 0).map(x => leftValue += x.volume) : 0;

            loads && loads.length > 0 ? loads.filter(x => x.plannedVolume && x.plannedVolume > 0).map(x => rightValue += x.plannedVolume) : 0;

            // 3 decimal places on Volume
            if (leftValue.toFixed(3) !== rightValue.toFixed(3)) {
                result.status = false;
                result.message = this.translateService.instant('validation.totalVolumeNoMatched');
                this._validationResults.push(result);
                return;
            }

            // validate Gross Weight
            leftValue = 0, rightValue = 0;
            customerPOs && customerPOs.length > 0 ? customerPOs.filter(x => x.grossWeight && x.grossWeight > 0).map(x => leftValue += x.grossWeight) : 0;

            loads && loads.length > 0 ? loads.filter(x => x.plannedGrossWeight && x.plannedGrossWeight > 0).map(x => rightValue += x.plannedGrossWeight) : 0;

            if (leftValue.toFixed(2) !== rightValue.toFixed(2)) {
                result.status = false;
                result.message = this.translateService.instant('validation.totalGrossWeightNoMatched');
                this._validationResults.push(result);
                return;
            }

            // validate Net Weight
            leftValue = 0, rightValue = 0;
            customerPOs && customerPOs.length > 0 ? customerPOs.filter(x => x.netWeight && x.netWeight > 0).map(x => leftValue += x.netWeight) : 0;

            loads && loads.length > 0 ? loads.filter(x => x.plannedNetWeight && x.plannedNetWeight > 0).map(x => rightValue += x.plannedNetWeight) : 0;

            if (leftValue.toFixed(2) !== rightValue.toFixed(2)) {
                result.status = false;
                result.message = this.translateService.instant('validation.totalNetWeightNoMatched');
                this._validationResults.push(result);
                return;
            }

            // validate Total Package
            leftValue = 0, rightValue = 0;
            customerPOs && customerPOs.length > 0 ? customerPOs.filter(x => x.bookedPackage && x.bookedPackage > 0).map(x => leftValue += x.bookedPackage) : 0;

            loads && loads.length > 0 ? loads.filter(x => x.plannedPackageQuantity && x.plannedPackageQuantity > 0).map(x => rightValue += x.plannedPackageQuantity) : 0;

            if (leftValue.toFixed(2) !== rightValue.toFixed(2)) {
                result.status = false;
                result.message = this.translateService.instant('validation.totalPackageQtyNoMatched');
                this._validationResults.push(result);
                return;
            }
        }
    }

    isVisibleField(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        if (!this.isViewMode) { // apply for view mode only
            return true;
        }
        
        return !FormHelper.isHiddenColumn(this.viewSettings, moduleId, fieldId);
    }

    validateTab(): Array<ValidationData> {
        this._validationResults = [];
        this._validateEquipmentTypes();
        this._validateLoadItemData();
        return this._validationResults;
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
