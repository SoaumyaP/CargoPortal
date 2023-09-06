/**
 * Usage: all activities of the provided entity will be shown as a timeline tree. It also provides the client-side filter feature.
 */

import {
    Component,
    Input,
    OnInit } from '@angular/core';
import {
    faMapMarkerAlt,
    faCheckCircle } from '@fortawesome/free-solid-svg-icons';
import moment from 'moment';
import { FilterActivityModel } from 'src/app/core/models/activity/filter-activity.model';
import { MilestoneEventCode } from 'src/app/core/models/constants/app-constants';
import {
    DATE_FORMAT,
    DropDownListItemModel,
    DropdownListModel,
    EntityType,
    FormModeType,
    StringHelper,
    UserContextService } from 'src/app/core';
import { Observable } from 'rxjs';
import { ArrayHelper } from 'src/app/core/helpers/array.helper';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import {
    ActivityMilestone,
    ActivityTimelineService,
    GetActivityTimelineRequest } from './activity-timeline.service';

@Component({
    selector: 'app-activity-timeline',
    templateUrl: './activity-timeline.component.html',
    styleUrls: ['./activity-timeline.component.scss']
})
export class ActivityTimelineComponent implements OnInit {
    /**
     * Id of entity.
     ** Hint: if entityType = SHI then it must be Shipment's Id.
     */
    @Input()
    entityId: number;
    
    /**
     * Type name of entity.
    */
    @Input()
    entityType: EntityType;

    /**
     * Filter-by options data-source.
     * */
    @Input()
    filterOptions: DropdownListModel<string>[];


    @Input()
    milestones: ActivityMilestone[];

    readonly milestoneEventCode = MilestoneEventCode;
    readonly stringHelper = StringHelper;
    formModeType = FormModeType;
    DATE_FORMAT = DATE_FORMAT;
    faMapMarkerAlt = faMapMarkerAlt;
    faCheckCircle = faCheckCircle;

    model: any;
    defaultPageSize = 5;

    pageSetting = {
        pageSize: this.defaultPageSize,
        pageIndex: 0,
        ascending: false,
        filterEventDate: '0001/1/1'
    };

    filterActivityModel: FilterActivityModel = new FilterActivityModel();
    filterActivityValueDataSource$: Observable<DropDownListItemModel<string>[]>;

    activityTotal = 0;
    showActionButton: boolean = false;
    showActivityList: boolean = false;
    showNoRecordMsg: boolean = false;

    groupModel = null;
    defaultMilestone: ActivityMilestone[] = [];

    isCanClickVessel: boolean;
    isCanClickContainerNo: boolean;
    isCanClickShipmentNo: boolean;
    isCanClickBookingNo: boolean;

    constructor(private _activityTimelineService: ActivityTimelineService,
        private _userContext: UserContextService
    ) {
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.isCanClickVessel = user.permissions?.some(c => c.name === AppPermissions.FreightScheduler_List);
                this.isCanClickContainerNo = user.permissions?.some(c => c.name === AppPermissions.Shipment_ContainerDetail);
                this.isCanClickShipmentNo = user.permissions?.some(c => c.name === AppPermissions.Shipment_Detail);
                this.isCanClickBookingNo = user.permissions?.some(c => c.name === AppPermissions.PO_Fulfillment_Detail);
            }
        });
    }

    async ngOnInit() {
        await this.getActivities();
        this.checkToShowActivityList();
        this.checkToShowActionButton();
    }

    async getActivities() {
        let reqModel: GetActivityTimelineRequest = {
            entityId: this.entityId,
            entityType: this.entityType,
            fromDate: '',
            toDate: '',
            filterBy: null,
            filterValue: null
        };
        const rs = await this._activityTimelineService.getActivities(reqModel).toPromise();
        this.activityTotal = rs.result.totalRecord;
        this.model = rs.result.globalIdActivities;

        this.mappingMilestone();
        this.groupActivity();

        this.milestones.forEach(elm => {
            if (StringHelper.isNullOrWhiteSpace(elm.activityCode)) {
                this.groupModel.unshift({
                    activities: [{
                        activity: { activityCode: '', milestone: elm.milestone, matchFilter: true }
                    }],
                    dateGroup: moment(new Date(elm.occurDate.toDateString())),
                    matchFilter: true
                });
            }
        });

        ArrayHelper.sortBy(this.groupModel, 'dateGroup', 'desc');
    }

    mappingMilestone() {
        const activityCodes = this.model.map(s => s.activity.activityCode);
        this.defaultMilestone = this.milestones.filter(c => !StringHelper.isNullOrWhiteSpace(c.activityCode) && !activityCodes.some(a => a === c.activityCode));
        
        for (const item of this.model.reverse()) {
            if (item.activity && !StringHelper.isNullOrWhiteSpace(item.activity.activityCode)) {
                let milestone = this.milestones?.find(x => x.activityCode === item.activity.activityCode);
                if (milestone && !milestone.hasLinked) {
                    item.activity.milestone = milestone.milestone;
                    milestone.hasLinked = true;
                }
            }

            this.convertStringToArray('poFulfillmentNos', item);
            this.convertStringToArray('shipmentNos', item);
            this.convertStringToArray('containerNos', item);
            this.convertStringToArray('vesselFlight', item);
        }

        this.model.reverse();
    }

    convertStringToArray(propertyName: string, obj: any) {
        if (obj[propertyName]) {
            obj[`${propertyName}Array`] = [];
            var textArray = obj[propertyName].split(';');
            for (let text of textArray) {
                const info = text.split('~');
                obj[`${propertyName}Array`].push(
                    {
                        text: info[0],
                        value: info[1]
                    }
                );
            }
        }
    }

    checkToShowActionButton() {
        if ((!this.model || this.model.length <= 0) && this.pageSetting.filterEventDate === '0001/1/1') {
            this.showActionButton = false;
        } else {
            this.showActionButton = true;
        }
    }

    checkToShowActivityList() {
        if ((this.model && this.model.length > 0)) {
            this.showActivityList = true;
        } else {
            this.showActivityList = false;
        }

        this.showNoRecordMsg = !this.showActivityList;
    }

    private groupActivity() {
        let curDate = moment();
        let i = 0;
        let items = [];
        let list = [];
        this.model.forEach(el => {
            el.matchFilter = true;

            const activityDate = moment(el.activityDate);

            if (i === 0) {
                curDate = activityDate;
            }

            if (curDate.isSame(activityDate, 'day')) {
                items.push(el);
            } else {
                list.push({ dateGroup: curDate, activities: items, matchFilter: true });

                items = [];
                items.push(el);
            }

            curDate = activityDate;
            i++;
        });

        if (items.length > 0) {
            list.push({ dateGroup: curDate, activities: items, matchFilter: true });
        }

        this.groupModel = list;
    }

    onFromDateFilterChange(value): void {
        if (value) {
            this.filterActivityModel.filterFromDate = `${value.getFullYear()}/${value.getMonth() + 1}/${value.getDate()}`;
        } else {
            this.filterActivityModel.filterFromDate = '';
        }
    }

    onToDateFilterChange(value): void {
        if (value) {
            this.filterActivityModel.filterToDate = `${value.getFullYear()}/${value.getMonth() + 1}/${value.getDate()}`;
        } else {
            this.filterActivityModel.filterToDate = '';
        }
    }

    filterActivityByValueChange(value): void {
        delete this.filterActivityModel.filterValue;
        this.filterActivityValueDataSource$ = this._activityTimelineService.getFilterActivityValueDropdown(
            value,
            this.entityId,
            this.entityType
        );
    }

    clearActivityFilter(): void {
        this.filterActivityModel = new FilterActivityModel();
        this.applyActivityFilter();
    }

    applyActivityFilter(): void {
        if (!this.groupModel || this.groupModel.length === 0) {
            return;
        }
        const hasFilterValue = !StringHelper.isNullOrWhiteSpace(this.filterActivityModel.filterValue);
        const hasDateFromFilter = this.filterActivityModel.filterFromDate && this.filterActivityModel.filterFromDate != '0001/1/1';
        const hasDateToFilter = this.filterActivityModel.filterToDate && this.filterActivityModel.filterToDate != '0001/1/1'
        for (let i = 0; i < this.groupModel.length; i++) {
            const activities = this.groupModel[i].activities;
            for (let j = 0; j < activities.length; j++) {
                let isMatch = true;

                if (hasDateFromFilter &&
                    this.groupModel[i].dateGroup.isBefore(this.filterActivityModel.filterFromDate, 'day')) {
                    isMatch = false;
                }
                if (hasDateToFilter &&
                    this.groupModel[i].dateGroup.isAfter(this.filterActivityModel.filterToDate, 'day')) {
                    isMatch = false;
                }
                if (!isMatch) {
                    activities[j].matchFilter = false;
                    continue;
                }
                if (hasFilterValue) {
                    switch (this.filterActivityModel.filterBy) {
                        case 'BookingNo':
                            isMatch = activities[j].poFulfillmentNosArray &&
                                activities[j].poFulfillmentNosArray.some(x => StringHelper.caseIgnoredCompare(x.text, this.filterActivityModel.filterValue));
                            break;
                        case 'ShipmentNo':
                            isMatch = activities[j].shipmentNosArray &&
                                activities[j].shipmentNosArray.some(x => StringHelper.caseIgnoredCompare(x.text, this.filterActivityModel.filterValue));
                            break;
                        case 'ContainerNo':
                            isMatch = activities[j].containerNosArray &&
                                activities[j].containerNosArray.some(x => StringHelper.caseIgnoredCompare(x.text, this.filterActivityModel.filterValue));
                            break;
                        case 'VesselName':
                            isMatch = activities[j].vesselFlightArray?.some(x => StringHelper.caseIgnoredCompare(x.text, this.filterActivityModel.filterValue)) ?? false;
                            break;
                        default:
                            break;
                    }
                }
                activities[j].matchFilter = isMatch;
            }
            this.groupModel[i].matchFilter = activities.some(x => x.matchFilter || !StringHelper.isNullOrWhiteSpace(x.activity.milestone));
        }
    }

    get enableApplyActivityFilterButton(): boolean {
        return this.filterActivityModel.filterValue?.length > 0 ||
            (this.filterActivityModel.filterFromDate != null && this.filterActivityModel.filterFromDate != '0001/1/1') ||
            (this.filterActivityModel.filterToDate != null && this.filterActivityModel.filterToDate != '0001/1/1');
    }
}
