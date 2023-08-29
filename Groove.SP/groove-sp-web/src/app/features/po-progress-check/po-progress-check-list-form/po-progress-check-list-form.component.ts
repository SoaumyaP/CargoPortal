import { Component, Input, OnChanges, OnDestroy, OnInit, QueryList, SimpleChanges, ViewChild, ViewChildren, ViewEncapsulation } from '@angular/core';
import { DataSourceRequestState, orderBy, SortDescriptor } from '@progress/kendo-data-query';
import { faArrowCircleDown } from '@fortawesome/free-solid-svg-icons';
import { DATE_FORMAT, DropDowns, StringHelper } from 'src/app/core';
import { PurchaseOrderListService } from '../../order/purchase-order-list/purchase-order-list.service';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { POProgressCheckModel } from '../po-progress-check.model';
import moment from 'moment';
import { POProgressCheckFilterFormService } from '../po-progress-check-filter-form/po-progress-check-filter-form.service';
import { Subscription } from 'rxjs/internal/Subscription';
import { AbstractControl, NgForm } from '@angular/forms';
import { DatePickerComponent } from '@progress/kendo-angular-dateinputs';

@Component({
  selector: 'app-po-progress-check-list-form',
  templateUrl: './po-progress-check-list-form.component.html',
  styleUrls: ['./po-progress-check-list-form.component.scss'],
})
export class POProgressCheckListFormComponent implements OnChanges, OnInit {
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;
  @ViewChildren('proposeDateElement') proposeDateElments: QueryList<DatePickerComponent>;

  DATE_FORMAT = DATE_FORMAT;
  minDate: Date = moment().startOf('day').toDate();
  gridView: GridDataResult;
  gridSort: SortDescriptor[] = [
    {
      field: "",
      dir: "asc",
    },
  ];
  pageSize = 20;
  skip = 0;
  faArrowCircleDown = faArrowCircleDown;
  productionStartedDropdown = DropDowns.ProductionStartedType;
  qcRequiredTypeDropdown = DropDowns.QCRequiredType;
  shortShipTypeDropdown = DropDowns.ShortShipType;
  splitShipmentTypeDropdown = DropDowns.SplitShipmentType;
  productionStarted: boolean;
  proposeDate: Date;
  qcRequired: boolean;
  shortShip: boolean;
  splitShipment: boolean;
  isInvalidPOList: boolean;
  private _subscriptions: Array<Subscription> = [];

  @Input() poProgressCheckList: POProgressCheckModel[] = [];
  @Input() heightToReduceOfPOGrid: string;

  constructor(
    private purchaseOrderListService: PurchaseOrderListService,
    private poProgressCheckFilterFormService: POProgressCheckFilterFormService
  ) { }


  ngOnChanges(changes: SimpleChanges): void {
    if (changes.poProgressCheckList?.firstChange === false) {
      this.loadPOProgressCheckList();
      setTimeout(() => {
        this.mainForm.form.markAllAsTouched();
      }, 1);
      this.checkStatusOfPOList();
    }
  }

  ngOnInit() {
    const sub = this.poProgressCheckFilterFormService.resetFilterEvent$.subscribe(
      r => {
        this.resetValueDropdown();
      }
    )
    this._subscriptions.push(sub);
  }

  applyProgressCheck() {
    this.setValueForPOList();
    this.resetValueDropdown();
    this.checkStatusOfPOList();
  }

  isInvalidProposeDate(controlName) {
    const proposeDate = this.proposeDateElments.find(c => c['control'].name === controlName);
    return proposeDate ? proposeDate['control'].control.status === 'INVALID' : false;
  }

  checkStatusOfPOList() {
    this.isInvalidPOList = this.poProgressCheckList.some(c => c.proposeDate && moment(c.proposeDate).toDate() < this.minDate);
  }

  onChangeProposeDate() {
    this.checkStatusOfPOList();
  }

  loadPOProgressCheckList() {
    this.gridView = {
      data: orderBy(this.poProgressCheckList, this.gridSort).slice(this.skip, this.skip + this.pageSize),
      total: this.poProgressCheckList.length
    };
  }

  gridSortChange(sort: SortDescriptor[]): void {
    this.gridSort = sort;
    this.loadPOProgressCheckList();
    this.checkStatusOfPOList();
  }

  public pageChange(event: PageChangeEvent): void {
    this.skip = event.skip;
    this.loadPOProgressCheckList();
    this.checkStatusOfPOList();
  }

  setValueForPOList() {
    const selectedDropdowns = this.getSelectedDropdowns();
    if (selectedDropdowns.length > 0) {
      for (let po of this.poProgressCheckList) {
        for (let selectedDropdown of selectedDropdowns) {
          const propertyName = Object.keys(selectedDropdown)[0];
          po[propertyName] = selectedDropdown[propertyName];
        }
      }
    }
  }

  /**
   * get dropdowns have been selected to patch value for PO list
   * @returns 
   */
  getSelectedDropdowns() {
    const selectedDropdowns = [];
    if (!StringHelper.isNullOrEmpty(this.productionStarted)) {
      selectedDropdowns.push({
        productionStarted: this.productionStarted
      })
    }

    if (!StringHelper.isNullOrEmpty(this.proposeDate)) {
      selectedDropdowns.push({
        proposeDate: this.proposeDate
      })
    }

    if (!StringHelper.isNullOrEmpty(this.qcRequired)) {
      selectedDropdowns.push({
        qcRequired: this.qcRequired
      })
    }

    if (!StringHelper.isNullOrEmpty(this.shortShip)) {
      selectedDropdowns.push({
        shortShip: this.shortShip
      })
    }

    if (!StringHelper.isNullOrEmpty(this.splitShipment)) {
      selectedDropdowns.push({
        splitShipment: this.splitShipment
      })
    }

    return selectedDropdowns;
  }

  getFormControl(controlName: string): AbstractControl {
    return this.mainForm?.form?.controls[controlName];
  }

  resetValueDropdown() {
    this.productionStarted = null;
    this.proposeDate = null;
    this.qcRequired = null;
    this.shortShip = null;
    this.splitShipment = null;
  }

  ngOnDestroy(): void {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
