import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs-compat/Observable';
import { DropDownListItemModel } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { EventCodeModel } from '../models/master-event.model';

@Injectable({
  providedIn: 'root'
})
export class MasterEventDialogService {

  constructor(
    private http: HttpClient
  ) { }

  getEventTypeDropdown(): Observable<DropDownListItemModel<string>[]> {
    return this.http.get<DropDownListItemModel<string>[]>(`${environment.commonApiUrl}/eventTypes/dropdown`)
  }

  getEventCodesDropdown(): Observable<DropDownListItemModel<string>[]> {
    return this.http.get<DropDownListItemModel<string>[]>(`${environment.commonApiUrl}/eventCodes/dropdown`)
  }

  checkEventCodeAlreadyExist(code: string): Observable<boolean> {
    return this.http.get<boolean>(`${environment.commonApiUrl}/eventCodes/check-already-exist?code=${code}`)
  }

  addNewEventCdoe(model: EventCodeModel) {
    return this.http.post(`${environment.commonApiUrl}/eventCodes`, model)
  }

  updateEventCode(model: EventCodeModel) {
    return this.http.put(`${environment.commonApiUrl}/eventCodes`, model)
  }
}
