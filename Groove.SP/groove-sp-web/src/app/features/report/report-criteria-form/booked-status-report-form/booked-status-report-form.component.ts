import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { StringHelper, UserContextService } from 'src/app/core';
import { ReportCriteriaFormService } from '../report-criteria-form.service';
import { Router, ActivatedRoute } from '@angular/router';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import * as cloneDeep from 'lodash/cloneDeep';
import { QueryModel } from 'src/app/core/models/forms/query.model';
import { ReportFormBase } from '../report-form-base';
import { TranslateService } from '@ngx-translate/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';

@Component({
  selector: 'app-booked-status-report-form',
  templateUrl: './booked-status-report-form.component.html',
  styleUrls: ['./booked-status-report-form.component.scss']
})
export class BookedStatusReportFormComponent extends ReportFormBase implements OnInit, OnDestroy {
  @ViewChild('portAutoComplete', { static: false }) public portAutoComplete: AutoCompleteComponent;

  countryList: any[];
  filteredCountryList: any[];
  portList: any[];
  filteredPortList: any[];

  acpTimeout: any;
  portLoading: boolean;

  constructor(
    protected reportCriteriaFormService: ReportCriteriaFormService,
    protected _userContext: UserContextService,
    protected router: Router,
    protected activatedRoute: ActivatedRoute,
    private translateService: TranslateService,
    protected notification: NotificationPopup
  ) {
    super(reportCriteriaFormService, _userContext, router, activatedRoute, notification);
  }

  ngOnInitDataLoaded() {
    super.ngOnInitDataLoaded();
    this.onFormInit();
    this.reportCriteriaFormService.getCountries().subscribe((countries: any) => {
      this.countryList = countries;
      this.filteredCountryList = this.countryList;
      this.filteredCountryList.unshift({
        'value': 0,
        'label': 'Any Country'
      });
      this.isInitDataLoaded = true;
    });
  }

  onFormInit() {
    this.mainForm = new FormGroup({
      poNoFrom: new FormControl(),
      poNoTo: new FormControl(),
      poStage: new FormControl(0),
      etdFrom: new FormControl(new Date()),
      etdTo: new FormControl(),
      bookingStage: new FormControl(0),
      shipFromCountry: new FormControl(0),
      shipFromLocation: new FormControl(),
      incoterm: new FormControl('0'),
      includeDraftBooking: new FormControl(false)
    });
  }

  private validatePORangeInput() {
    delete this.formErrors['poNoTo'];
    let poNoFrom = this.fieldValue('poNoFrom');
    let poNoTo = this.fieldValue('poNoTo');

    if (StringHelper.isNullOrEmpty(poNoTo)) {
      return;
    }

    // let to compare number
    if (!isNaN(poNoFrom) && !isNaN(poNoTo)) {
      poNoFrom = Number.parseInt(poNoFrom);
      poNoTo = Number.parseInt(poNoTo);
    }

    if (StringHelper.isNullOrEmpty(poNoFrom) || poNoFrom > poNoTo) {
      this.formErrors['poNoTo'] = this.translateService.instant('validation.poNoRangeInvalid');
    }
  }

  private validateETDRangeInput() {
    delete this.formErrors['etdTo'];
    const etdTo = this.fieldValue('etdTo');
    const etdFrom = this.fieldValue('etdFrom');

    if (StringHelper.isNullOrEmpty(etdTo)) {
      return;
    }

    if (StringHelper.isNullOrEmpty(etdFrom) || new Date(etdFrom).setHours(0, 0, 0, 0) > new Date(etdTo).setHours(0, 0, 0, 0)) {
      this.formErrors['etdTo'] = this.translateService.instant('validation.exWorkDateRangeInvalid');
    }
  }

  onCountryValueChange(value) {
    const countryValue = this.fieldValue('shipFromCountry');
    if (countryValue > 0) {
      this.reportCriteriaFormService.getLocations(countryValue).subscribe(data => {
        this.portList = data;
      });
    } else {
      this.mainForm.controls['shipFromLocation'].setValue('');
      delete this.formErrors['shipFromLocation'];
    }
  }

  onPortValueChange(value) {
    delete this.formErrors['shipFromLocation'];
    if (StringHelper.isNullOrEmpty(value)) {
      return;
    }
    const selectedItem = this.portList.find(
      (element) => {
        return element.label === value;
      }
    );
    if (!selectedItem) {
      this.formErrors['shipFromLocation'] = this.translateService.instant('validation.shipFromPortInvalid');
    }
  }

  private get isDisablePort() {
    return this.fieldValue('shipFromLocation') ? true : false;
  }

  onCountryFilterChanged(value) {
    this.filteredCountryList = this.countryList.filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
  }

  onPortFilterChange(value) {
    if (value.length > 0) {
      this.portAutoComplete.toggle(false);
      this.filteredPortList = [];
      clearTimeout(this.acpTimeout);
      this.acpTimeout = setTimeout(() => {
        this.portLoading = true;
        value = value.toLowerCase();
        let take = 10;
        for (let i = 0; i < this.portList.length && take > 0; i++) {
          if (this.portList[i].label.toLowerCase().indexOf(value) !== -1) {
            this.filteredPortList.push(this.portList[i]);
            take--;
          }
        }
        this.portAutoComplete.toggle(true);
        this.portLoading = false;
      }, 400);
    } else {
      this.portLoading = false;
      if (this.acpTimeout) {
        clearTimeout(this.acpTimeout);
      }
      delete this.formErrors['shipFromLocation'];
      this.portAutoComplete.toggle(false);
    }
  }

  onIncludeDraftBookingChanged($event) {
    this.mainForm.controls['includeDraftBooking'].setValue(!this.fieldValue('includeDraftBooking'));
  }

  onExportClick() {
    super.onExportClick();

    const submitData = cloneDeep(this.mainForm.value);
    let shipFromPortIds = "";
    const shipFromPort = this.portList ? this.portList.find(x => x.label === submitData.shipFromLocation) : null;
    if (StringHelper.isNullOrEmpty(shipFromPort)) {
      shipFromPortIds = submitData.shipFromCountry > 0 ? this.portList.map(x => x.value).filter(x => x > 0).join() : "";
    } else {
      shipFromPortIds = shipFromPort.value;
    }
    // Preprocessing data before it's sent to the server
    const filterDsModel = new BookedStatusReportFilterData(
      this.selectedCustomerId,
      submitData.poNoFrom,
      submitData.poNoTo,
      submitData.etdFrom,
      submitData.etdTo,
      submitData.poStage,
      submitData.bookingStage,
      shipFromPortIds,
      submitData.incoterm,
      submitData.includeDraftBooking,
    );

    this.reportCriteriaFormService.exportXlsx(
      this.selectedReportId,
      'Booked Status Report.xlsx',
      filterDsModel.buildToQueryParams,
    ).subscribe(response => {
      if (!response) {
        this.notification.showWarningPopup(
          'msg.noPOFound',
          'label.result'
        );
      }
      this.resetAfterDownload();
    }, error => {
      this.notification.showErrorPopup(
        "msg.exportFailed",
        "label.result"
      );
      this.resetAfterDownload();
    });
  }
}

export class BookedStatusReportFilterData extends QueryModel {

  private SelectedCustomerId: number;

  private PONoFrom: string;

  private PONoTo: string;

  private ETDFrom: string;

  private ETDTo: string;

  private POStage: number;

  private BookingStage: number;

  private ShipFromPortIds: string;

  private Incoterm: string;

  private IncludeDraftBooking: boolean;

  /**
   * CONSTRUCTOR
   */
  constructor(
    selectedCustomerId: number,
    poNoFrom: string,
    poNoTo: string,
    etdFrom: Date,
    etdTo: Date,
    poStage: number,
    bookingStage: number,
    shipFromPortIds: string,
    incoterm: string,
    includeDraftBooking: boolean) {
    super();
    this.SelectedCustomerId = selectedCustomerId;
    this.PONoFrom = StringHelper.isNullOrEmpty(poNoFrom) ? null : poNoFrom;
    this.PONoTo = StringHelper.isNullOrEmpty(poNoTo) ? null : poNoTo;
    this.ETDFrom = StringHelper.isNullOrEmpty(etdFrom) ? null : this.convertToQueryDateString(etdFrom);
    this.ETDTo = StringHelper.isNullOrEmpty(etdTo) ? null : this.convertToQueryDateString(etdTo);
    this.POStage = poStage <= 0 ? null : poStage;
    this.BookingStage = bookingStage <= 0 ? null : bookingStage;
    this.Incoterm =  incoterm === '0' ? null : incoterm;
    this.ShipFromPortIds = StringHelper.isNullOrEmpty(shipFromPortIds) ? null : shipFromPortIds;
    this.IncludeDraftBooking = includeDraftBooking;
  }
}
