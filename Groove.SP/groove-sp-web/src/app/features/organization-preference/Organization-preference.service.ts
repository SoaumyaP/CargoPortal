import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class OrganizationPreferenceService {
  constructor(private httpService: HttpService) {
  }

  getOrganizationPreference(organizationId: number, productCode: string) {
    return this.httpService.get<any>(`${environment.apiUrl}/organizationPreferences?organizationId=${organizationId}&productCode=${productCode}`);
  }

  getListByOrganization(organizationId: number) {
    return this.httpService.get<any>(`${environment.apiUrl}/organizationPreferences/organization/${organizationId}`);
  }
}