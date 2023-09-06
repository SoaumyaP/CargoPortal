import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core/services/http.service';
import { DropDownListItemModel, UserContextService } from 'src/app/core';
import { EventEmitter } from '@angular/core';

@Injectable()
export class POProgressCheckFilterFormService {
    public currentUser: any;
    public resetFilterEvent$: EventEmitter<any> = new EventEmitter();
    constructor(protected httpService: HttpService, private _userContext: UserContextService) {
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
            }
          });
    }

    getCustomerDataSource(roleId:number,affiliates:string): Observable<Array<DropDownListItemModel<number>>> {
        return this.httpService.get<Array<DropDownListItemModel<number>>>(`${environment.apiUrl}/buyerCompliances/dropdown/hasEnableProgressCheck/${roleId}?affiliates=${affiliates}`);
    }
}