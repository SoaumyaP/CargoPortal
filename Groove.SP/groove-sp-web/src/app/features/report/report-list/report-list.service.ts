import { Injectable } from '@angular/core';
import { ListService, HttpService, Roles, DropDownListItemModel, StringHelper } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { forkJoin, Observable, of } from 'rxjs';
import { ReportPermissionModel } from '../models/report-permission.model';
import { ReportOptionModel } from '../../scheduling/models/report.option.model';

@Injectable({
  providedIn: 'root'
})
export class ReportListService extends ListService {

    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/reports`);
        this.defaultState.sort = [
            { field: 'reportName', dir: 'asc' }
        ];
    }

    getPrincipalDataSource(roleId: number, organizationId: number, affiliates: string): Observable<Array<DropDownListItemModel<number>>> {
        const url1 = `${environment.commonApiUrl}/organizations/principals?roleId=${roleId}&affiliates=${affiliates || '[]'}&organizationId=${organizationId || 0}`;
        const obs1 = this.httpService.get<Array<DropDownListItemModel<number>>>(url1);

        const url2 = `${environment.apiUrl}/buyerCompliances/principals/agent?organizationId=${organizationId || 0}`;
        const obs2 = this.httpService.get<Array<DropDownListItemModel<number>>>(url2);

        switch (roleId) {
            case Roles.Agent:
                // Current user's role is Agent, call to Shipment Portal + CFSE to get data
                return forkJoin(obs1, obs2).map((data) => {
                    let result: Array<DropDownListItemModel<number>> = [];
                    const principals = data[0];
                    const filteredPrincipal = data[1];
                    if (!StringHelper.isNullOrEmpty(filteredPrincipal)) {
                        result = principals.filter(x => filteredPrincipal.some(y => y.value === x.value));
                    }
                    return result;
                });

            default:
                // Else, call to CFSE to get data
                return obs1;
        }
    }

    getAccessibleOrganizationIds(roleId: number, organizationId: number, affiliates: string): Observable<any> {
        const url = `${environment.commonApiUrl}/organizations/reports/accessibleOrganizationIds?roleId=${roleId}&affiliates=${affiliates || "[]"}&organizationId=${organizationId || 0}`;
        return this.httpService.get<any>(url);
    }

    getOrganizationDataSource() {
        const url = `${environment.commonApiUrl}/organizations/orgReferenceData/active`;
        return this.httpService.get<any>(url);
    }

    grantReportPermission(formData: ReportPermissionModel) {
        const reportId = formData.reportId;
        const url = `${environment.apiUrl}/reports/${reportId}/permissions`;
        return this.httpService.update(url, formData);
    }

    getReportPermission(reportId): Observable<ReportPermissionModel> {
        if (reportId > 0) {
            const url = `${environment.apiUrl}/reports/${reportId}/permissions`;
            return this.httpService.get<ReportPermissionModel>(url);
        }
    }

    /**
     * To get list of report options
     * @param isInternal
     * @param roleId
     * @param affiliates
     * @returns
     */
    $getReportOptions(isInternal: boolean, roleId: number, affiliates: number[]): Observable<Array<ReportOptionModel>> {
        const url = `${environment.apiUrl}/reports/SelectOptions?isInternal=${isInternal}&roleId=${roleId}&affiliates=${affiliates}`;
        return this.httpService.get(url);
    }
}
