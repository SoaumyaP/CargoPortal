import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService} from 'src/app/core';
import { BarChartSeriesItem, PieChartSeriesItem } from 'src/app/core/models/chart/kendo-chart.model';
import { SurveyQuestionModel } from 'src/app/core/models/survey/survey-question.model';
import { environment } from 'src/environments/environment';

@Injectable()
export class SurveyReportService {

    readonly surveyApiUrl = `${environment.apiUrl}/surveys`;
    readonly questionApiUrl = `${environment.apiUrl}/surveyQuestions`;

    constructor(private httpService: HttpService) {
    }

    countParticipants(surveyId: number, isSubmitted: boolean | null) {
        return this.httpService.get(`${this.surveyApiUrl}/${surveyId}/participantCount${isSubmitted !== null ? ('?isSubmitted=' + isSubmitted) : ''}`);
    }

    getQuestions(surveyId: number): Observable<SurveyQuestionModel[]> {
        return this.httpService.get(`${this.surveyApiUrl}/${surveyId}/questions`);
    }

    getPieChartQuestionReport(questionId: number): Observable<PieChartSeriesItem[]> {
        return this.httpService.get(`${this.questionApiUrl}/${questionId}/pie-chart`);
    }

    getBarChartQuestionReport(questionId: number) {
        return this.httpService.get(`${this.questionApiUrl}/${questionId}/bar-chart`);
    }

    getSummaryQuestionReport(questionId: number, requestString: string) {
        return this.httpService.get(`${this.questionApiUrl}/${questionId}/answer-summary?${requestString}`);
    }
}