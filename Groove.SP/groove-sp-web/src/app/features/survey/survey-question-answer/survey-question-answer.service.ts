import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { FormService, HttpService } from "src/app/core";
import { SurveyAnswerModel } from "src/app/core/models/survey/survey-answer.model";
import { SurveyModel } from "src/app/core/models/survey/survey.model";
import { environment } from "src/environments/environment";

@Injectable({
    providedIn: 'root'
})
export class SurveyQuestionAnswerService extends FormService<any>{
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/surveys`);
    }

    previewFormSubject: BehaviorSubject<SurveyModel>;

    previewSurvey(surveyId: number) {
        return this.httpService.get(`${environment.apiUrl}/surveys/${surveyId}/question-answer?mode=preview`);
    }

    getToSubmit(surveyId: number) {
        return this.httpService.get(`${environment.apiUrl}/surveys/${surveyId}/question-answer?mode=submit`);
    }

    publishSurvey(surveyId: number, surveyModel: SurveyModel) {
        return this.httpService.update(`${environment.apiUrl}/surveys/${surveyId}/publish`, surveyModel);
    }

    submitSurvey(surveyId: number, surveyAnswerModel: SurveyAnswerModel[]) {
        return this.httpService.create(`${environment.apiUrl}/surveys/${surveyId}/submit`, surveyAnswerModel);
    }
}