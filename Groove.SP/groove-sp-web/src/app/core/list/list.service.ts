import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, DataSourceRequestState, toDataSourceRequestString } from '@progress/kendo-data-query';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';
import { StringHelper } from '../helpers';
import { tap, map } from 'rxjs/operators';
import * as cloneDeep from 'lodash/cloneDeep';
import { HttpService } from '..';
import { environment } from 'src/environments/environment';
import { ConsolidationStage, EquipmentStringType, EquipmentType, PaymentStatusType } from '../models/enums/enums';
import { ViewSettingModel } from '../models/viewSetting.model';
import { DynamicView } from '../models/constants/app-constants';

export abstract class ListService extends BehaviorSubject<GridDataResult> {
    public state: DataSourceRequestState = {
        sort: [],
        skip: 0,
        take: 20
    };
    public dataCount: number = 0;
    public serviceData: any[];
    public viewSettings: ViewSettingModel[] = [];
    public gridLoading: boolean = false;
    public listModeNumber: any = null;
    public quickTrackNumber = '';
    public affiliates: any = null;
    public roleId: number = 0;
    public organizationId: number = 0;
    public otherQueryParams: { [key: string]: any } = {};
    public parentId: any = null;
    
    /** To get the list of visible column names (field: id, poNo, cargoReadyDate,...) */
    public get visibleColumns(): string[] {
        // Ignore 'viewSettingModuleId'
        if (StringHelper.isNullOrEmpty(this.viewSettings)) {
            return [];
        }
        return this.viewSettings.map( x => x.field).filter(x => !StringHelper.caseIgnoredCompare(x, DynamicView.moduleKey));
    }


    /**
     * Define the statistic identifier. It must be an unique key for each statistics.
     */
    public statisticKey: any = null;

    /**
     * Define selected statistic date range. For example: This week, Next 14 days,...
     */
    public statisticFilter: any = null;

    /**
     * Define selected statistic value for further filtering purposes
     */
    public statisticValue: any = null;

    public userRole: string;

    public formListId: any = null;
    public defaultState: DataSourceRequestState = <DataSourceRequestState>{ sort: [], skip: 0, take: 20 };

    public initDataLoaded: BehaviorSubject<any> = new BehaviorSubject(null);
    public organizationStatus: Array<{ text: string, value: number }> = [
        { text: 'label.inactive', value: 0 },
        { text: 'label.active', value: 1 },
        { text: 'label.pending', value: 2 },
    ];

    public connectionType: Array<{ text: string, value: number }> = [
        { text: 'label.active', value: 1 },
        { text: 'label.pending', value: 2 },
        { text: 'label.inactive', value: 3 },
    ];

    public buyerComplianceStatus: Array<{ text: string, value: number }> = [
        { text: 'label.inactive', value: 0 },
        { text: 'label.active', value: 1 },
    ];

    public poFulfillmentStage: Array<{ text: string, value: number }> = [
        { text: 'label.draft', value: 10 },
        { text: 'label.forwarderBookingRequest', value: 20 },
        { text: 'label.forwarderBookingConfirmed', value: 30 },
        { text: 'label.cargoReceived', value: 35 },
        { text: 'label.shipmentDispatch', value: 40 },
        { text: 'label.closed', value: 50 },
    ];

    public poFulfillmentStatus: Array<{ text: string, value: number }> = [
        { text: 'label.active', value: 10 },
        { text: 'label.inactive', value: 20 }
    ];

    public vesselStatus: Array<{ text: string, value: number }> = [
        { text: 'label.active', value: 1 },
        { text: 'label.inactive', value: 0 }
    ];

    public carrierStatus: Array<{ text: string, value: number }> = [
        { text: 'label.active', value: 1 },
        { text: 'label.inactive', value: 0 }
    ];

    public contractMasterStatus: Array<{ text: string, value: number }> = [
        { text: 'label.active', value: 1 },
        { text: 'label.inactive', value: 0 }
    ];

    public buyerComplianceStage: Array<{ text: string, value: number }> = [
        { text: 'label.cancelled', value: 0 },
        { text: 'label.activated', value: 1 },
        { text: 'label.draft', value: 2 },
    ];

    public integrationLogStatus: Array<{ text: string, value: number }> = [
        { text: 'label.succeed', value: 1 },
        { text: 'label.failed', value: 2 },
    ];

    public userStatus: Array<{ text: string, value: number }> = [
        { text: 'label.active', value: 2 },
        { text: 'label.inactive', value: 3 },
        { text: 'label.waitForConfirm', value: 4 }
    ];

    public roleStatus: Array<{ text: string, value: number }> = [
        { text: 'label.inactive', value: 0 },
        { text: 'label.active', value: 1 },
    ];

    public userRequestStatus: Array<{ text: string, value: number }> = [
        { text: 'label.pending', value: 1 },
        { text: 'label.rejected', value: 0 },
    ];

    public purchaseOrderStatus: Array<{ text: string, value: number }> = [
        { text: 'label.cancel', value: 0 },
        { text: 'label.active', value: 1 },
    ];

    public cruiseOrderStatus: Array<{ text: string, value: string }> = [
        { text: 'label.cancel', value: 'Cancel' },
        { text: 'label.active', value: 'Active' },
    ];

    public schedulingStatus: Array<{ text: string, value: number }> = [
        { text: 'label.inactive', value: 0 },
        { text: 'label.active', value: 1 },
    ];

    public modeOfTransport: Array<{ text: string, value: string }> = [
        { text: 'label.sea', value: 'Sea' },
        { text: 'label.air', value: 'Air' },
    ];

    public carrierModeOfTransport: Array<{ text: string, value: string }> = [
        { text: 'label.sea', value: 'Sea' },
        { text: 'label.air', value: 'Air' },
        { text: 'label.road', value: 'Road' },
        { text: 'label.railway', value: 'Railway' },
        { text: 'label.courier', value: 'Courier' },
    ];

    public poStageType: Array<{ text: string, value: number }> = [
        { text: 'label.released', value: 20 },
        { text: 'label.forwarderBookingRequest', value: 30 },
        { text: 'label.forwarderBookingConfirmed', value: 40 },
        { text: 'label.cargoReceived', value: 45 },
        { text: 'label.shipmentDispatch', value: 50 },
        { text: 'label.closed', value: 60 },
    ];

    public masterDialogCategory: Array<{ text: string, value: string }> = [
        { text: 'label.general', value: 'General' }
    ];

    public displayOnType: Array<{ text: string, value: string }> = [
        { text: 'label.purchaseOrders', value: 'Purchase Orders' },
        { text: 'label.bookings', value: 'Bookings' },
        { text: 'label.shipments', value: 'Shipments' }
    ];

    public filterCriteriaType: Array<{ text: string, value: string }> = [
        { text: 'label.masterBLNo', value: 'Master BL No.' },
        { text: 'label.houseBillOfLadingNumber', value: 'House BL No.' },
        { text: 'label.containerNo', value: 'Container No.' },
        { text: 'label.purchaseOrderNo', value: 'Purchase Order No.' },
        { text: 'label.fulfillmentNumber', value: 'Booking No.' },
        { text: 'label.shipmentNo', value: 'Shipment No.' }
    ];

    public cruiseOrderStageType: Array<{ text: string, value: string }> = [
        // { text: 'label.released', value: 'Release' }
    ];

    public consolidationStage: Array<{ text: string, value: string }> = [
        { text: 'label.new', value: 'New' },
        { text: 'label.confirmed', value: 'Confirmed' }
    ];

    public containerType: Array<{ text: string, value: string }> = [
        { text: 'label.twentyGP', value: EquipmentStringType.TwentyGP },
        { text: 'label.twentyNOR', value: EquipmentStringType.TwentyNOR },
        { text: 'label.twentyRF', value: EquipmentStringType.TwentyRF },
        { text: 'label.twentyHC', value: EquipmentStringType.TwentyHC },
        { text: 'label.fourtyGP', value: EquipmentStringType.FourtyGP },
        { text: 'label.fourtyNOR', value: EquipmentStringType.FourtyNOR },
        { text: 'label.fourtyRF', value: EquipmentStringType.FourtyRF },
        { text: 'label.fourtyHC', value: EquipmentStringType.FourtyHC },
        { text: 'label.fourtyFiveHC', value: EquipmentStringType.FourtyFiveHC }
    ]

    public buyerApprovalStage: Array<{ text: string, value: number }> = [
        { text: 'label.pending', value: 10 },
        { text: 'label.approved', value: 20 },
        { text: 'label.rejected', value: 30 },
        { text: 'label.cancelled', value: 40 },
        { text: 'label.overdue', value: 50 }
    ];

    public exceptionType: Array<{ text: string, value: number }> = [
        { text: 'label.poFulfillmentException', value: 10 },
        { text: 'label.shipmentException', value: 20 },
        { text: 'label.consignmentException', value: 30 },
        { text: 'label.containerException', value: 40 },
        { text: 'label.consolidationException', value: 50 }
    ];

    public invoiceType: Array<{ text: string, value: string }> = [
        { text: 'label.normalInvoice', value: 'N' },
        { text: 'label.statementInvoice', value: 'F' },
        { text: 'label.manualInvoice', value: 'I' },
    ];

    public paymentStatusType: Array<{ text: string, value: number }> = [
        { text: 'label.paid', value: PaymentStatusType.Paid },
        { text: 'label.partial', value: PaymentStatusType.Partial },
    ];

    public surveyStatus: Array<{ text: string, value: number }> = [
        { text: 'label.draft', value: 10 },
        { text: 'label.published', value: 20 },
        { text: 'label.closed', value: 30 }
    ];

    public incotermType: Array<{ text: string, value: number }> = [
        { text: 'EXW', value: 1 << 0 },
        { text: 'FCA', value: 1 << 1 },
        { text: 'CPT', value: 1 << 2 },
        { text: 'CIP', value: 1 << 3 },
        { text: 'DAT', value: 1 << 4 },
        { text: 'DAP', value: 1 << 5 },
        { text: 'DDP', value: 1 << 6 },
        { text: 'FAS', value: 1 << 7 },
        { text: 'FOB', value: 1 << 8 },
        { text: 'CFR', value: 1 << 9 },
        { text: 'CIF', value: 1 << 10 },
        { text: 'DPU', value: 1 << 11 }
    ];

    public incotermStringType: Array<{ text: string, value: string }> = [
        { text: 'EXW', value: 'EXW' },
        { text: 'FCA', value: 'FCA' },
        { text: 'CPT', value: 'CPT' },
        { text: 'CIP', value: 'CIP' },
        { text: 'DAT', value: 'DAT' },
        { text: 'DAP', value: 'DAP' },
        { text: 'DDP', value: 'DDP' },
        { text: 'FAS', value: 'FAS' },
        { text: 'FOB', value: 'FOB' },
        { text: 'CFR', value: 'CFR' },
        { text: 'CIF', value: 'CIF' },
        { text: 'DPU', value: 'DPU' }
    ];

    public routingOrderStageType: Array<{ text: string, value: number }> = [
        { text: 'label.released', value: 20 },
        // { text: 'label.rateAccepted', value: 30 },
        // { text: 'label.rateConfirmed', value: 40 },
        { text: 'label.booked', value: 50 },
        { text: 'label.bookingConfirmed', value: 60 },
        { text: 'label.shipmentDispatch', value: 70 },
        { text: 'label.closed', value: 80 }
    ];

    public routingOrderStatus: Array<{ text: string, value: number }> = [
        { text: 'label.cancel', value: 0 },
        { text: 'label.active', value: 1 }
    ];

    
    constructor(protected httpService: HttpService, private _apiUrl: string) {
        super(null);
    }

    public queryToExport(): Observable<GridDataResult> {
        const state = Object.assign({}, this.state);
        delete state.skip;
        delete state.take;

        return this.fetch(this._apiUrl, state, true);
    }

    /** Fire request to server to get grid: data/total/viewSettings... */
    public query(state: any): void {
        this.fetch(this._apiUrl, state)
            .subscribe(x => {
                this.updateData(x);
            });
    }

    /** With data responded from server, update these information to local  */
    public updateData(x) {
        this.dataCount = x.total;
        this.serviceData = x.data;
        this.viewSettings = x.viewSettings;
        // After local grid changed, fire data to subscribers that may be on list.component.ts to handle dynamic columns
        super.next(x);
        this.gridLoading = false;
    }

    public queryQuickTracking(state: any): Observable<GridDataResult> {
        return this.fetch(this._apiUrl, state);
    }

    /**
     * As grid's state changed, this method will fire request to server
     * @param state
     */
    public dataStateChange(state: DataStateChangeEvent): void {
        this.state = Object.assign({}, state);
        this.query(this.state);
    }

    public pageChange(event: PageChangeEvent): void {
        this.state.skip = event.skip;
    }

    public pageSizeChange(value: any): void {
        this.state.skip = 0;
        this.state.take = value;
        this.query(this.state);
    }

    public sortChange(sort: SortDescriptor[]): void {
        this.state.sort = sort;
    }

    public fetch(_apiUrl: string, state: any, isExport = false): Observable<GridDataResult> {
        // preprocess the state
        const selectState = cloneDeep(state);
        if (selectState.filter) {
            selectState.filter.filters.forEach((x: any) => {
                if (typeof (x.value) === 'string') {
                    x.value = x.value.trim();
                } else if (x.field.endsWith('Time')) {
                    const tzOffsetMillisecond = x.value.getTimezoneOffset() * 60000;
                    x.value = new Date(x.value.getTime() + tzOffsetMillisecond);
                }
            });
        }
        // serialize the state
        const queryStr = `${toDataSourceRequestString(selectState)}`;

        let url = `${_apiUrl}/search?${queryStr}`;
        if (this.checkAffiliateApiPrefix(_apiUrl)) {
            const affiliatesString = !StringHelper.isNullOrEmpty(this.affiliates) ? this.affiliates : '';
            let otherQueryParamsString: string = '';
            const otherQueryParams = this.otherQueryParams;
            if (otherQueryParams) {
                const keys = Object.keys(otherQueryParams);
                const values = keys.map(key => {
                    // will not send to back-end if null/undefined/empty
                    if (!StringHelper.isNullOrEmpty(otherQueryParams[key])) {
                        return `&${key}=${otherQueryParams[key]}`;
                    }
                    return '';
                });
                values.forEach(value => {
                    otherQueryParamsString += value;
                });
            }

            if (this.quickTrackNumber.length > 0) {
                url = `${_apiUrl}/search/${this.quickTrackNumber}?${queryStr}&affiliates=${affiliatesString}`;
                if (_apiUrl.toLocaleLowerCase().includes("purchaseorders")) {
                    url = url + otherQueryParamsString;
                }

            } else {

                url = `${_apiUrl}/search?${queryStr}&affiliates=${affiliatesString}${otherQueryParamsString}`;
            }
        }

        if (this.checkChildrenApiPrefix(_apiUrl)) {
            url = `${_apiUrl}/search?${queryStr}&parentId=${this.parentId}`;
        }

        if (!StringHelper.isNullOrEmpty(this.formListId)) {
            url = `${_apiUrl}/search?${queryStr}&id=${this.formListId}`;
        }

        if (isExport) {
            url += '&isExport=true';
        }

        if (_apiUrl.includes('billOfLadings')) {
            url += `&roleId=${this.roleId}&organizationId=${this.organizationId}`;
        }

        this.gridLoading = true;

        return this.httpService
            .get(`${url}`)
            .pipe(
                map(({ data, total, viewSettings }: GridDataResult | any) =>
                (<GridDataResult>{
                    data: data,
                    total: total,
                    viewSettings : viewSettings
                })),
                tap((res) => {
                    this.gridLoading = false;
                    this.initDataLoaded.next(res);
                })
            );
    }


    public checkAffiliateApiPrefix(api: string) {
        // Must be lower cased
        const affiliateApiPrefix = [
            'vesselArrivals',
            'shipments',
            'containers',
            'billofladings',
            'masterbillofladings',
            'purchaseorders',
            'purchaseorders/shipped',
            'pofulfillments',
            'buyerapprovals',
            'consignments',
            'reports',
            'consolidations/internal',
            'articlemasters/internal',
            'shortships',
            'routingorders'
        ];

        if (affiliateApiPrefix.map(x => `${environment.apiUrl}/${x}`.toLocaleLowerCase()).indexOf(api.toLowerCase()) >= 0) {
            return true;
        }
        return false;
    }

    public checkChildrenApiPrefix(api: string) {
        const apiPrefix = [
            'bookingvalidationlogs'];

        if (apiPrefix.map(x => `${environment.apiUrl}/${x}`.toLocaleLowerCase()).indexOf(api.toLowerCase()) >= 0) {
            return true;
        }
        return false;
    }

    public downloadTemplate(templateUrl, fileName) {
        return this.httpService.downloadFile(templateUrl, fileName);
    }
}
