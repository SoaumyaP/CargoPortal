import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { HttpService, FormService, DropdownListModel, OrganizationType, DropDownListItemModel} from 'src/app/core';
import { SurveyParticipantModel } from 'src/app/core/models/survey/survey-participant.model';
import { SurveyQuestionModel } from 'src/app/core/models/survey/survey-question.model';
import { SurveyModel } from 'src/app/core/models/survey/survey.model';
import { environment } from 'src/environments/environment';

@Injectable()
export class SurveyFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/surveys`);
    }

    surveyFormSubject = new BehaviorSubject<SurveyModel>(null);

    getOrgSelectionDataSourceByType(orgType: OrganizationType): Observable<DropdownListModel<number>[]> {
        return this.httpService.get<DropdownListModel<number>[]>(`${environment.commonApiUrl}/organizations/selectionsByOrgType?type=${orgType}`);
    }

    getUserSelectionDataSource(searchEmail: string) : Observable<DropDownListItemModel<number>[]> {
        return this.httpService.get<DropDownListItemModel<number>[]>(`${environment.apiUrl}/users/selections?searchEmail=${searchEmail}`);
    }

    saveSurvey(model: any) {
        // edit
        if (model.id > 0) {
            return this.httpService.update(`${this.apiUrl}/${model.id}`, model);
        }
        // add new
        else {
            return this.httpService.create(this.apiUrl, model);
        }
    }

    getSurveyParticipants() {
        return this.httpService.get<SurveyParticipantModel[]>(`${environment.apiUrl}/surveys/statistics/sent-to-you`);
    }

    closeSurvey(surveyId: number) {
        return this.httpService.update(`${this.apiUrl}/${surveyId}/close`);
    }
}