import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
@Injectable()
export class ReportCriteriaFormService {
    constructor(private httpService: HttpService) {

    }

    getCountries(): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/countries/dropdown`);
    }

    getLocations(countryId: number): Observable<any[]> {
        return this.httpService.get(
            `${environment.commonApiUrl}/countries/${countryId}/locations/dropdown`);
    }

    checkExportPermission(reportId, customerId) {
        return this.httpService.get(
            `${environment.apiUrl}/reports/${reportId}/authorized?selectedOrganizationId=${customerId}`);
    }

    exportXlsx(reportId, fileName, queryParams) {
        return this.httpService.downloadFile(
            `${environment.apiUrl}/reports/${reportId}/export?${queryParams}`, fileName);
    }

}
