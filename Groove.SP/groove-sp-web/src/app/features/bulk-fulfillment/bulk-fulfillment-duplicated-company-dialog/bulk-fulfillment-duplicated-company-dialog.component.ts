import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { RowArgs } from '@progress/kendo-angular-grid';
import { SortDescriptor } from '@progress/kendo-data-query';
import { DialogActionType, StringHelper } from 'src/app/core';

@Component({
  selector: 'app-bulk-fulfillment-duplicated-company-dialog',
  templateUrl: './bulk-fulfillment-duplicated-company-dialog.component.html',
  styleUrls: ['./bulk-fulfillment-duplicated-company-dialog.component.scss']
})
export class BulkFulfillmentDuplicatedCompanyDialogComponent implements OnInit {
  @Input() isOpenDialog: boolean;
  @Input() duplicatedCompanies: any[] = [];
  @Output() dialogEvent: EventEmitter<{ data: any, dialogActionType: DialogActionType }> = new EventEmitter<{ data: any, dialogActionType: DialogActionType }>();

  duplicatedCompanySelected: any;
  faInfoCircle = faInfoCircle;
  selectedCompanies = [];
  selectableSettings = {
    enabled: true,
    checkboxOnly: true,
    mode: 'single'
  };
  sortSettings: SortDescriptor[] = [
    {
      field: 'name',
      dir: 'desc'
    }
  ]
  constructor() { }

  ngOnInit() {
    this.selectedCompanies = [];
  }

  onClickOverride() {
    if (!this.duplicatedCompanySelected) {
      return;
    }

    this.dialogEvent.emit({ data: this.duplicatedCompanySelected, dialogActionType: DialogActionType.Submit });
    this.duplicatedCompanySelected = null;
  }

  onClickNoThanks() {
    this.dialogEvent.emit({ data: null, dialogActionType: DialogActionType.Cancel });
    this.duplicatedCompanySelected = null;
  }

  onCloseDialog() {
    this.dialogEvent.emit({ data: null, dialogActionType: DialogActionType.Close });
    this.duplicatedCompanySelected = null;
  }

  onSelectionChanged(event: any) {
    this.duplicatedCompanySelected = event?.selectedRows[0]?.dataItem;
  }

  public selectedCompany(context: RowArgs): string {
    return context.dataItem;
  }

  public getConcatenatedAddress(data: any) {
    let concatenatedAddress = data.address;
    if (!StringHelper.isNullOrEmpty(data.addressLine2)) {
      concatenatedAddress += '\n' + data.addressLine2;
    }
    if (!StringHelper.isNullOrEmpty(data.addressLine3)) {
      concatenatedAddress += '\n' + data.addressLine3;
    }
    if (!StringHelper.isNullOrEmpty(data.addressLine4)) {
      concatenatedAddress += '\n' + data.addressLine4;
    }
    return concatenatedAddress;
  }
}
