import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { InvoiceListService } from './invoice-list.service';
import { DATE_FORMAT, InvoiceType, PaymentStatusType } from 'src/app/core';
import { faCheck, faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-invoice-list',
    templateUrl: './invoice-list.component.html',
    styleUrls: ['./invoice-list.component.scss']
})
export class InvoiceListComponent extends ListComponent implements OnInit {

    listName = 'invoices';
    PaymentStatusType = PaymentStatusType;
    
    faInfoCircle = faInfoCircle;
    faCheck = faCheck;
    DATE_FORMAT = DATE_FORMAT;
    defaultValue = DefaultValue2Hyphens;

    constructor(service: InvoiceListService, route: ActivatedRoute, location: Location) {
        super(service, route, location);
    }

    downloadFile(id, fileName) {
        (this.service as InvoiceListService).downloadFile(id, fileName).subscribe();
    }

    getInvoiceType(invoiceType: string) {
        switch (invoiceType.toUpperCase()) {
            case InvoiceType.Invoice:
                return 'Invoice';
            case InvoiceType.Statement:
                return 'Statement';
            case InvoiceType.Manual:
                return 'Manual';
            default:
                return '';
        }
    }
}
