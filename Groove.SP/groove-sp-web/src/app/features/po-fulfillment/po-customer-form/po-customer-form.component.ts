import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { FormComponent, UserContextService, DropDowns, StringHelper, VerificationSetting, POType } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { POFulfillmentFormService } from '../po-fulfillment-form/po-fulfillment-form.service';
import { faPlus, faEllipsisV, faInfoCircle, faTimes } from '@fortawesome/free-solid-svg-icons';
import { of, Subscription, Subject, Observable } from 'rxjs';
import { MathHelper } from 'src/app/core/helpers/math.helper';
import { debounceTime, tap } from 'rxjs/operators';
import { OrganizationPreferenceService } from '../../organization-preference/Organization-preference.service';

//14-09-2023 changes for addition of Multiple po's 
interface SelectPurchaseOrderModel {
    id: number;
    poNumber: string;
    poKey: string;
    itemsCount: number;
    poLineItemId: number
}
@Component({
    selector: 'app-po-customer-form',
    templateUrl: './po-customer-form.component.html',
    styleUrls: ['./po-customer-form.component.scss']
})
export class POCustomerFormComponent extends FormComponent implements OnChanges, OnInit, OnDestroy {
    @Input() public customerFormOpened: boolean = false;
    @Output() close: EventEmitter<any> = new EventEmitter();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();
    /**It is Customer PO data source which is liked to po-fulfillment-customer
     * Used to store customer PO fetched from server.
     */
    availablePOsList: Array<any>;
    faPlus = faPlus;
    faEllipsisV = faEllipsisV;
    faInfoCircle = faInfoCircle;
    @Input() public formMode: any;
    @Input() public model: any;
    @Input() public parentModel: any;
    @Input() public isSelectedDrag = false;
    @Input() public isRequireHsCode: boolean;
    @Input() public isRequireBookedPackage: boolean;
    @Input() public isRequirePackageUOM: boolean;
    @Input() public currentSelectedIndex = null;
    @Input() public originBalance: number;
    @Input() public originFulfillmentUnitQty: number;
    @Input() public poType: POType;
    @Input() public currentUser: any;

    public searchTermKeyup$ = new Subject<string>();

    /**It is model data source for the tree */
    treeData = null;
    treeViewPagination: ILoadMoreRequestArgs = {
        skip: 0,
        take: 20,
        loadedRecordCount: 0,
        maximumRecordCount: 0,
        loadingData: false
    };
    selectedDragItem = null;
    public expandedKeys: any[];

    CustomerPOFormModeType = {
        add: 0,
        edit: 1
    };

    validationRules = {
        'fulfillmentUnitQty': {
            'required': 'label.bookedQty',
            'greaterThan': 'label.zero'
        },
        'productCode': {
            'required': 'label.productCode'
        },
        'bookedPackage': {
            'required': 'label.bookedPackage',
            'greaterThan': 'label.zero'
        },
        'netWeight': {
            'mustNotGreaterThan': 'label.grossWeight'
        },
        'grossWeight': {
            'greaterThan': 'label.zero'
        },
        'volumeCBM': {
            'greaterThan': 'label.zero'
        },
        'hsCode': {
            'required': 'label.hsCode',
            'numericMaxlength': 'validation.numericMaxlengthHsCode',
            'separatedSymbol': 'validation.separatedSymbolHsCode'
        },
        'packageUOM': {}
    };

    searchByOptions: any = [
        { label: 'label.customerPO', value: 'C' },
        { label: 'label.productCode', value: 'P' },
    ];

    searchByType = {
        customerPO: 'C',
        productCode: 'P'
    };

    searchBy = this.searchByType.customerPO;
    searchTerm = '';

    productVerificationSetting: any = {
        commodityVerification: VerificationSetting.AsPerPO
    };
    verificationSetting = VerificationSetting;
    isAutoCalculationMode: boolean = true;
    commodityOptions = DropDowns.CommodityString;
    countryOptions: any;
    unitUOMTypeOptions = DropDowns.UnitUOMStringType;
    packageUOMTypeOptions = DropDowns.PackageUOMStringType;
    /**Customer PO data source */
    selectPOList: any[];

    /**Cache loaded tree data, used as replacing line item by another */
    treeDataCache: any[] = [];

    firstPOSelected: any;
    customerOrganization: any;
    supplierOrganization: any;
    nodeToExpand: any;
    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    //14-09-2023 changes for addition of Multiple po's 
    selectedPOs: Array<any> = [];
    existingPo = [];
    faTimes = faTimes;
    public expandedKeysForMultiple: any[];
    wholePo: any;
    bookedQuantityErrorMessage="";
    buyerCompliance: any = {};


    get ifEditMode(): boolean {
        return this.formMode === this.CustomerPOFormModeType.edit;
    }

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: POFulfillmentFormService,
        public translateService: TranslateService,
        private organizationPreferenceService: OrganizationPreferenceService,
        private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);

        let sub = this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
            }
        });
        this._subscriptions.push(sub);

        // Register handler for key input on search term
        sub = this.searchTermKeyup$.pipe(
            debounceTime(1000),
            tap((searchTerm: string) => {
                if (StringHelper.isNullOrEmpty(searchTerm) || searchTerm.length === 0 || searchTerm.length >= 3) {
                    this._fetchPOsDataSource(false, searchTerm);
                }
            }
            )).subscribe();
        this._subscriptions.push(sub);

    }



    ngOnChanges(changes: SimpleChanges): void {
        if (changes?.model?.currentValue) {
            setTimeout(() => {
                this.validateHsCode(this.model.hsCode, true);
            }, 1);
        }
    }

    ngOnInit() {
        let sub = this.service.getCountries().subscribe(countries => {
            this.countryOptions = countries;
        });
        this._subscriptions.push(sub);

        // Set Buyer Compliance for tab Customer PO
        sub = this.service.buyerCompliance$.subscribe(x => {
            this.productVerificationSetting = x ? x[0].productVerificationSetting : null;
            if (this.productVerificationSetting && this.productVerificationSetting.isRequireGrossWeight) {
                this.validationRules['grossWeight']['required'] = 'label.grossWeight';
            }

            if (this.productVerificationSetting && this.productVerificationSetting.isRequireVolume) {
                this.validationRules['volumeCBM']['required'] = 'label.volume';
            }
        });

        this._subscriptions.push(sub);

        if (this.isRequirePackageUOM) {
            this.validationRules['packageUOM']['required'] = 'label.packageUOM';
        }
        
        // 15-09-2023 /CR/
        this.service.buyerCompliance$.subscribe(x => {
            this.service._buyerComplianceData$.next(x);
        });
    }

    // need statements to drop work
    allowDrop(event) {
        event.stopPropagation();
        event.preventDefault();
    }

    clickItem(purchaseOrderId, poLineItemId) {
        this.wholePo = false;
        const selectedDragItem = Object.assign({}, this.findCurrentPO(purchaseOrderId, poLineItemId));
        selectedDragItem.fulfillmentUnitQty = selectedDragItem.balanceUnitQty;
        selectedDragItem.bookedPackage = Math.ceil(!selectedDragItem.outerQuantity || selectedDragItem.outerQuantity <= 0 ? 0 : selectedDragItem.fulfillmentUnitQty / selectedDragItem.outerQuantity);
        const volume = MathHelper.calculateCBM(selectedDragItem.outerDepth, selectedDragItem.outerWidth, selectedDragItem.outerHeight) * selectedDragItem.bookedPackage;
        selectedDragItem.volume = isNaN(volume) || volume === 0 ? null : MathHelper.roundToThreeDecimals(volume);
        const grossWeight = selectedDragItem.outerGrossWeight * selectedDragItem.bookedPackage;
        selectedDragItem.grossWeight = isNaN(grossWeight) || grossWeight === 0 ? null : grossWeight;
        selectedDragItem.status = 1;
        const result: { customerPONumber?: number, lineItem?: {}[] } = {};
        // Iterate through the input data
        result.customerPONumber = selectedDragItem.customerPONumber;
        result.lineItem = [];
        result.lineItem.push(selectedDragItem);
        this.selectedDragItem = null;
        this.selectedDragItem = result;
        console.log(this.selectedDragItem, 'cust po and line item');
    }

    // 25-09-2023 
    clickItemWholePO(selectedPO) {
        this.selectedDragItem = selectedPO.items;
        const newArray = [];
        // Loop through the arrays to find and copy the selected items
        for (let i = 0; i < this.selectedDragItem.length; i++) {
            const purchaseOrderId = this.selectedDragItem[i].purchaseOrderId;
            const poLineItemId = this.selectedDragItem[i].poLineItemId;
            // Find and copy the selected item for each combination of purchaseOrderId and poLineItemId
            const selectedDragItem = Object.assign({}, this.findCurrentPO(purchaseOrderId, poLineItemId));
            if (!selectedDragItem.isSelected) {
                selectedDragItem.fulfillmentUnitQty = selectedDragItem.balanceUnitQty;
                selectedDragItem.bookedPackage = Math.ceil(!selectedDragItem.outerQuantity || selectedDragItem.outerQuantity <= 0 ? 0 : selectedDragItem.fulfillmentUnitQty / selectedDragItem.outerQuantity);
                const volume = MathHelper.calculateCBM(selectedDragItem.outerDepth, selectedDragItem.outerWidth, selectedDragItem.outerHeight) * selectedDragItem.bookedPackage;
                selectedDragItem.volume = isNaN(volume) || volume === 0 ? null : MathHelper.roundToThreeDecimals(volume);
                const grossWeight = selectedDragItem.outerGrossWeight * selectedDragItem.bookedPackage;
                selectedDragItem.grossWeight = isNaN(grossWeight) || grossWeight === 0 ? null : grossWeight;
                selectedDragItem.status = 1;
                // Push the selected item into the array
                newArray.push(selectedDragItem);
            }
        }
        console.log(newArray, "new");
        // Initialize the result object
        const result: { customerPONumber?: number, lineItem?: {}[] } = {};

        // Iterate through the input data
        for (const item of newArray) {
            if (!result.customerPONumber) {
                result.customerPONumber = item.customerPONumber;
                result.lineItem = [];
            }
            result.lineItem.push(item);
        }
        this.selectedDragItem = null;
        this.selectedDragItem = result;
        console.log(this.selectedDragItem, 'cust po and line item');
        this.wholePo = true;
    }

    /**Find line-item information for current node
     * Must seek in availablePOsList which contains all loaded customer POs
     */
    findCurrentPO(purchaseOrderId, poLineItemId) {
        const poNodeIndex = this.availablePOsList.findIndex(x => x.id === purchaseOrderId);
        if (poNodeIndex < 0) {
            return null;
        }
        const purchaseOrder = this.availablePOsList[poNodeIndex];
        const lineItemIndex = purchaseOrder.lineItems.findIndex(x => x.id === poLineItemId);
        if (lineItemIndex < 0) {
            return null;
        }
        const result = purchaseOrder.lineItems[lineItemIndex];
        result.customerPONumber = purchaseOrder.poNumber;
        result.purchaseOrderId = purchaseOrder.id;
        result.poLineItemId = result.id;
        result.poType = purchaseOrder.poType;

        // When add Customer PO
        // Need get more expectedShipDate/expectedDeliveryDate to update expectedShipDate/expectedDeliveryDate in general booking tab 
        result.expectedShipDate = purchaseOrder.expectedShipDate;
        result.expectedDeliveryDate = purchaseOrder.expectedDeliveryDate;

        return result;
    }

    /**Find line-item information for current node.
     * Must seek in treeDataCache which contains all loaded tree data
     */
    findCurrentNodeInTree(purchaseOrderId, poLineItemId) {
        const poNodeIndex = this.treeDataCache.findIndex(x => x.purchaseOrderId === purchaseOrderId);
        if (poNodeIndex < 0) {
            return null;
        }
        const lineItemIndex = this.treeDataCache[poNodeIndex].items.findIndex(x => x.poLineItemId === poLineItemId);
        if (lineItemIndex < 0) {
            return null;
        }
        const result = this.treeDataCache[poNodeIndex].items[lineItemIndex];
        return result;
    }

    onDrop() {
        // notice: handle after user drop new line-item into the right
        this.resetTree();
        this.isSelectedDrag = true;
        //14-09-2023 changes for addition of Multiple po's 
        let found = false; // Use a flag to check if a match is found
        if (this.selectedPOs.length > 0) {
            if (this.wholePo) {
                for (let i = 0; i < this.selectedPOs.length; i++) {
                    if (this.selectedPOs[i].customerPONumber === this.selectedDragItem.customerPONumber) {
                        // Replace the existing object with selectedDragItem
                        this.selectedPOs[i] = this.selectedDragItem;
                        found = true;
                        break; // Exit the loop since we found a match
                    }
                }
            }
            else {
                found = true;
                const matchingPO = this.selectedPOs.find(po => po.customerPONumber === this.selectedDragItem.customerPONumber);
                if (matchingPO) {
                    matchingPO.lineItem.push(this.selectedDragItem.lineItem[0]);
                } else {
                    // If no matchingPO is found, create a new one
                    this.selectedPOs.push(this.selectedDragItem);
                }
            }
        }
        // if match is found also selectedPOs is having values
        if (!found) {
            this.selectedPOs.push(this.selectedDragItem);
        }
        const selectedpoLineItemId = [];
        this.selectedPOs.forEach(po => {
            po.lineItem.forEach(item => {
                selectedpoLineItemId.push(item.id);
            });
        });
        this.treeData.forEach(item => {
            item.items.forEach(subitem => {
                if (selectedpoLineItemId.includes(subitem.poLineItemId)) {
                    subitem.isSelected = true;
                    subitem.balanceUnitQty = 0;
                    console.log(subitem);
                }
            });
        });
        //14-09-2023 changes for addition of Multiple po's 
        // Reset data for selected node first
        // if (this.model) {
        //     const currentSelectedNode = this.findCurrentNodeInTree(this.model.purchaseOrderId,
        //         this.model.poLineItemId);
        //     currentSelectedNode.isSelected = false;
        //     currentSelectedNode.balanceUnitQty = this.model.fulfillmentUnitQty + this.model.balanceUnitQty;

        //     const currentSelectedPO = this.findCurrentPO(this.model.purchaseOrderId, this.model.poLineItemId);
        //     currentSelectedPO.isSelected = false;
        //     currentSelectedPO.balanceUnitQty = this.model.fulfillmentUnitQty + this.model.balanceUnitQty;
        // }

        this.selectedDragItem.id = 0;
        // const newSelectedNode = this.findCurrentNodeInTree(this.selectedDragItem.purchaseOrderId, this.selectedDragItem.poLineItemId);
        // newSelectedNode.isSelected = true;
        // newSelectedNode.balanceUnitQty = 0;
        // this.bindingData(this.selectedDragItem);
        // setTimeout(() => {
        //     this.validateHsCode(this.model.hsCode, true);
        // }, 1);
    }

    onDragStart(event) {
        this.isSelectedDrag = false;
        event.dataTransfer.setData('text', '');
    }

    onDragEnd() {
        if (this.isEditMode) {
            this.isSelectedDrag = this.selectedDragItem !== null;
        } else {
            this.isSelectedDrag = false;
        }
    }

    closeSelect() {
        if (this.selectedDragItem === null && this.formMode === this.CustomerPOFormModeType.edit) {
            this.selectedDragItem = {
                purchaseOrderId: this.currentSelectedIndex.purchaseOrderId,
                poLineItemId: this.currentSelectedIndex.poLineItemId,
            };
        }

        if (this.selectedDragItem) {
            const selectedPO = this.findCurrentPO(this.selectedDragItem.purchaseOrderId, this.selectedDragItem.poLineItemId);
            if (selectedPO) {
                selectedPO.isSelected = false;
                selectedPO.balanceUnitQty = this.model.fulfillmentUnitQty + this.model.balanceUnitQty;
            }

            const currentNode = this.findCurrentNodeInTree(this.selectedDragItem.purchaseOrderId, this.selectedDragItem.poLineItemId);
            currentNode.isSelected = false;
            currentNode.balanceUnitQty = this.model.fulfillmentUnitQty + this.model.balanceUnitQty;

            this.selectedDragItem = null;
            this.isSelectedDrag = false;
        }
    }

    hasChildren(item: any) {
        return item.items && item.items.length > 0;
    }

    fetchChildren(item: any) {
        return of(item.items);
    }

    bindingData(item) {
        this.model = item;
        this.isAutoCalculationMode = true;
        this.model.status = 1;
        this.originBalance = this.model.balanceUnitQty;
        this.originFulfillmentUnitQty = this.model.fulfillmentUnitQty || 0;
        // calculate and auto-fill
        this.model.fulfillmentUnitQty = this.model.balanceUnitQty;
        this.model.balanceUnitQty = 0;
        this.model.bookedPackage = Math.ceil(!this.model.outerQuantity || this.model.outerQuantity <= 0 ? 0 : this.model.fulfillmentUnitQty / this.model.outerQuantity);
        const volume = MathHelper.calculateCBM(this.model.outerDepth, this.model.outerWidth, this.model.outerHeight) * this.model.bookedPackage;
        this.model.volume = isNaN(volume) || volume === 0 ? null : MathHelper.roundToThreeDecimals(volume);
        const grossWeight = this.model.outerGrossWeight * this.model.bookedPackage;
        this.model.grossWeight = isNaN(grossWeight) || grossWeight === 0 ? null : grossWeight;
    }

    onFulfillmentUnitQtyChanged(val) {
        const bookedQty = StringHelper.isNullOrEmpty(this.model.fulfillmentUnitQty) ? 0 : this.model.fulfillmentUnitQty;

        this.model.balanceUnitQty = this.originBalance - bookedQty + this.originFulfillmentUnitQty;
        if (this.isAutoCalculationMode) {
            this.model.bookedPackage = Math.ceil(!this.model.outerQuantity || this.model.outerQuantity <= 0 ? 0 : bookedQty / this.model.outerQuantity);
            this.onBookedPackageChanged();
        }
        const currentNode = this.findCurrentNodeInTree(this.model.purchaseOrderId, this.model.poLineItemId);
        currentNode.balanceUnitQty = this.model.balanceUnitQty;
        currentNode.isSelected = currentNode.balanceUnitQty < 0 || currentNode.isSelected;
        this.checkMinMaxBookedQuantity(this.model);
    }

    onBookedPackageChanged() {
        if (StringHelper.isNullOrEmpty(this.model.bookedPackage)) {
            this.model.bookedPackage = 0;
        }
        if (this.isAutoCalculationMode) {
            const volume = MathHelper.calculateCBM(this.model.outerDepth, this.model.outerWidth, this.model.outerHeight) * this.model.bookedPackage;
            this.model.volume = isNaN(volume) || volume === 0 ? null : MathHelper.roundToThreeDecimals(volume);
            const grossWeight = this.model.outerGrossWeight * this.model.bookedPackage;
            this.model.grossWeight = isNaN(grossWeight) || grossWeight === 0 ? null : grossWeight;
            this.onValidateNetWeightKGS();
        }
    }

    onValidateNetWeightKGS() {
        if (StringHelper.isNullOrEmpty(this.model.netWeight) || StringHelper.isNullOrEmpty(this.model.grossWeight)) {
            this.setValidControl('netWeight');
            return;
        }
        if (this.model.netWeight > this.model.grossWeight) {
            this.setInvalidControl('netWeight', 'mustNotGreaterThan');
        } else {
            this.setValidControl('netWeight');
        }
    }

    onCalculationModeChanged() {
        this.isAutoCalculationMode = !this.isAutoCalculationMode;
        if (this.isAutoCalculationMode && !StringHelper.isNullOrEmpty(this.model.fulfillmentUnitQty)) {
            // populate data based on fulfillmentUnitQty
            this.model.bookedPackage = Math.ceil(!this.model.outerQuantity || this.model.outerQuantity <= 0 ? 0 : this.model.fulfillmentUnitQty / this.model.outerQuantity);
            this.onBookedPackageChanged();
        }
    }

    onHsCodeValueChanged() {
        if (this.isRequireHsCode && StringHelper.isNullOrEmpty(this.model.hsCode)) {
            this.setInvalidControl('hsCode', 'required');
        }
        this.validateHsCode(this.model.hsCode, true);
    }

    validateHsCode(hsCodeInputValue: string, isSetErrorOnForm?: boolean) {
        if (!StringHelper.isNullOrEmpty(hsCodeInputValue)) {
            let isValidHsCode = true;
            let valueToTest = hsCodeInputValue.replace(/[.\s]/g, '');
            const hsCodeControl = isSetErrorOnForm ? this.mainForm.form.get('hsCode') : null;

            // check for comma first: Please separate the HS Code by comma.
            if (/^\b([a-zA-Z0-9,])+$/g.test(valueToTest)) {
            } else {
                if (isSetErrorOnForm) {
                    this.setInvalidControl('hsCode', 'separatedSymbol');
                    hsCodeControl.markAsDirty();
                }
                isValidHsCode = false;
            }
            // fast return
            if (!isValidHsCode) {
                return isValidHsCode;
            }


            // Check for valid format: Its length must be in 4, 6, 8, 10, and 13 digits only.
            valueToTest = valueToTest.replace(/[^0-9]/g, ',');
            const hsCodeArray = valueToTest.split(',');

            hsCodeArray.map((item: string) => {
                if (/^(?:\d{4},?|\d{6},?|\d{8},?|\d{10},?|\d{13},?)$/g.test(item)) {
                    // is valid
                } else {
                    if (isSetErrorOnForm) {
                        this.setInvalidControl('hsCode', 'numericMaxlength');
                        hsCodeControl.markAsDirty();
                    }
                    isValidHsCode = false;
                }
            });
            return isValidHsCode;
        }
    }

    onPackageUOMValueChanged() {
        if (this.isRequirePackageUOM && StringHelper.isNullOrEmpty(this.model.packageUOM)) {
            this.validationRules['packageUOM'] = {
                'required': 'label.packageUOM'
            };
            this.setInvalidControl('packageUOM', 'required');
        }
    }

    /**To build the tree for PO. It is in the left of customer PO popup
     * It should be call to initialize popup
    */
    buildTreeForPODataSource(purchaseOrdersSource, customerOrg, supplierOrg, firstPOSelected, expandNode?: any) {
        this.availablePOsList = purchaseOrdersSource;
        this.firstPOSelected = firstPOSelected;
        this.customerOrganization = customerOrg;
        this.supplierOrganization = supplierOrg;
        this.nodeToExpand = expandNode;

        let obs$;

        // There is not information on organization code on booking contacts
        // Have to get organization code to get data from ArticleMaster table
        const customerOrganizationCode = customerOrg.organizationCode;
        if (StringHelper.isNullOrEmpty(customerOrganizationCode)) {
            obs$ = this.service.getOrganizationsByIds([customerOrg.organizationId]).map((x: any) => x[0].code);
        } else {
            obs$ = of(customerOrg.organizationCode);
        }

        this._cleanupWorkingState();

        obs$.pipe(
            tap((orgCode: string) => {
                this.customerOrganization.organizationCode = orgCode;
                const data = [...purchaseOrdersSource];
                this._refreshTreeView(data);
                this._fetchPOsDataSource(false, '');

            })).subscribe();
    }

    /**To clear all working data: variable, caches.. */
    private _cleanupWorkingState() {
        this.searchBy = 'C';
        this.searchTerm = '';
        this.expandedKeys = [];
        // 26-09-2023
        this.expandedKeysForMultiple = [];
        this.treeData = [];
        this.selectPOList = [];
        this.treeDataCache = [];
    }

    private _refreshTreeView(data: Array<any>) {
        this.treeData = [];
        for (let i = 0; i < data.length; i++) {
            const customerPO = data[i];
            const menu = {
                text: customerPO.poNumber,
                items: [],
                type: this.searchByType.customerPO,
                purchaseOrderId: data[i].id,
                disabled: true
            };
            for (let j = 0; j < customerPO.lineItems.length; j++) {
                const poLine = customerPO.lineItems[j];
                if (poLine.balanceUnitQty > 0 || (!StringHelper.isNullOrEmpty(poLine.isSelected) && poLine.isSelected)) {
                    menu.items.push({
                        text: `${poLine.productCode}${!StringHelper.isNullOrEmpty(poLine.gridValue) ? (' - ' + poLine.gridValue) : ''}`,
                        type: this.searchByType.productCode,
                        balanceUnitQty: poLine.balanceUnitQty,
                        isSelected: poLine.isSelected,
                        purchaseOrderId: data[i].id,
                        poLineItemId: poLine.id,
                        /** Flag to show item on tree data
                         * Otherwise, hide from UI
                        */
                        isShown: true
                    });
                    // If any child item is not selected, mark the parent node as not disabled
                    if (!poLine.isSelected) {
                        menu.disabled = false;
                    }
                }
            }
            this.treeData.push(menu);
        }
        this.expandedKeys = [];
        // 26-09-2023
        this.expandedKeysForMultiple = [];

        this._integrateWithTreeDataCache(this.treeData);

        if (this.searchBy === 'P') {
            this.expandAllNode();
        } else if (this.nodeToExpand) {
            this._expandNode(this.nodeToExpand);
        }
    }

    /**Access the cache to store new data
     * Otherwise, load data from cache.
    */
    private _integrateWithTreeDataCache(treeData: Array<any>) {
        for (let index = 0; index < treeData.length; index++) {
            const nodeData = treeData[index];
            const existed = this.treeDataCache.find(x => x.purchaseOrderId === nodeData.purchaseOrderId);
            if (!existed) {
                this.treeDataCache.push(nodeData);
            } else {
                treeData[index] = existed;
            }
        }
    }

    // for edit mode
    private _expandNode(currentSelectedIndex) {
        for (let i = 0; i < this.treeData.length; i++) {
            const poItems = this.treeData[i];
            if (poItems.purchaseOrderId === currentSelectedIndex.purchaseOrderId) {
                for (let j = 0; j < poItems.items.length; j++) {
                    const poLineItem = poItems.items[j];
                    if (poLineItem.poLineItemId === currentSelectedIndex.poLineItemId) {
                        return this.expandedKeys = [i.toString()];
                    }
                }
            }
        }
    }

    expandAllNode() {
        this.expandedKeys = [];
        for (let i = 0; i < this.treeData.length; i++) {
            this.expandedKeys.push(i.toString());
        }
    }

    resetTree() {
        this.treeData.forEach(node => {
            node.items.forEach(item => {
                const poLine = this.findCurrentPO(item.purchaseOrderId, item.poLineItemId);
                if (poLine) {
                    item.isSelected = poLine.isSelected;
                }
            });
        });
    }

    loadMorePO() {
        if (
            this.treeViewPagination.loadedRecordCount <
            this.treeViewPagination.maximumRecordCount
        ) {
            this._fetchPOsDataSource(true);
        }
    }

    /**Fetch data from server */
    protected _fetchPOsDataSource(
        loadMoreMode?: boolean,
        searchValue?: string
    ) {
        // Set status here to make show loading icon
        this.treeViewPagination.loadingData = true;

        // Reset data if it is not loading more POs
        if (!loadMoreMode) {
            this.treeViewPagination.skip = 0;
            this.treeViewPagination.loadedRecordCount = 0;
            this.treeViewPagination.maximumRecordCount = 0;
        }
        const skip = this.treeViewPagination.skip;
        const take = this.treeViewPagination.take;
        const sub = this.service
            .fetchPOsDataSource(
                this.customerOrganization.organizationId,
                this.customerOrganization.organizationCode,
                this.supplierOrganization.organizationId,
                this.supplierOrganization.companyName,
                skip,
                take,
                // searchTeam will be from direct value of input or data model
                StringHelper.isNullOrEmpty(searchValue)
                    ? this.searchTerm
                    : searchValue,
                this.searchBy,
                this.poType,
                StringHelper.isNullOrEmpty(this.firstPOSelected)
                    ? 0
                    : this.firstPOSelected.id
            )
            .pipe(
                tap((serverData) => {
                    const rowCount = StringHelper.isNullOrEmpty(serverData) || StringHelper.isNullOrEmpty(serverData[0]) ? 0 : serverData[0].recordCount;
                    for (let index = 0; index < serverData.length; index++) {
                        const item = serverData[index];
                        const loaded = this.availablePOsList.find((y) => y.id === item.id);
                        if (!StringHelper.isNullOrEmpty(loaded)) {
                            // to update hs code, chinese description from organization preference
                            // as it gets latest data from servers
                            loaded.lineItems.map(cachedData => {
                                const latestData = serverData[index].lineItems.find(y => y.productCode === cachedData.productCode);
                                if (latestData) {
                                    cachedData.hsCode = latestData.hsCode;
                                    cachedData.chineseDescription = latestData.chineseDescription;
                                }
                            });
                            serverData[index] = loaded;
                        } else {
                            this.availablePOsList.push(item);
                        }
                    }
                    // Customer PO data must be linked to the shared variable on service
                    this.service.cacheCustomerPOs(serverData);

                    if (loadMoreMode) {
                        this.selectPOList = this.selectPOList.concat(serverData);
                    } else {
                        this.selectPOList = serverData;
                        this.treeViewPagination.maximumRecordCount = rowCount;
                    }

                    // Refresh Tree UI again
                    this._refreshTreeView(this.selectPOList);

                    // update tree view paging
                    this.treeViewPagination.loadedRecordCount += serverData.length;
                    this.treeViewPagination.skip = this.treeViewPagination.loadedRecordCount;

                    // set other status
                    this.treeViewPagination.loadingData = false;

                })
            )
            .subscribe();
        this._subscriptions.push(sub);
    }

    onFormClosed() {
        this.customerFormOpened = false;
        this.resetForm();
        this.resetCurrentForm();
        this.close.emit();
        //14-09-2023 changes for addition of Multiple po's /CR/
        this.selectedPOs = [];
    }

    resetForm() {
        this.selectedDragItem = null;
        this.isSelectedDrag = false;
        this.searchTerm = '';
    }

    onSave() {
        if (this.selectedPOs) {
            if (this.selectedPOs.length > 0) {
                this.isSelectedDrag = true;
            }
        }
        this.validateAllFields(false);
        if (!this.mainForm.valid || !this.isSelectedDrag) {
            return;
        }

        if (this.mainForm.valid) {
            this.resetForm();
            switch (this.formMode) {
                case this.CustomerPOFormModeType.add:
                    //14-09-2023 changes for addition of Multiple po's
                    // Initialize an empty array to store the line items
                    const allLineItems = [];
                    // Loop through selectedPOs and extract line items
                    this.selectedPOs.forEach(po => {
                        // Iterate through line items and set balanceQuantity to 0
                        const modifiedLineItems = po.lineItem.map(lineItem => ({ ...lineItem, balanceUnitQty: 0 }));
                        allLineItems.push(...modifiedLineItems);
                    });
                    this.add.emit(allLineItems);
                    // this.add.emit(this.model);
                    break;
                case this.CustomerPOFormModeType.edit:
                    this.edit.emit(this.model);
                    break;
            }
        }
        //14-09-2023 changes for addition of Multiple po's /CR/
        this.selectedPOs = [];
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }
    //14-09-2023 changes for addition of Multiple po's
    // remove line items
    unselectPOLineItem(data) {
        this.treeData.forEach(item => {
            item.items.forEach(subitem => {
                if (data.poLineItemId == subitem.poLineItemId) {
                    subitem.isSelected = undefined;
                    subitem.balanceUnitQty = data.balanceUnitQty
                }
            });
        });
        // this.selectedPOs = this.selectedPOs.filter(item => item.poLineItemId !== data.poLineItemId);
        // Remove lineItem items based on data.poLineItemId
        this.selectedPOs.forEach(po => {
            po.lineItem = po.lineItem.filter(item => item.poLineItemId !== data.poLineItemId);
        });
        // Remove objects with no lineItem
        this.selectedPOs = this.selectedPOs.filter(po => po.lineItem.length > 0);
        if (this.selectedPOs.length == 0) { this.isSelectedDrag = false; }
        this.selectedPOs = [...this.selectedPOs];

    }
    // remove whole po
    unselectPO(data) {
        this.treeData.forEach(item => {
            if (data.customerPONumber == item.text) {
                item.items.forEach(subitem => {
                    const matchingLineItem = data.lineItem.find(element => element.id == subitem.poLineItemId);
                    if (matchingLineItem) {
                        subitem.isSelected = undefined;
                        subitem.balanceUnitQty = matchingLineItem.balanceUnitQty;
                    }
                });
            }
        });
        this.selectedPOs = this.selectedPOs.filter(po => po.customerPONumber !== data.customerPONumber);
        if (this.selectedPOs.length == 0) { this.isSelectedDrag = false; }
        this.selectedPOs = [...this.selectedPOs];
    }

    public fetchChildrenForMultiple(node: any): Observable<any[]> {
        //Return the items collection of the parent node as children.
        return of(node.lineItem);
    }

    public hasChildrenForMultiple(node: any): boolean {
        //Check if the parent node has children.
        return node.lineItem && node.lineItem.length > 0;
    }

    expandAllNodeForMultiple() {
        this.expandedKeysForMultiple = [];
        for (let i = 0; i < this.selectedPOs.length; i++) {
            this.expandedKeysForMultiple.push(i.toString());
        }
    }
         //27/9/2023
         checkMinMaxBookedQuantity(poLineItem){
            this.bookedQuantityErrorMessage = "";
            let policy;
            this.service.buyerCompliance$.subscribe(x => {
                this.buyerCompliance = x[0];
                policy = x[0].bookingPolicies;
            })
            if (poLineItem != null)
                    {
                        var min = poLineItem.orderedUnitQty - (this.buyerCompliance.shortShipTolerancePercentage * poLineItem.orderedUnitQty);
                        var max = poLineItem.orderedUnitQty + (this.buyerCompliance.overshipTolerancePercentage * poLineItem.orderedUnitQty);
    
                        for(let i = 0 ; i < policy.length;i++){
    
                            const isShortShipment = policy[i].fulfillmentAccuracyIds.includes(1);
                            const isNormalShipment = policy[i].fulfillmentAccuracyIds.includes(2);
                            const isOverShipment = policy[i].fulfillmentAccuracyIds.includes(4);
                        // light 
                        if (isShortShipment && poLineItem.fulfillmentUnitQty < min)
                        {
                            this.bookedQuantityErrorMessage =  "Min Booked Qty :" + min;
                            break;
                        }
                        // normal 
                        if (isNormalShipment &&  (poLineItem.fulfillmentUnitQty >= min && poLineItem.fulfillmentUnitQty <= max))
                        {
                            this.bookedQuantityErrorMessage = "Min Booked Qty :" + min + "Max Booked Qty :" + max;
                            break;
    
                        }
                        // over
                        if (isOverShipment &&  poLineItem.fulfillmentUnitQty > max)
                        {
                            this.bookedQuantityErrorMessage = "Max Booked Qty :" + max;
                            break;
                        }
                    }                        
                }
         }
}
