import { HttpClient } from '@angular/common/http';
import { OnDestroy } from '@angular/core';
import { Component } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { faMinus, faPencilAlt, faPlay, faPlayCircle, faPlus, faPowerOff, faTrash, faUnlockAlt } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import moment from 'moment';
import { Observable, Subscription } from 'rxjs';
import { concatMap, tap } from 'rxjs/operators';
import { ColumnSetting, DateHelper, DATE_HOUR_FORMAT, DropDowns, FormComponent, LocalStorageService, Roles, SchedulingStatus, StringHelper, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { SchedulingModel } from '../models/scheduling.model';
import { SchedulingFormService } from './scheduling-form.service';
import { EmailValidationPattern } from 'src/app/core/models/constants/app-constants';
import { ReportListService } from '../../report/report-list/report-list.service';
import { TelerikAccessTokenModel } from 'src/app/core/models/telerik/telerik-access-token.model';
import { TelerikReportParameterModel } from 'src/app/core/models/telerik/telerik-report-parameter.model';
import { TelerikReportSourceModel } from 'src/app/core/models/telerik/telerik-report-source.model';
import { ReportOptionModel } from '../models/report.option.model';
import { SubscriberModel } from '../models/subscriber.model';
import { TelerikActivityModel } from '../models/telerik-activity.model';
import { ColumnOptionModel } from '../models/column.option.model';

@Component({
    selector: 'app-scheduling-form',
    templateUrl: './scheduling-form.component.html',
    styleUrls: ['./scheduling-form.component.scss']
})
export class SchedulingFormComponent extends FormComponent implements OnDestroy {

    modelName = 'scheduling';
    readonly AppPermissions = AppPermissions;
    readonly documentTypes: Array<any> = DropDowns.SchedulingDocumentFormats;
    readonly Roles = Roles;
    readonly defaultSelectOption = {
        name: 'Select',
        value: null
    };

    DATE_HOUR_FORMAT = DATE_HOUR_FORMAT;
    faMinus = faMinus;
    faPlay = faPlay;
    faPencilAlt = faPencilAlt;
    faPowerOff = faPowerOff;
    faUnlockAlt = faUnlockAlt;
    faPlus = faPlus;
    faTrash = faTrash;

    /**
     * selected principal organization from url, all values is lower cased
     */
    urlParams: { [key: string]: any } = {};
    model: SchedulingModel;

    // Section General
    schedulingStatus = SchedulingStatus;

    /**
     * To store information on selected report
     */
    reportSource: TelerikReportSourceModel = {};

    /**
     * To source data source for report down-down list
     */
    reportOptionsDataSource: Array<ReportOptionModel> = [];

    /**
     * Default option for report drop-down list
     */
    public defaultReportOptionItem =
        {
            id: null,
            reportName: 'label.select'
        };

    /**
     * Validation rules on main form, but not on Filtering Criteria section
     */
    validationRules = {
        'name': {
            'required': 'label.name'
        },
        'report': {
            'required': 'label.report'
        },
        'documentFormat': {
            'required': 'label.documentType'
        },
        'startDate': {
            'required': 'label.start'
        }
    };

    /**Selected columns on the report. */
    selectedColumns: ColumnSetting[] = [];
    columnOptionsDataSource: Array<ColumnOptionModel> = [];
    columnOptionsDialogOpened: boolean = false;


    // Section Report filtering
    /**
     * To contain list of inputs (visible) will be shown on section Report Filtering
     */
    reportFilteringFormFields: Array<TelerikReportParameterModel>;

    /**
     * Form group to handle section Report Filtering, to get param values then send to Telerik
     */
    reportFilteringFormGroup: FormGroup = new FormGroup({
        'dummyControl': new FormControl()
    });
    isUpdatingReportFiltering: boolean = false;
    acpTimeout: any;

    // Section Recurrence

    // Section Subscribers
    readonly emailPattern = EmailValidationPattern;

    /**
     * To store data of subscriber emails
     */
    isNewEmailAdding = false;


    // Section form
    isReadyForSubmit: boolean = true;
    private _subscriptions: Array<Subscription> = [];

    /**
     *
     */
    constructor(protected route: ActivatedRoute,
        public schedulingFormService: SchedulingFormService,
        private _reportListService: ReportListService,
        public notification: NotificationPopup,
        public router: Router,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private _httpClient: HttpClient) {

        super(route, schedulingFormService, notification, translateService, router);

        const sub = this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                this._fetchReportListDataSource();
            }
        });
        this._subscriptions.push(sub);

        this.route.queryParams.subscribe(params => {
            this.urlParams = params;
        });
    }

    // Handler for General section

    /**
     * To handle as user changed report selection
     * @param selectedReportId
     */
    onReportValueChange(selectedReportId: number) {
        this.isUpdatingReportFiltering = true;
        if (StringHelper.isNullOrEmpty(selectedReportId)) {
            this._resetReportFilteringSection();
        } else {
            const selectedReport = this.reportOptionsDataSource.find(x => x.id === selectedReportId);
            if (selectedReport) {
                // dummy object to get query string
                const params = (new URL(`http://dummyhost/${selectedReport.reportUrl}`)).searchParams;
                this.reportSource.name = params.get('reportkey');
                this.reportSource.reportServerUrl = params.get('reportserverurl');
                this.reportSource.categoryId = selectedReport.telerikCategoryId;
                this.reportSource.category = selectedReport.telerikCategoryName;
                this.reportSource.reportId = selectedReport.telerikReportId;
                // Dynamic report filter set build
                this._fetchTelerikReportParameters(false);
            } else {
                this._resetReportFilteringSection();
            }

        }

    }

    /**
     * Fetch data source for report drop-down list
     */
    private _fetchReportListDataSource(): void {
        const isInternal = this.currentUser.isInternal;
        const roleId = this.currentUser.role.id;
        const affiliates = this.currentUser.affiliates ? JSON.parse(this.currentUser.affiliates) : [0];
        const sub = this._reportListService.$getReportOptions(isInternal, roleId, affiliates).subscribe(
            (value) => {
                this.reportOptionsDataSource = value;

                if (this.urlParams?.reportId) {
                    this.model.csPortalReportId = +this.urlParams.reportId;
                }
            }
        );
        this._subscriptions.push(sub);
    }

    // End Handler for General section

    // Handler for Recurrence section

    // End Handler for Recurrence section


    // Handler for Filtering Criteria section

    /**
     * To fetch parameter for selected report
     * @param setValueFromModel
     */
    private _fetchTelerikReportParameters(setValueFromModel: boolean) {
        // If view/edit mode, set report parameter from model
        const sub = this._getReportToken$()
            .pipe(
                concatMap(x => this._createReportClient$()),
                concatMap(x => this._getTelerikReportParameters$(setValueFromModel)),
            ).subscribe(
                (parameters: Array<TelerikReportParameterModel>) => {
                    this._buildFilteringCriteriaSection();
                }
            );
        this._subscriptions.push(sub);
    }

    /**
     * To get access token to access Telerik report server
     * @returns
     */
    private _getReportToken$(): Observable<string> {

        // call server to get token
        return this.schedulingFormService.getTelerikAccessToken$()
            .pipe(
                tap(
                    (token: TelerikAccessTokenModel) => {
                        this.reportSource.accessToken = token.access_token;
                    }
                )
            ).map(
                (token: TelerikAccessTokenModel) => token.access_token
            );
    }

    /**
     * To obtains report client
     * @returns
     */
    private _createReportClient$(): Observable<number> {
        const tokenString = this.reportSource.accessToken;

        return this._httpClient.post(
            this.reportSource.reportServerUrl + '/api/reports/clients',
            null,
            {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${tokenString}`,
                    'CSPortal-IgnoreAuthInterceptor': 'true'
                }
            }
        ).pipe(
            tap(
                (client: any) => {
                    this.reportSource.clientId = client.clientId;
                }
            )
        ).map(
            (client: any) => client.clientId
        );

    }

    /**
     * Fetch parameters of selected report from Telerik server
     * @param setValueFromModel
     * @returns
     */
    private _getTelerikReportParameters$(setValueFromModel: boolean): Observable<Array<TelerikReportParameterModel>> {
        const tokenString = this.reportSource.accessToken;
        const clientId = this.reportSource.clientId;

        const postData = {
            'report': `${this.reportSource.category}/${this.reportSource.name}.trdp`,
            'parameterValues': null
        };

        // try to get report params from local storage (set from telerik report preview)
        const reportParams = LocalStorageService.read<{ [key: string]: any }>('task-report-params');
        if (reportParams) {
            const keys = Object.keys(reportParams);
            keys?.map(key => {
                const value = reportParams[key];
                if (!StringHelper.isNullOrEmpty(value)) {
                    postData.parameterValues = postData.parameterValues || {};
                    postData.parameterValues[key] = value;
                }
            });
        }

        // In edit mode, get report params from model
        if (setValueFromModel) {
            postData.parameterValues = JSON.parse(this.model.parameters);
        }

        // If add mode, not internal and agent user
        if (this.isAddMode && !this.currentUser.isInternal && this.currentUser?.role?.id !== Roles.Agent) {
            postData.parameterValues = postData.parameterValues || {};
            postData.parameterValues['selectedCustomerId'] = this.currentUser.organizationId;
        }

        return this._httpClient.post(
            this.reportSource.reportServerUrl + '/api/reports/clients/' + clientId + '/parameters',
            postData,
            {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${tokenString}`,
                    'CSPortal-IgnoreAuthInterceptor': 'true'
                }
            }
        ).pipe(
            tap(
                (parameters: any) => {
                    this.reportSource.parameters = parameters;
                }
            )
        );

    }

    /**
     * To build dynamic inputs/controls on Filtering Criteria section
     */
    private _buildFilteringCriteriaSection() {
        const isDisableCustomerId = this.currentUser?.role?.id === Roles.Principal;
        this.isUpdatingReportFiltering = true;
        const formControl: { [key: string]: AbstractControl } = {};
        const reportParams = this.reportSource.parameters || [];

        // Init selected report columns preview
        this.selectedColumns = new Array<ColumnSetting>();
        reportParams?.find(x => x.id === 'selectedColumns')?.value.map(y => {
            this.selectedColumns.push({
                title: y,
                field: y,
                filter: 'text',
                width: '200px'
            });
        });

        this.columnOptionsDataSource = new Array<ColumnOptionModel>();
        reportParams?.find(x => x.id === 'selectedColumns')?.availableValues.map(el => {
            this.columnOptionsDataSource.push({
                name: el.name
            });
        });


        // Remove previous control
        const ignoreKeys = ['name', 'report', 'documentFormat', 'startDate'];
        let formControlKeys = Object.keys(this.mainForm.controls);
        if (formControlKeys) {
            formControlKeys.map(
                (key: string) => {
                    if (ignoreKeys.indexOf(key) < 0) {
                        this.mainForm.form.removeControl(key);
                        if (this.validationRules[key]) {
                            delete this.validationRules[key];
                        }
                    }
                }
            );
        }

        // Add custom logic to hidden params
        const hiddenParameterNames = ['validationResult', 'tmpCalculation1', 'tmpCalculation2', 'tmpCalculation3'];
        if (StringHelper.caseIgnoredCompare(this.reportSource.name, 'Master Summary Report (PO Level)')
            || StringHelper.caseIgnoredCompare(this.reportSource.name, 'Master Summary Report (Item Level)')) {
            [
                'etdFrom',
                'etdTo',
                'etaFrom',
                'etaTo',
                'cargoReadyDateFrom',
                'cargoReadyDateTo',
                'atdFrom',
                'atdTo',
                'periodDays'
            ].map(x => {
                hiddenParameterNames.push(x);
            });
        }

        const visibleParams = reportParams.filter(x => hiddenParameterNames.indexOf(x.name) < 0);
        visibleParams.map(
            x => {
                x.filteredAvailableValues = Object.assign([], x.availableValues);

                if (x.type === 'System.DateTime') {
                    if (!StringHelper.isNullOrEmpty(x.value)) {
                        x.value = moment(x.value).toDate();
                    } else {
                        x.value = null;
                    }
                }
                if (x.multivalue) {
                    const values = x.value?.map((y: any) => {
                        return { name: y, value: y };
                    });
                    x.value = values;
                }

                // Add custom logic to make params required
                if (StringHelper.caseIgnoredCompare(this.reportSource.name, 'Master Summary Report (PO Level)')
                    || StringHelper.caseIgnoredCompare(this.reportSource.name, 'Master Summary Report (Item Level)')) {
                    const requiredParams = [
                        'filteringDate',
                        'dateRange',
                        'fromDate',
                        'toDate',
                        'numberOfDateBefore',
                        'numberOfDateFromNow'
                    ];
                    if (requiredParams.indexOf(x.name) >= 0) {
                        x.allowNull = false;
                    }
                }
                if (isDisableCustomerId && StringHelper.caseIgnoredCompare(x.name, 'selectedCustomerId')) {
                    formControl[x.name] = x.allowNull ? new FormControl({ value: null, disabled: true }) : new FormControl({ value: null, disabled: true }, Validators.required);
                    // If add mode, not internal and agent user
                    if (this.isAddMode && !this.currentUser.isInternal && this.currentUser?.role?.id !== Roles.Agent) {
                        x.value = this.currentUser.organizationId;
                    }
                } else {
                    formControl[x.name] = (x.allowNull || x.name === 'selectedColumns') ? new FormControl() : new FormControl(null, Validators.required);
                }
                if (!x.allowNull && !this.validationRules[x.name] && x.name !== 'selectedColumns') {
                    this.validationRules[x.name] = {
                        'required': x.text
                    };
                }

            }
        );
        this.reportFilteringFormGroup = new FormGroup(formControl);
        this.reportFilteringFormFields = visibleParams;

        const dateRangeField = this.reportFilteringFormFields.find(c => c.name === 'dateRange');
        setTimeout(() => {
            this.toggleControl(dateRangeField?.name, dateRangeField?.value);
        });

        // Add control into mainform
        formControlKeys = Object.keys(formControl);
        if (formControlKeys) {
            formControlKeys.map(
                (key: string) => {
                    const control = formControl[key];
                    this.mainForm.form.addControl(key, control);
                }
            );
        }

        // Assume that report filtering rendering finished after 2s
        setTimeout(() => {
            this.isUpdatingReportFiltering = false;
        }, 2000);
    }

    /**
     * Reset/remove all inputs/controls on Filtering Criteria section
     */
    private _resetReportFilteringSection() {
        this.reportSource.name = null;
        this.reportSource.category = null;
        this.reportSource.clientId = null;
        this.reportSource.parameters = null;
        this._buildFilteringCriteriaSection();
    }

    onColumnOptionsBtnClick(event): void {
        this.columnOptionsDialogOpened = true;

        this.columnOptionsDataSource?.forEach(el => {
            const currentIndex = this.selectedColumns?.map(y => y.field)?.indexOf(el.name);
            el.selected = currentIndex > -1;
            el.sequence = currentIndex > -1 ? (currentIndex + 1) : null;
        });
    }

    onColumnOptionsDialogSaved(data: ColumnOptionModel[]): void {
        this.columnOptionsDialogOpened = false;

        this.selectedColumns = new Array<ColumnSetting>();
        data.forEach(el => {
            this.selectedColumns.push({
                field: el.name,
                title: el.name,
                filter: 'text',
                width: '200px'
            })
        });

        // bind data to form field
        let formField = this.reportFilteringFormFields?.find(x => x.name === 'selectedColumns');
        if (formField) {
            formField.value = this.selectedColumns?.map((x: any) => {
                return { name: x.field, value: x.field };
            });
        }
    }

    onColumnOptionsDialogClosed(): void {
        this.columnOptionsDialogOpened = false;
    }

    /**
     * To handle event as any input value changed on Filtering Criteria section
     * @param value
     * @param name
     * @returns
     */
    public onReportFilteringValueChange(value: any, name: string) {

        // If Filtering Criteria section is updating, ignore
        if (this.isUpdatingReportFiltering) {
            return;
        }

        // Only refresh if there is any other dependent parameter on current one
        const parameter = this.reportSource.parameters.find(x => x.name === name);
        if (!parameter?.hasChildParameters) {
            return;
        }

        // Refresh after 1s
        clearTimeout(this.acpTimeout);
        this.acpTimeout = setTimeout(() => {
            this.isUpdatingReportFiltering = true;
            this._refreshReportFilteringSection(value, name);
            this.toggleControl(name, value, true);
        }, 1000);


    }

    /**
     * To show/hide/disable/enable control by business logic
     * @param name
     * @param value
     */
    toggleControl(name: string, value: string, resetCascadingParams: boolean = false) {
        // Ignore if it is Master Summary Report
        if (!StringHelper.caseIgnoredCompare(this.reportSource.name, 'Master Summary Report (PO Level)')
            && !StringHelper.caseIgnoredCompare(this.reportSource.name, 'Master Summary Report (Item Level)')) {
            return;
        }

        const dateRangeField = this.reportFilteringFormFields.find(c => c.name === 'dateRange');
        const fromDateField = this.reportFilteringFormFields.find(c => c.name === 'fromDate');
        const toDateField = this.reportFilteringFormFields.find(c => c.name === 'toDate');
        const numberOfDateBefore = this.reportFilteringFormFields.find(c => c.name === 'numberOfDateBefore');
        const numberOfDateFromNow = this.reportFilteringFormFields.find(c => c.name === 'numberOfDateFromNow');

        const selectedColumnElements = document.getElementById("selectedColumns");
        const numberOfDateBeforeElement = document.getElementById("numberOfDateBefore");
        selectedColumnElements.insertAdjacentHTML('beforebegin', '<div class="break-line-row"></div>');

        switch (name) {
            case 'filteringDate':
                if (!value) {
                    dateRangeField.isDisabled = true;
                    this.resetControl('dateRange');
                } else {
                    dateRangeField.isDisabled = false;
                }
                break;

            case 'dateRange':
                const blankColElements = document.getElementsByClassName('blank-col');
                while (blankColElements.length > 0) {
                    blankColElements[0].parentNode.removeChild(blankColElements[0]);
                }

                if (value === 'Specific Date Range') {
                    fromDateField.isHidden = false;
                    toDateField.isHidden = false;
                    if (!this.getFromControl('fromDate').value && !this.getFromControl('toDate').value) {
                        this.resetControl('numberOfDateBefore', 0);
                        this.resetControl('numberOfDateFromNow', 0);
                    }

                } else {

                    fromDateField.isHidden = true;
                    toDateField.isHidden = true;
                    this.resetControl('fromDate');
                    this.resetControl('toDate');

                    if (resetCascadingParams) {
                        this.resetControl('numberOfDateBefore', 1);
                        this.resetControl('numberOfDateFromNow', 1);
                    }

                    if (!value || value === 'MTD' || value === 'YTD') {
                        numberOfDateBeforeElement.insertAdjacentHTML('afterend', '<div class="blank-col col-lg-3"></div>');
                        numberOfDateBeforeElement.insertAdjacentHTML('afterend', '<div class="blank-col col-lg-3"></div>');
                        this.resetControl('numberOfDateBefore', 0);
                        this.resetControl('numberOfDateFromNow', 0);
                    }
                }

                if (value?.startsWith('By')) {
                    numberOfDateBefore.isHidden = false;
                    numberOfDateFromNow.isHidden = false;
                    if (value === 'By days') {
                        numberOfDateBefore.text = "No. of day before";
                        numberOfDateFromNow.text = "No. of day from now";
                    }
                    if (value === 'By weeks') {
                        numberOfDateBefore.text = "No. of week before";
                        numberOfDateFromNow.text = "No. of week from now";
                    }
                    if (value === 'By months') {
                        numberOfDateBefore.text = "No. of month before";
                        numberOfDateFromNow.text = "No. of month from now";
                    }

                } else {
                    numberOfDateBefore.isHidden = true;
                    numberOfDateFromNow.isHidden = true;
                }
                if (this.isAddMode && !this.getFromControl('filteringDate').value) {
                    dateRangeField.isDisabled = true;
                }
                break;
            default:
                break;
        }
    }


    /**
     * To handle event filtering for control on Filtering Criteria section
     * @param value
     * @param name
     */
    public onReportFilteringFilterChange(value: any, name: string) {
        const parameter = this.reportSource.parameters.find(x => x.name === name);
        parameter.filteredAvailableValues = parameter.availableValues.filter(x => x.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }

    /**
     * To get values of Filtering Criteria section
     */
    private get _reportFilteringValues() {
        const filterSetModel = {
        };

        const formControls = this.reportFilteringFormGroup.controls;
        const formControlKeys = Object.keys(formControls);
        if (formControlKeys) {
            formControlKeys.map(
                (key: string) => {
                    const formControl = formControls[key];
                    const reportParameter = this.reportSource.parameters.find(x => x.id === key);
                    if (reportParameter) {
                        if (reportParameter.type === 'System.DateTime') {
                            filterSetModel[key] = StringHelper.isNullOrEmpty(formControl.value) ? null : moment(formControl.value).format('YYYY-MM-DDTHH:mm:ss');
                        } else if (reportParameter.multivalue) {
                            let values = formControl.value?.map(
                                x => x.value
                            );

                            if (key === 'selectedColumns') {
                                values = reportParameter.value.map(
                                    x => x.value
                                );
                            }

                            filterSetModel[key] = values;
                        } else {
                            filterSetModel[key] = formControl.value === '' ? null : formControl.value;
                        }
                    }
                }
            );
        }
        return filterSetModel;
    }

    /**
     * In case there is cascading parameter, a method will update correct related value
     * @param value
     * @param name
     * @returns
     */
    private _refreshReportFilteringSection(value: any, name: string) {

        const parameter = this.reportSource.parameters.find(x => x.name === name);
        if (parameter?.hasChildParameters) {
            // Get selected report parameters' values

            const cascadingParameters = parameter.childParameters;
            const filterSetModel = this._reportFilteringValues;

            const tokenString = this.reportSource.accessToken;
            const clientId = this.reportSource.clientId;
            const postData = {
                report: `NAME/${this.reportSource.category}/${this.reportSource.name}/`,
                parameterValues: filterSetModel
            };
            return this._httpClient.post(
                this.reportSource.reportServerUrl + '/api/reports/clients/' + clientId + '/parameters',
                postData,
                {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${tokenString}`,
                        'CSPortal-IgnoreAuthInterceptor': 'true'
                    }
                }
            ).pipe(
                tap(
                    (parameters: Array<TelerikReportParameterModel>) => {
                        // Update report parameters
                        cascadingParameters.map(cascadingParamName => {
                            const newValue = parameters.find(x => x.name === cascadingParamName);
                            newValue.filteredAvailableValues = newValue.availableValues;
                            let currentValue = this.reportSource.parameters.find(x => x.name === cascadingParamName);
                            let currentIndex = this.reportSource.parameters.indexOf(currentValue);
                            this.reportSource.parameters[currentIndex] = newValue;

                            currentValue = this.reportFilteringFormFields.find(x => x.name === cascadingParamName);
                            currentIndex = this.reportFilteringFormFields.indexOf(currentValue);
                            this.reportFilteringFormFields[currentIndex] = newValue;

                        });
                    }
                )
            ).subscribe(() => {
                this.isUpdatingReportFiltering = false;
            });
        }
    }

    /**
     * Reset value and clear error of form control
     * @param controlName
     */
    resetControl(controlName: string, value?: any) {
        const control = this.mainForm.form.controls[controlName];
        control.setValue(value);
        this.setValidControl(controlName);
    }

    getFromControl(controlName: string) {
        return this.mainForm?.form?.controls[controlName];
    }

    // End Handler for Filtering Criteria section

    // Handler for Subscribers section

    /**
     * To handle adding new subscriber
     */
    onAddNewEmailRowBtnClick() {
        this.model.subscribers = this.model.subscribers || [];
        this.model.subscribers.push({ isAddLine: true });
        this.isNewEmailAdding = true;
    }

    /**
     * To check if email valid
     * @param value
     * @param rowIndex
     */
    checkEmailValid(value, rowIndex) {

        this.model.subscribers[rowIndex].email = value;

        // In case that email is empty, show required validation message
        if (StringHelper.isNullOrEmpty(value)) {
            this.model.subscribers[rowIndex].isValid = false;
            this.model.subscribers[rowIndex].validationMessage = this.translateService.instant('validation.requiredField',
                {
                    fieldName: this.translateService.instant('label.email')
                });
            return;
        }

        // Check multi emails and duplication
        const isValidEmails = this._validateMultiEmailAddresses(value);
        const isDuplicated = this.model.subscribers.filter(x => x.email === value).length > 1;
        if (isValidEmails && !isDuplicated) {
            this.model.subscribers[rowIndex].isValid = true;
        } else {
            this.model.subscribers[rowIndex].isValid = false;

            // Invalid multi emails
            if (!isValidEmails) {
                this.model.subscribers[rowIndex].validationMessage = this.translateService.instant('validation.invalidMultipleEmails');
            }

            // Duplicated
            if (isDuplicated) {
                this.model.subscribers[rowIndex].validationMessage = this.translateService.instant('validation.cannotBeDuplicated',
                    {
                        fieldName: this.translateService.instant('label.email')
                    });
            }

        }
    }

    private _validateMultiEmailAddresses(inputValue: string): boolean {
        let isValid = true;
        if (StringHelper.isNullOrEmpty(inputValue)) {
            return false;
        }
        const patternEmail = EmailValidationPattern;
        const emails = inputValue.split(',');
        emails?.map(email => {
            if (!patternEmail.test(email)) {
                isValid = false;
            }
        });
        return isValid;
    }

    /**
     * As adding new blank subscriber
     * @param dataItem
     * @param index
     */
    onAddEmailBtnClick(dataItem, index) {
        this.isNewEmailAdding = false;
        dataItem.isAddLine = false;
        this._setSubscribers();
    }

    /**
     * To handle removing subscriber
     * @param dataItem
     * @param index
     */
    onRemoveEmailBtnClick(dataItem: SubscriberModel, rowIndex: number) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.removeSubscriber', 'label.scheduling');

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.model.subscribers.splice(rowIndex, 1);
                    if (!dataItem.isAddLine) {
                        this._removeSubscriber(dataItem);
                    }
                    this.isNewEmailAdding = false;
                }
            });
        this._subscriptions.push(sub);
    }

    /**
     * To add new email
     */
    private _setSubscribers() {

        const postData = this.model.subscribers?.map(x => x.email) || [];
        const sub = this.schedulingFormService.setSubscribers$(this.model.id, this.model.telerikSchedulingId, postData)
            .subscribe(
                x => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.scheduling');
                }
            );
        this._subscriptions.push(sub);
    }

    /**
     * To remove email
     * @param subscriber
     */
    private _removeSubscriber(subscriber: SubscriberModel) {

        const sub = this.schedulingFormService.removeSubscriber$(this.model.id, this.model.telerikSchedulingId, subscriber.id, subscriber.email)
            .subscribe(
                x => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.scheduling');
                }
            );
        this._subscriptions.push(sub);
    }

    // End handler for Subscribers section

    // Section form

    /**
     * Proceed further steps after getting model data from server
     * @param data
     */
    onInitDataLoaded(data) {
        if (this.model === null) {
            return;
        }
        // If view/edit mode, auto build dynamic filter set
        if (!this.isAddMode) {
            const selectedReport = this.model?.csPortalReport;
            if (selectedReport) {
                const params = (new URL(`http://dummyhost/${selectedReport.reportUrl}`)).searchParams;
                this.reportSource.name = params.get('reportkey');
                this.reportSource.reportServerUrl = params.get('reportserverurl');
                this.reportSource.categoryId = selectedReport.telerikCategoryId;
                this.reportSource.category = selectedReport.telerikCategoryName;
                this.reportSource.reportId = selectedReport.telerikReportId;
                // Dynamic report filter set build
                this._fetchTelerikReportParameters(true);
            }

        } else {
            // If adding mode

            // Set default values
            this.model.documentFormat = 'XLSX';
            const now = moment();
            const nearestHour = now.startOf('hour').add(1, 'hour');
            this.model.startDate = nearestHour.toDate();

            // there is selected report from url, auto build filtering criteria section
            if (this.urlParams?.reportId) {
                this.model.csPortalReportId = +this.urlParams.reportId;

                // There is some case request report dropdown not finished yet, wait 1s
                setTimeout(function (component) {
                    component.onReportValueChange(component.model.csPortalReportId);
                }, this.reportOptionsDataSource.length === 0 ? 1000 : 0, this);
            }

            // Add default current user email
            this.model.subscribers = this.model.subscribers || [];
            this.model.subscribers.push({
                email: this.currentUser.email,
                isValid: true,
                isAddLine: false
            });
        }
    }

    breakLineEmailSubscribers(email: string) {
        return email?.replace(/,/g, ",\n");
    }

    ngOnDestroy(): void {
        this._subscriptions.map(
            x => x.unsubscribe()
        );
    }

    /**
     * To handle save button
     * @returns
     */
    onSaveBtnClick() {

        // Validate inputs on main form, but not in Filtering Criteria section
        this.validateAllFields(false);
        if (this.mainForm.valid) {
            // Remove local storage from telerik report preview
            LocalStorageService.remove('task-report-params');

            // Validate Filtering Criteria section: all required fields + logic from telerik report server
            const validationResult = this.reportSource.parameters.find(x => x.name === 'validationResult');
            let validationResultValid = true;
            if (validationResult) {
                validationResultValid = validationResult.value || false;
            } else {
                this.notification.showErrorPopup('validation.mandatoryFieldsValidation', 'label.scheduling');
                return;
            }

            // Prepare data to save
            const postData: SchedulingModel = Object.assign({}, this.model);

            postData.documentFormatDescr = this.documentTypes.find(x => x.value === postData.documentFormat).label;
            postData.parameters = this._reportFilteringValues ? JSON.stringify(this._reportFilteringValues) : '';
            postData.report = this.reportSource.name;
            postData.reportId = this.reportSource.reportId;
            postData.category = this.reportSource.category;
            postData.categoryId = this.reportSource.categoryId;
            postData.mailTemplateSubject = 'Telerik Report Server - Task Scheduler notification';
            postData.mailTemplateBody = '<p>Hello {FirstName},</p><p><em>{ReportName}</em> report has been generated based on the following scheduled task:&nbsp;<strong>{TaskName}</strong>.</p><p>The report is available as an attachment to this e-mail.</p>';

            this.isReadyForSubmit = false;
            if (this.isAddMode) {
                postData.enabled = true;
                this.schedulingFormService.createNewScheduling$(postData).subscribe(
                    data => {
                        this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                        this.modelId = data.id;
                        this.model = data;
                        this.router.navigate([`/scheduling/view/${data.id}`]);
                        this.isReadyForSubmit = true;
                    },
                    error => {
                        let errorString = this.translateService.instant('save.failureNotification');
                        if (error?.error?.title) {
                            errorString = `${error.error.title}`;
                        }
                        this.notification.showErrorPopup(errorString, 'label.scheduling');
                        this.isReadyForSubmit = true;
                    }
                );
            } else {
                this.schedulingFormService.updateScheduling$(postData).subscribe(
                    data => {
                        this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                        this.router.navigate([`/scheduling/view/${this.model.id}`]);
                        this.model = data;
                        this.isReadyForSubmit = true;
                    },
                    error => {
                        this.notification.showErrorPopup('save.failureNotification', 'label.scheduling');
                        this.isReadyForSubmit = true;
                    }
                );
            }

        } else {
            this.notification.showErrorPopup('validation.mandatoryFieldsValidation', 'label.scheduling');
        }
    }

    /**
     * To handle cancel button
     */
    onCancelBtnClick() {
        const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.scheduling');

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    if (this.isAddMode) {
                        this.backList();
                    } else {
                        if (this.isEditMode) {
                            this.router.navigate([`/scheduling/view/${this.model.id}`]);
                            this.ngOnInit();
                        }
                    }
                }
            });
        this._subscriptions.push(sub);
    }

    // Section Activities section

    onRemoveActivityBtnClick(dataItem: TelerikActivityModel, rowIndex: number) {

        const confirmDlg = this.notification.showConfirmationDialog('msg.removeActivity', 'label.scheduling');

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.model.activities.splice(rowIndex, 1);
                    this.schedulingFormService.removeActivity$(this.model.id, dataItem.id, dataItem.taskId).subscribe(
                        x => {
                            this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.scheduling');
                        }
                    );
                }
            });
        this._subscriptions.push(sub);

    }

    /**
     *
     * @param telerikDocument To download document
     */
    downloadTelerikDocument(telerikDocument: TelerikActivityModel) {

        const tokenString = this.reportSource.accessToken;
        const sub = this._httpClient.get(
            this.reportSource.reportServerUrl + `/Scheduling/DownloadDocument/${telerikDocument.id}`,
            {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${tokenString}`,
                    'CSPortal-IgnoreAuthInterceptor': 'true'
                },
                responseType: 'blob'
            }
        ).subscribe(
            data => {
                const a = document.createElement('a');
                const url = window.URL.createObjectURL(data);
                a.href = url;
                a.download = telerikDocument.documentName;
                document.body.append(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
            }
        );

        this._subscriptions.push(sub);
    }


    // End section Activities section

    // Section top menu

    /**
     * Check whether current user is able to edit
     */
    get hasEditPermission() {
        return this.currentUser.permissions.some(x => x.name === AppPermissions.Reports_TaskDetail_Edit);
    }

    /**
     * To go to the list
     */
    backList() {
        this.router.navigate(['/scheduling']);
    }

    /**
     * Click edit button
     */
    onEditBtnClick() {
        this.router.navigate([`/scheduling/edit/${this.model.id}`]);
    }

    /**
     * Click execute button
     */
    onExecuteBtnClick() {

        const confirmDlg = this.notification.showConfirmationDialog('msg.executeScheduling', 'label.scheduling');
        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.schedulingFormService.executeScheduling$(this.model.id, this.model.telerikSchedulingId)
                        .subscribe(
                            x => {
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                            },
                            error => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.scheduling');
                            }
                        );
                }
            });
        this._subscriptions.push(sub);
    }

    /**
     * Click deactivate button
     */
    onDeactivateBtnClick() {

        const confirmDlg = this.notification.showConfirmationDialog('msg.deactivateScheduling', 'label.scheduling');
        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.schedulingFormService.deactivateScheduling$(this.model.id, this.model.telerikSchedulingId)
                        .subscribe(
                            x => {
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                                this.model.status = SchedulingStatus.Inactive;
                            },
                            error => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.scheduling');
                            }
                        );
                }
            });
        this._subscriptions.push(sub);
    }

    /**
     * Click activate button
     */
    onActivateBtnClick() {

        const confirmDlg = this.notification.showConfirmationDialog('msg.activateScheduling', 'label.scheduling');
        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.schedulingFormService.activateScheduling$(this.model.id, this.model.telerikSchedulingId)
                        .subscribe(
                            x => {
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                                this.model.status = SchedulingStatus.Active;
                            },
                            error => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.scheduling');
                            }
                        );
                }
            });
        this._subscriptions.push(sub);
    }

    /**
     * Click delete button
     */
    onDeleteBtnClick() {

        const confirmDlg = this.notification.showConfirmationDialog('msg.removeTask', 'label.scheduling');

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.schedulingFormService.deleteScheduling$(this.model.id, this.model.telerikSchedulingId)
                        .subscribe(
                            x => {
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.scheduling');
                                this.router.navigate(['/scheduling']);
                            },
                            error => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.scheduling');
                            }
                        );
                }
            });
        this._subscriptions.push(sub);
    }

    // End section top menu

    protected fieldValue(name) {
        return this.mainForm.controls[`${name}`].value;
    }

    onChangeCheckBox(controlName) {
        this.mainForm.controls[controlName].setValue(!this.fieldValue(controlName));
    }
}
