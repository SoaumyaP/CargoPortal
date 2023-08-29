import { OnInit, ViewChild, AfterViewChecked } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { forkJoin } from 'rxjs';
import { FormService } from './form.service';
import { TranslateService } from '@ngx-translate/core';
import { UserProfileModel } from '../models/user/user-profile.model';
import { DATE_FORMAT } from '../helpers/date.helper';
import { StringHelper } from '../helpers/string.helper';
import { DefaultDebounceTimeInput, DefaultValue2Hyphens } from '../models/constants/app-constants';
import { AppPermissions } from '../auth/auth-constants';
import { FormModeType } from '../models/enums/enums';


export class FormComponent implements OnInit, AfterViewChecked {
    mainForm: NgForm;
    @ViewChild('mainForm', { static: false }) currentForm: NgForm;
    DATE_FORMAT = DATE_FORMAT;
    defaultValue = DefaultValue2Hyphens;
    defaultDebounceTimeInput = DefaultDebounceTimeInput;
    readonly AppPermissions = AppPermissions;
    
    public defaultDropDownItem: { text: string, label: string, description: string, value: number } =
    {
        text: 'label.select',
        label: 'label.select',
        description: 'select',
        value: null
    };

    public formMode: any = {
        add: 'add',
        edit: 'edit',
        view: 'view'
    };

    /* ==== FORM MODE ==== */
    public isViewMode: boolean;
    public isEditMode: boolean;
    public isAddMode: boolean;

    // id for view/edit - from url
    public modelId: any;

    // to be overridden in derived component to load model on view/edit mode based on model name
    public modelName: string;

    public modelIdParam = 'id';

    /**The model for form*/
    public model: any = {};
    public updateModel: any;

    public initData: Array<{ sourceUrl: string, params?: any }> = [];

    public isInitDataLoaded: boolean;

    protected isModelSubmitted: boolean;
    protected elements: any = {};

    protected currentUser: UserProfileModel;

    /**
     * To contains validation messages for all inputs in the main form
     *
     * @usageNotes
     *
     * ```
     * this.formErrors['shipmentNo'] = 'Shipment No. is not existing in the system';
     * this.formErrors.poNo = 'Purchase No. is not existing in the system';
     * ```
     */
    formErrors: {[key: string]: any} = {};

    /**
     * To contains all validation rules for all inputs in the main form
     *
     * @usageNotes
     *
     * ```
     *  this. validationRules = {
     *       'masterBillOfLadingNo': {
     *               'required': 'label.masterBLNo'
     *           },
     *           'placeOfIssue': {
     *               'required': 'label.placeOfIssue'
     *           },
     *           'issueDate': {
     *               'required': 'label.issueDates',
     *               'dateWithin90DaysToday': 'validation.dateWithin90DaysToday'
     *           },
     *           'onBoardDate': {
     *               'required': 'label.onBoardDates',
     *               'dateWithin90DaysToday': 'validation.dateWithin90DaysToday'
     *           }
     *       };
     * ```
     */
    validationRules: {[key: string]: any} = {};
    formType: FormModeType;
    paramMode: string;
    
    /**Store current query params. */
    queryParams: Params;

    constructor(protected route: ActivatedRoute, public service: FormService<any>,
        public notification: NotificationPopup, public translateService: TranslateService,
        public router: Router) {
    }

    ngOnInit() {
        this.route.queryParams.subscribe((queryParams: Params) => {
            this.queryParams = queryParams; // capture queryParams for further uses.
            this.modelId = this.route.snapshot.params[this.modelIdParam];
            this.formType = queryParams['formType'];
            this.isModelSubmitted = false;

            const state = queryParams['state'];
            if (!StringHelper.isNullOrEmpty(state) && state === 'tracking') {
                this.initData = [];
                this.isInitDataLoaded = false;
            }
            this.prepareModel();
            this.loadInitData();
        });

        this.route.params.subscribe((params: Params) => {
            const mode = params['mode'] != null ? params['mode'].toLowerCase() : null;
            this.paramMode = mode;
            this.setFormMode(mode);
        });
    }

    ngOnInitForAddMode() {
        this.prepareModel();
        this.loadInitData();
    }

    canDeactivate(): Observable<boolean> | boolean {
        if ((this.isAddMode || this.isEditMode) && !this.isModelSubmitted) {
            const confirmTitle = StringHelper.isNullOrEmpty(this.modelName) ? 'confirmation' : 'label.' + this.modelName;
            const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', confirmTitle);

            return confirmDlg.result.map(result => (<any>result).value);
        }

        return true;
    }

    /**Very early called to set current working state
     * @example: url, affiliateCodes, customerRelationshipsParam, organizationIdParam, sourceUrl
     */
    prepareModel() {
        if (!StringHelper.isNullOrEmpty(this.modelName)) {
            let sourceUrl = `${this.service.apiUrl}/${this.modelId}`;
            if (this.service.checkApiPrefix(this.service.apiUrl)) {
                if (!this.route.snapshot.data['allowAnonymous']) {
                    const affiliateCodes = !StringHelper.isNullOrEmpty(this.service.affiliateCodes) ? this.service.affiliateCodes : '';

                    // apply for shipper to fetch PO
                    const customerRelationships = !StringHelper.isNullOrEmpty(this.service.customerRelationships) ? this.service.customerRelationships : '';
                    const customerRelationshipsParam = customerRelationships.length > 0 ? '&customerRelationships=' + customerRelationships : '';

                    const organizationId = this.service.organizationId;
                    const organizationIdParam = organizationId > 0 ? '&organizationId=' + organizationId : '';

                    sourceUrl = `${this.service.apiUrl}/${this.modelId}?affiliates=${affiliateCodes}${customerRelationshipsParam}${organizationIdParam}${this.formType !== undefined ? `&formType=${this.formType}`:''}`;

                } else {
                    sourceUrl = `${this.service.apiUrl}/quicktrack/${this.modelId}`;
                }

            }
            
            this.initData = this.initData.filter(x => x.sourceUrl !== sourceUrl); // Prevent duplicated requests at the same time
            this.initData.push({ sourceUrl });
            
            // Remove the request include 'formType=' query param that is different from the current formType
            this.initData = this.initData.filter(c => !StringHelper.includes(c.sourceUrl,'formType=') ||  StringHelper.includes(c.sourceUrl,`formType=${this.formType}`))

            if (this.modelName === 'bulkFulfillments') {
                // Prevent user enter an invalid url
                // Because ngOnInit function is subscribing both params and queryParams observables
                // So need to setTimeout to waiting for those 2 jobs to complete
                setTimeout(() => {
                    if (this.paramMode === FormModeType.Add) {
                        if (this.formType !== FormModeType.Add && this.formType !== FormModeType.Copy) {
                            this.router.navigate(['/error/404']);
                        }
                    }
                    else if (this.paramMode !== this.formType) {
                        this.router.navigate(['/error/404']);
                    }
                }, 100);
            }
        }
    }

    /**Initial data loading here
     * It will be called inside ngOnInit and just after prepareModel method
    */
    loadInitData(keepPageInit: boolean = true) {
        if (keepPageInit) {
            this.isInitDataLoaded = this.initData.length === 0;
        }

        const requests = [];

        for (let i = 0; i < this.initData.length; i++) {
            requests.push(this.service.getData(this.initData[i].sourceUrl, this.initData[i].params));
        }

        forkJoin(requests)
            .subscribe(data => {
                if (!this.isAddMode && !StringHelper.isNullOrEmpty(this.modelName)) {
                    this.model = this.convertModelType(data[data.length - 1]);
                    if (this.model == null) {
                        this.route.queryParams.subscribe((params: Params) => {
                            const state = params['state'];
                            if (!StringHelper.isNullOrEmpty(state) && state === 'tracking') {
                                if (!this.route.snapshot.data['allowAnonymous']) {
                                    this.router.navigate(['/search/no-result']);
                                }
                            } else {
                                this.router.navigate(['/error/404']);
                            }
                        });
                    }
                }

                if (keepPageInit) {
                    this.isInitDataLoaded = true;
                }
                this.onInitDataLoaded(data);
            },
            error => {
                if (error.status === 404) {
                    this.route.queryParams.subscribe((params: Params) => {
                        const state = params['state'];
                        const lang = params['lang'];
                        if (!StringHelper.isNullOrEmpty(state) && state === 'tracking') {
                            if (this.route.snapshot.data['allowAnonymous']) {
                                this.router.navigate(['/quick-track/no-result'],  {queryParams: {lang: lang }});
                            }
                        } else {
                            this.router.navigate(['/error/404']);
                        }
                    });
                } else if (error.status !== 401) {
                    this.notification.showErrorPopup(error.message, 'label.error');
                }
            },
            () => {
            });
    }

    /**Right after data returned, convert it to provided date type.
     * Notes: please return null if input data = null then system redirects to 404 as default behavior.
     * @example convertModelType(data: any): CruiseOrderModel {
            if (data !== null) {
                return new CruiseOrderModel(data);
            }
            return data;
            }
     */
    convertModelType(data: any) {
        return data;
    }

    /**Some further data loadings should be here.
     * It will be called after some initial data loading.
     * @example Custom/specific business data for a component.
    */
    onInitDataLoaded(data) { }

    /**Called as submitting form data */
    onSubmit() {
        let errorMessage = null;

        this.prepareModelForSubmit();

        if (this.mainForm.invalid) {
            errorMessage = 'validation.mandatoryFieldsValidation';
        } else {
            errorMessage = this.doCustomValidationOnSubmit();
        }

        if (!StringHelper.isNullOrEmpty(errorMessage)) {
            this.notification.showErrorPopup(errorMessage, 'label.' + this.modelName);
            this.validateAllFields(false);
            this.focusFirstErrorElement();
        }
        return errorMessage;
    }

    /**Do some other specific validations before submitting form data */
    doCustomValidationOnSubmit(): string { return null; }

    /**Adjust/amend form data before submitting
     * @remarks It will be called before validating form
    */
    prepareModelForSubmit() { }

    setFormMode(mode) {
        if (mode != null) {
            this.isViewMode = mode === this.formMode.view;
            this.isEditMode = mode === this.formMode.edit;
            this.isAddMode = mode === this.formMode.add;
        }
    }

    ngAfterViewChecked() {
        this.formChanged();
    }

    formChanged() {
        if (this.currentForm === this.mainForm) { return; }
        this.mainForm = this.currentForm;
        if (this.mainForm) {
            this.mainForm.valueChanges
                .subscribe(data => this.onValueChanged(data));
        }
    }

    onValueChanged(data?: any) {
        this.validateAllFields(true);
    }

    /**
     * To validate all inputs of the main form
     * @param checkDirty Need to check dirty status of each input
     * @returns void
     */
    validateAllFields(checkDirty: boolean) {
        if (!this.mainForm) { return; }
        const form = this.mainForm.form;

        for (const field in this.validationRules) {
            if (field) {
                this.validateField(field, checkDirty);
            }
        }
    }

    /**
     * To set input is invalid manually
    * @param fieldName Name of input
    * @param errorName The error name is in the validation-rules
    * @param errorMessage Message for error name
    * @returns void
    */
    setInvalidControl(fieldName: string, errorName: string = 'invalid', errorMessage?: string) {
        const field = this.mainForm.form.get(fieldName);
        const currentFieldErrors = field.errors || {};
        currentFieldErrors[errorName] = false;
        field.setErrors(currentFieldErrors);
        // If needed to store message for error name into validationRules
        if (!StringHelper.isNullOrEmpty(errorMessage)) {
            if (StringHelper.isNullOrEmpty(this.validationRules)) {
                this.validationRules = {};
            }
            if (StringHelper.isNullOrEmpty(this.validationRules[fieldName])) {
                this.validationRules[fieldName] = {};
            }
            this.validationRules[fieldName][errorName] = errorMessage;
        }
        this.setFormError(fieldName, errorName);
    }

    /**
     * To set input is valid manually
     * @param fieldName Name of input
     * @param errorName Name of error that needs to be removed, remove all errors if it is null.
     * @returns void
     */
    setValidControl(fieldName: string, errorName?: string) {
        const field = this.mainForm.form.get(fieldName);
        if (StringHelper.isNullOrEmpty(errorName)) {
            field.setErrors(null);
            this.formErrors[fieldName] = '';
        } else {
            if (field.hasError(errorName)) {
                delete field.errors[errorName];
            }
        }
        this.validateField(fieldName);
    }

    /**
     * To validate specific field in the main form by name
     * @param fieldName name of field
     * @param checkDirty need to check dirty
     * @returns void
     */
    validateField(fieldName: string, checkDirty: boolean = false) {
        if (!this.mainForm) { return; }
        const form = this.mainForm.form;
        if (StringHelper.isNullOrEmpty(this.validationRules[fieldName])) {
            return;
        }

        const field = form.get(fieldName);
        if (field) {
            if (field.dirty || !checkDirty) {
                // if control is valid by some default html validations
                if (field.valid) {
                    // need to check more if there is any another custom/business validation
                    const formErrors = this.formErrors;
                    const otherFormErrorKeys = Object.keys(formErrors)?.filter(x => x.startsWith(fieldName + '_') || x.startsWith(fieldName + '.'));
                    let isValid = true;

                    // if there is some formErrors like 'carrier_custom' or 'carrier.business'
                    otherFormErrorKeys?.map(formErrorKey => {
                        const formErrorValue = formErrors[formErrorKey];
                        if (!StringHelper.isNullOrEmpty(formErrorValue)) {
                            // make a control is invalid as custom/business validation failed
                            const currentFieldErrors = field.errors || {};
                            currentFieldErrors[formErrorKey] = false;
                            field.setErrors(currentFieldErrors);
                            isValid = false;
                        }
                    });

                    if (!isValid) {
                        return;
                    }

                    // all good, reset formError for current control
                    this.formErrors[fieldName] = '';
                    field.setErrors(null);
                } else {
                    for (const key in field.errors) {
                        // If the error is custom validation will be validated manually.
                        if (key.startsWith(fieldName + '_') || key.startsWith(fieldName + '.')) {
                            continue;
                        }
                        if (field.errors.hasOwnProperty(key)) {
                            this.setFormError(fieldName, key);
                        }
                    }
                    field.markAsDirty();
                }
            }
        }
    }

    private setFormError(fieldName: string, errorName: string) {
        const messages = this.validationRules[fieldName];

        if (errorName === 'required') {
            this.formErrors[fieldName] = this.translateService.instant('validation.requiredField',
                {
                    fieldName: this.translateService.instant(messages[errorName])
                });
        } else if (errorName === 'pattern') {
            this.formErrors[fieldName] = this.translateService.instant('validation.invalidFormat',
                {
                    fieldName: this.translateService.instant(messages[errorName])
                });
        } else if (errorName === 'minlength') {
            this.formErrors[fieldName] = this.translateService.instant('validation.invalidFormat',
                {
                    fieldName: this.translateService.instant(messages[errorName])
                });
        } else if (errorName === 'maxlength') {
            this.formErrors[fieldName] = this.translateService.instant('validation.invalidFormat',
                {
                    fieldName: this.translateService.instant(messages[errorName])
                });
        } else if (errorName === 'greaterThan') {
            this.formErrors[fieldName] = this.translateService.instant('validation.greaterThan',
                {
                    fieldName: this.translateService.instant(messages[errorName])
                });
        } else if (errorName === 'lessThanOrEqualTo') {
            this.formErrors[fieldName] = this.translateService.instant('validation.lessThanOrEqualTo',
                {
                    fieldName: this.translateService.instant(messages[errorName])
                });
        } else if (errorName === 'laterThanOrEqualTo') {
            this.formErrors[fieldName] = this.translateService.instant('validation.laterThanOrEqualTo',
                {
                    fieldName: this.translateService.instant(messages[errorName])
                });
        } else if (errorName === 'alreadyExists') {
            this.formErrors[fieldName] = this.translateService.instant('validation.alreadyExists',
                {
                    fieldName: this.translateService.instant(messages[errorName])
                });
        } else if (errorName === 'mustNotGreaterThan') {
            this.formErrors[fieldName] = this.translateService.instant('validation.mustNotGreaterThan',
                {
                    currentFieldName: StringHelper.toUpperCaseFirstLetter(this.translateService.instant(`label.${fieldName}`)),
                    fieldName: this.translateService.instant(messages[errorName]).toLowerCase()
                });
        } else if (errorName === 'maxLengthInput') {
            this.formErrors[fieldName] = this.translateService.instant('validation.maxLengthInput',
                {
                    maxValue: messages[errorName].toLocaleString()
                });
        } else {
            try {
                this.formErrors[fieldName] = this.translateService.instant(messages[errorName]);
            } catch {
                this.formErrors[fieldName] = errorName;
                console.log(`Invalid translation with name: '${fieldName}':'${messages[errorName]}'`);
            }
        }
    }

    focusFirstErrorElement() {
        for (const field in this.elements) {
            if (field) {
                const control = this.mainForm.form.get(field);

                if (control && !control.valid) {
                    control.markAsDirty();
                    const el = this.elements[field];
                    if (el) {
                        // IE11, firefox: fix screen dose not scroll to invalid control
                        // window.scrollTo(0, el.wrapper.nativeElement.offsetParent.offsetTop);
                        el.focus();
                        break;
                    }
                }
            }
        }
    }

    deleteFormControls(...formControlNames: string[]) {
        for (const control of formControlNames) {
            this.mainForm.form.removeControl(control);
        }
    }

    /**
     * To clear errors on current form
     */
    resetCurrentForm() {
        this.currentForm.resetForm();
        this.formErrors = {};
    }

    /**
     * To re-call api base on param mode, re-set value for form mode
     */
    reloadData() {
        this.setFormMode(this.paramMode);
        this.loadInitData();
    }
}
