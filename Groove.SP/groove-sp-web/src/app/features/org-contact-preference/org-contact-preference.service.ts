import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { OrgContactPreferenceModel } from './org-contact.model';

@Injectable()
export class OrgContactPreferenceService {
  constructor(private httpService: HttpService) {
  }

  getByOrganization(organizationId: number): Observable<OrgContactPreferenceModel[]> {
    return this.httpService.get<OrgContactPreferenceModel[]>(`${environment.apiUrl}/orgContactPreferences/organization/${organizationId}`);
  }
}