import { Injectable } from '@angular/core';
import { DropDownListItemModel, EntityType, HttpService } from 'src/app/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable()
export class ActivityTimelineService {

    constructor(private _httpService: HttpService) {
    }

    getActivities(request: GetActivityTimelineRequest): Observable<any> {
        return this._httpService.get(`${environment.apiUrl}/globalidactivities/activities-timeline`, request);
    }

    getFilterActivityValueDropdown(filterBy: string, entityId: number, entityType: string): Observable<DropDownListItemModel<string>[]> {
        return this._httpService.get<DropDownListItemModel<string>[]>(`${environment.apiUrl}/globalidactivities/filter-value-dropdown?filterBy=${filterBy}&entityType=${entityType}&entityId=${entityId}`);
    }
}

export interface GetActivityTimelineRequest {
    entityId: number;
    entityType: EntityType;
    fromDate: string;
    toDate: string;
    filterBy: string;
    filterValue: string;
}

export interface ActivityMilestone {
    activityCode: string,
    milestone: string,
    occurDate: Date | null,
    hasLinked: boolean
}