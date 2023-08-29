import { Injectable } from '@angular/core';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class RolesListService extends ListService {
  constructor(httpService: HttpService) {
      super(httpService, `${environment.apiUrl}/roles`);
      this.defaultState.sort = [
          { field: 'updatedDate', dir: 'desc' },
      ];
       this.defaultState.filter = {
         logic: 'and',
         filters: [{ field: 'status', operator: 'eq', value: 1 }]
       };
  }
}