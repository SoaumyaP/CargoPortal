import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SurveyQuestionType } from 'src/app/core';
import { SurveyQuestionModel } from 'src/app/core/models/survey/survey-question.model';
import { SurveyReportService } from './survey-report.service';

@Component({
  selector: 'app-survey-report',
  templateUrl: './survey-report.component.html',
  styleUrls: ['./survey-report.component.scss']
})
export class SurveyReportComponent implements OnInit {

  surveyId: number;
  surveyIdParam = 'survey-id';

  readonly surveyQuestionType = SurveyQuestionType;

  generalStatisticModel = {
    participantCount: 0,
    completedCount: 0,
    completionRate: 0,
  };

  questions: SurveyQuestionModel[] = [];

  constructor(private router: Router, private route: ActivatedRoute, private service: SurveyReportService) { }

  ngOnInit() {
    this.route.params.subscribe((params: Params) => {
      this.surveyId = params['surveyId'];
      this.loadInitData();
    });
  }

  loadInitData() {
    let participantCount$ = this.service.countParticipants(this.surveyId, null);
    let completedCount$ = this.service.countParticipants(this.surveyId, true);

    forkJoin([participantCount$, completedCount$]).subscribe(
      (res: any[]) => {
        this.generalStatisticModel.participantCount = res[0];
        this.generalStatisticModel.completedCount = res[1];

        if (this.generalStatisticModel.participantCount > 0) {
          this.generalStatisticModel.completionRate = this.generalStatisticModel.completedCount / this.generalStatisticModel.participantCount;
        }
      }
    );

    this.service.getQuestions(this.surveyId).subscribe(
      res => this.questions = res
    )
  }

  backTo() {
    this.router.navigate([`surveys/view/${this.surveyId}`]);
  }

}