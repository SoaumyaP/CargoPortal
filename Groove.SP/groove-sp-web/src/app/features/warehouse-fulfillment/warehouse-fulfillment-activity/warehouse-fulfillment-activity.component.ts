import {
  Component,
  Input,
  OnChanges,
  SimpleChanges
} from '@angular/core';
import {
  faMapMarkerAlt,
  faBell,
  faCheckCircle
} from '@fortawesome/free-solid-svg-icons';
import moment from 'moment';
import { Observable } from 'rxjs';
import { DATE_FORMAT, DropDownListItemModel, StringHelper, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ArrayHelper } from 'src/app/core/helpers/array.helper';
import { FilterActivityModel } from 'src/app/core/models/activity/filter-activity.model';
import { MilestoneEventCode } from 'src/app/core/models/constants/app-constants';
import { WarehouseFulfillmentFormService } from '../warehouse-fulfillment-form/warehouse-fulfillment-form.service';

@Component({
  selector: 'app-warehouse-fulfillment-activity',
  templateUrl: './warehouse-fulfillment-activity.component.html',
  styleUrls: ['./warehouse-fulfillment-activity.component.scss']
})
export class WarehouseFulfillmentActivityComponent implements OnChanges {
  @Input() public poffId;
  @Input() public bookingModel;
  DATE_FORMAT = DATE_FORMAT;
  faCheckCircle = faCheckCircle;
  faMapMarkerAlt = faMapMarkerAlt;
  faBell = faBell;
  model: any;
  defaultPageSize = 5;
  isSorting: boolean;
  isFiltering: boolean;
  pageSetting = {
    pageSize: this.defaultPageSize,
    pageIndex: 0,
    ascending: false,
    filterEventDate: '0001/1/1'
  };
  activityTotal = 0;
  showLoadMoreButton: boolean = false;
  showActionButton: boolean = false;
  showActivityList: boolean = false;
  showNoRecordMsg: boolean = false;
  groupModel = null;
  filterActivityModel: FilterActivityModel = new FilterActivityModel();
  filterActivityValueDataSource$: Observable<DropDownListItemModel<string>[]>;

  readonly milestoneEventCode = MilestoneEventCode;
  milestoneStatus: any = {
    isExistingBooked: false,
    isExistingBookingConfirmed: false,
    isExistingShipmentDispatch: false,
    isExistingCargoReceived: false,
    isExistingClosed: false
  };
  isCanClickBookingNo: boolean;
  defaultMilestone: any = [];

  constructor(
    private _warehouseFulfillmentFormService: WarehouseFulfillmentFormService,
    private _userContext: UserContextService) {
    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
          this.isCanClickBookingNo = user.permissions?.some(c => c.name === AppPermissions.PO_Fulfillment_Detail);
      }
  });
   }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.poffId.currentValue && changes.poffId.currentValue > 0) {
      this.loadActivity();
    }
  }

  public async loadActivity() {
    // reset to default.
    this.reset();

    await this.getActivities();
    this.checkToShowActivityList();
    this.checkToShowLoadMoreButton();
    this.checkToShowActionButton();
  }

  async onLoadMoreClick() {
    this.pageSetting.pageIndex++;
    await this.getActivities();
    this.checkToShowLoadMoreButton();
  }

  mappingMilestone() {
    this.milestoneStatus.isExistingBooked = false;
    this.milestoneStatus.isExistingBookingConfirmed = false;
    this.milestoneStatus.isExistingShipmentDispatch = false;
    this.milestoneStatus.isExistingCargoReceived = false;
    this.milestoneStatus.isExistingClosed = false;

    this.defaultMilestone = this._warehouseFulfillmentFormService.getDefaultMilestone();
    const activityCodes = this.model.map(s => s.activity.activityCode);
    this.defaultMilestone = this.defaultMilestone.filter(c => !activityCodes.some(a => a === c.activityCode));
    
    for (const item of this.model.reverse()) {

      switch (item.activity?.activityCode) {

        case '1051':
          if (!this.milestoneStatus.isExistingBooked) {
            item.activity.milestone = 'label.forwarderBookingRequest';
            this.milestoneStatus.isExistingBooked = true;
          }
          break;

        case '1061':
          if (!this.milestoneStatus.isExistingBookingConfirmed) {
            item.activity.milestone = 'label.forwarderBookingConfirmed';
            this.milestoneStatus.isExistingBookingConfirmed = true;
          }
          break;

        case '1063':
          if (!this.milestoneStatus.isExistingCargoReceived) {
            item.activity.milestone = 'label.cargoReceived';
            this.milestoneStatus.isExistingCargoReceived = true;
          }

          break;
        default:
          break;
      }

      this.convertStringToArray('poFulfillmentNos', item);
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

  async getActivities() {
    const rs = await this._warehouseFulfillmentFormService.getActivities(this.poffId, {}).toPromise();
    this.activityTotal = rs.result.totalRecord;
    this.model = rs.result.globalIdActivities;
    this.mappingMilestone();
    this.groupActivity();
    this.groupModel.unshift({
      activities: [{
        activity: { activityCode: 'Draft', milestone: 'label.draft', matchFilter: true }
      }],
      dateGroup: moment(new Date(this.bookingModel.createdDate.toDateString())) ,
      matchFilter: true
    });

    ArrayHelper.sortBy(this.groupModel, 'dateGroup', 'desc');
  }

  async onSortClick() {
    this.isSorting = true;
    this.pageSetting.ascending = !this.pageSetting.ascending;
    if (this.pageSetting.pageIndex > 0) {
      this.pageSetting.pageSize = this.pageSetting.pageSize * (this.pageSetting.pageIndex + 1);
      this.pageSetting.pageIndex = 0;
    }

    await this.getActivities();
    this.checkToShowLoadMoreButton();

    this.isSorting = false;
  }

  checkToShowLoadMoreButton() {
    const itemCount = this.model.length;

    if (this.activityTotal && itemCount < this.activityTotal && this.activityTotal > 0 && itemCount > 0) {
      this.showLoadMoreButton = true;
    } else {
      this.showLoadMoreButton = false;
    }
  }

  async onDateFilterChange(value) {
    this.isFiltering = true;
    if (value) {
      this.pageSetting.filterEventDate = `${value.getFullYear()}/${value.getMonth() + 1}/${value.getDate()}`;
    } else {
      this.pageSetting.filterEventDate = '0001/1/1';
    }
    this.pageSetting.pageIndex = 0;

    await this.getActivities();

    this.checkToShowActivityList();
    this.checkToShowLoadMoreButton();
    this.checkToShowActionButton();

    this.isFiltering = false;
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

  private reset(): void {
    this.model = [];
    this.groupModel = null;
    this.isSorting = null;
    this.isFiltering = null;
    this.pageSetting = {
      pageSize: this.defaultPageSize,
      pageIndex: 0,
      ascending: false,
      filterEventDate: '0001/1/1'
    };
    this.activityTotal = 0;
    this.showLoadMoreButton = true;
    this.showActionButton = true;
    this.showActivityList = false;
    this.showNoRecordMsg = false;
  }

  filterActivityByValueChange(value): void {
    delete this.filterActivityModel.filterValue;
  }

  clearActivityFilter(): void {
    this.filterActivityModel = new FilterActivityModel();
    this.applyActivityFilter();
  }

  onFromDateFilterChange(value): void {
    if (value) {
      this.filterActivityModel.filterFromDate = `${value.getFullYear()}/${value.getMonth() + 1}/${value.getDate()}`;
    } else {
      this.filterActivityModel.filterFromDate = '0001/1/1';
    }
  }

  onToDateFilterChange(value): void {
    if (value) {
      this.filterActivityModel.filterToDate = `${value.getFullYear()}/${value.getMonth() + 1}/${value.getDate()}`;
    } else {
      this.filterActivityModel.filterToDate = '0001/1/1';
    }
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
