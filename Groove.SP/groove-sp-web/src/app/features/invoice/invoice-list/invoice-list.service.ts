import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class InvoiceListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/invoices`);
        this.defaultState.sort = [
            { field: 'invoiceDate', dir: 'desc' }
        ];
    }

    public downloadFile(id, fileName) {
        return this.httpService.downloadFile(`${environment.apiUrl}/invoices/${id}/download/${encodeURIComponent(fileName)}`, fileName);
    }
}
