import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { SurveyFormComponent } from './survey-form/survey-form.component';
import { SurveyListComponent } from './survey-list/survey-list.component';
import { SurveyQuestionAnswerComponent } from './survey-question-answer/survey-question-answer.component';
import { SurveyReportComponent } from './survey-report/survey-report.component';
import { SurveyTemplateComponent } from './survey-template/survey-template.component';


const routes: Routes = [
  {
    path: '',
    component: SurveyListComponent,
    data:
    {
      permission: AppPermissions.Organization_SurveyList,
      pageName: 'listOfSurvey'
    }
  },
  {
    path: ':surveyId/question-answer',
        component: SurveyQuestionAnswerComponent,
        data:
        {
            pageName: 'surveyDetail'
        }
  },
  {
    path: 'template',
        component: SurveyTemplateComponent,
        data:
        {
            pageName: 'surveyDetail'
        }
  },
  {
    path: ':surveyId/report',
    component: SurveyReportComponent,
    data:
    {
      permission: AppPermissions.Organization_SurveyDetail,
      pageName: 'surveyReport'
    }
  },
  {
    path: ':mode/:id',
    component: SurveyFormComponent,
    data:
    {
      permission: {
        'view': AppPermissions.Organization_SurveyDetail,
        'add': AppPermissions.Organization_SurveyDetail_Add,
        'edit': AppPermissions.Organization_SurveyDetail_Edit
      },
      pageName: 'surveyDetail'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SurveyRoutingModule { }
