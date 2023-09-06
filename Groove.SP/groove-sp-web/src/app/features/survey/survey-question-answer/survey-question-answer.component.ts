import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { faStar as regularFaStar } from '@fortawesome/free-regular-svg-icons';
import { faStar as solidFaStar } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject, combineLatest, Subscription } from 'rxjs';
import { StringHelper, SurveyQuestionType, SurveyStatus, UserContextService } from 'src/app/core';
import { SurveyAnswerModel } from 'src/app/core/models/survey/survey-answer.model';
import { SurveyQuestionOptionModel } from 'src/app/core/models/survey/survey-question-option.model';
import { SurveyQuestionModel } from 'src/app/core/models/survey/survey-question.model';
import { SurveyModel } from 'src/app/core/models/survey/survey.model';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { SurveyFormService } from '../survey-form/survey-form.service';
import { SurveyQuestionAnswerService } from './survey-question-answer.service';

@Component({
  selector: 'app-survey-question-answer',
  templateUrl: './survey-question-answer.component.html',
  styleUrls: ['./survey-question-answer.component.scss']
})
export class SurveyQuestionAnswerComponent implements OnInit, OnDestroy {
  subscriptions: Subscription = new Subscription();
  questionsModel: SurveyQuestionModel[];
  surveyModel: SurveyModel;
  isInitDataLoaded: boolean;
  SurveyQuestionType = SurveyQuestionType;
  surveyAnswerModel: SurveyAnswerModel[] = [];
  regularFaStar = regularFaStar;
  solidFaStar = solidFaStar;
  currentUser: UserProfileModel;
  answerForm: FormGroup = new FormGroup({});
  mode: string;
  surveyStatus = SurveyStatus;
  isPreviewMode: boolean;
  isSubmitMode: boolean;

  constructor(
    private router: Router,
    public notification: NotificationPopup,
    private activatedRoute: ActivatedRoute,
    private surveyQuestionAnswerService: SurveyQuestionAnswerService,
    public surveyFormService: SurveyFormService,
    private _userContext: UserContextService
  ) {
    this.currentUser = this._userContext.currentUser;
  }

  ngOnInit() {
    this.surveyQuestionAnswerService.previewFormSubject = new BehaviorSubject<SurveyModel>(null);
    const sub = combineLatest([this.activatedRoute.params, this.activatedRoute.queryParams, this.surveyFormService.surveyFormSubject]).subscribe(
      params => {
        if (params[1].mode === 'preview') {
          this.isPreviewMode = true;
          if (params[2]) {
            this.isInitDataLoaded = true;
            this.questionsModel = params[2].questions;
            this.surveyModel = Object.assign({}, params[2]);
            this.setMoreInfo();
          } else {
            this.surveyModel = {} as SurveyModel;
            this.surveyQuestionAnswerService.previewSurvey(params[0].surveyId).subscribe(
              (data: any) => {
                this.isInitDataLoaded = true;
                this.questionsModel = data;
                this.setMoreInfo();
              },
              error => {
                if (error.status === 404) {
                  this.router.navigate(['/error/404']);
                } else {
                  this.notification.showErrorPopup(error.message, 'label.error');
                }
              }
            );
          }
        }

        if (params[1].mode === 'submit') {
          this.isSubmitMode = true;
          this.surveyQuestionAnswerService.getToSubmit(params[0].surveyId).subscribe(
            (data: any[]) => {
              if (data[0].isSubmitted === true) {
                this.router.navigateByUrl(`/surveys/template?mode=submitted`);
              }
              this.isInitDataLoaded = true;
              this.questionsModel = data;
              this.setMoreInfo();
            },
            error => {
              if (error.status === 404) {
                this.router.navigate(['/error/404']);
              } else if (error.status === 400) {
                const isNoLongerSurvey = error.error.errors?.split('#').includes('SurveyIsNoLongerActive') ?? false;
                if (isNoLongerSurvey) {
                  this.router.navigateByUrl(`/surveys/template?mode=closed`);
                }
                else {
                  this.notification.showErrorPopup(error.message, 'label.error');
                }
              }
              else {
                this.notification.showErrorPopup(error.message, 'label.error');
              }
            }
          );
        }
      }
    )

    this.subscriptions.add(sub);
  }

  setMoreInfo() {
    this.setRatingStar();
    this.buildSurveyAnswerModel();
    this.initializeForm();
  }

  initializeForm() {
    for (let questionIndex = 0; questionIndex < this.questionsModel.length; questionIndex++) {
      if (this.questionsModel[questionIndex].type === SurveyQuestionType.SingleAnswer) {
        this.answerForm.addControl(`q${questionIndex}-radio`, new FormControl());
        if (this.questionsModel[questionIndex].isIncludeOpenEndedText) {
          this.answerForm.addControl(`q${questionIndex}-radio-input`, new FormControl());
        }
      }

      if (this.questionsModel[questionIndex].type === SurveyQuestionType.MultiAnswers) {
        for (let optionIndex = 0; optionIndex < this.questionsModel[questionIndex].options.length; optionIndex++) {
          this.answerForm.addControl(`q${questionIndex}-checkbox${optionIndex}`, new FormControl());
          if (this.questionsModel[questionIndex].isIncludeOpenEndedText) {
            this.answerForm.addControl(`q${questionIndex}-checkbox-input`, new FormControl());
          }
        }
      }

      if (this.questionsModel[questionIndex].type === SurveyQuestionType.OpenEnded) {
        this.answerForm.addControl(`q${questionIndex}-openEnded-input`, new FormControl());
      }

      if (this.questionsModel[questionIndex].type === SurveyQuestionType.OpenEndedWithMultiLines) {
        this.answerForm.addControl(`q${questionIndex}-openEnded-textarea`, new FormControl());
      }

      if (this.questionsModel[questionIndex].type === SurveyQuestionType.RatingScale) {
        this.answerForm.addControl(`q${questionIndex}-rating`, new FormControl());
      }
    }
  }

  setRatingStar() {
    let ratingScaleQuestion = this.questionsModel.filter(c => c.type === SurveyQuestionType.RatingScale);
    if (ratingScaleQuestion?.length > 0) {
      for (let item of ratingScaleQuestion) {
        item.starRatingArray = [];
        for (let index = 1; index <= item.starRating; index++) {
          item.starRatingArray.push({
            type: this.regularFaStar
          });
        }
      }
    }
  }

  buildSurveyAnswerModel() {
    for (let question of this.questionsModel) {
      switch (question.type) {
        case SurveyQuestionType.SingleAnswer:
          this.surveyAnswerModel.push(this.createSurveyAnswerModel(question));

          if (question.isIncludeOpenEndedText) {
            this.surveyAnswerModel.push(this.createSurveyAnswerModel(question));
          }
          break;

        case SurveyQuestionType.MultiAnswers:
          question.options.forEach(c => {
            this.surveyAnswerModel.push(this.createSurveyAnswerModel(question, c));
          })
          if (question.isIncludeOpenEndedText) {
            this.surveyAnswerModel.push(this.createSurveyAnswerModel(question));
          }
          break;

        default:
          this.surveyAnswerModel.push(this.createSurveyAnswerModel(question, question.options[0]));
          break;
      }
    }
  }

  mapSurveyAnswerModel() {
    for (let questionIndex = 0; questionIndex < this.questionsModel.length; questionIndex++) {
      if (this.questionsModel[questionIndex].type === SurveyQuestionType.SingleAnswer) {
        let value = this.answerForm.controls[`q${questionIndex}-radio`].value;
        if (this.questionsModel[questionIndex].isIncludeOpenEndedText && value === `q${questionIndex}-radio-0`) {
          const inputedValue = this.answerForm.controls[`q${questionIndex}-radio-input`].value;
          let surveyAnswerModel = this.surveyAnswerModel.find(c => c.questionId === this.questionsModel[questionIndex].id && !c.optionId);
          surveyAnswerModel.answerText = inputedValue;
        } else {
          let surveyAnswerModel = this.surveyAnswerModel.find(c => c.questionId === this.questionsModel[questionIndex].id);
          surveyAnswerModel.answerText = value?.substring(value.indexOf('--') + 2);
        }
      }

      if (this.questionsModel[questionIndex].type === SurveyQuestionType.MultiAnswers) {
        for (let optionIndex = 0; optionIndex < this.questionsModel[questionIndex].options.length; optionIndex++) {
          let surveyAnswerModel = this.surveyAnswerModel.find(c => c.optionId === this.questionsModel[questionIndex].options[optionIndex].id);
          let optionControl = this.answerForm.controls[`q${questionIndex}-checkbox${optionIndex}`];
          if (optionControl?.value === true) {
            surveyAnswerModel.answerText = this.questionsModel[questionIndex].options[optionIndex].content;
          } else {
            surveyAnswerModel.answerText = null;
          }
        }

        if (this.questionsModel[questionIndex].isIncludeOpenEndedText) {
          let inputedSurveyAnswerModel = this.surveyAnswerModel.find(c => c.questionId === this.questionsModel[questionIndex].id && !c.optionId);
          let inputControl = this.answerForm.controls[`q${questionIndex}-checkbox-input`];
          if (inputControl?.value?.trim().length > 0) {
            inputedSurveyAnswerModel.answerText = inputControl.value.trim();
          } else {
            inputedSurveyAnswerModel.answerText = null;
          }
        }
      }

      if (this.questionsModel[questionIndex].type === SurveyQuestionType.OpenEnded) {
        const openEndedControl = this.answerForm.controls[`q${questionIndex}-openEnded-input`];
        let surveyAnswerModel = this.surveyAnswerModel.find(c => c.questionId === this.questionsModel[questionIndex].id);
        if (openEndedControl?.value?.trim().length > 0) {
          surveyAnswerModel.answerText = openEndedControl.value.trim();
        } else {
          surveyAnswerModel.answerText = null;
        }
      }


      if (this.questionsModel[questionIndex].type === SurveyQuestionType.OpenEndedWithMultiLines) {
        const multipleOpenEndedControl = this.answerForm.controls[`q${questionIndex}-openEnded-textarea`];
        let surveyAnswerModel = this.surveyAnswerModel.find(c => c.questionId === this.questionsModel[questionIndex].id);

        if (multipleOpenEndedControl?.value?.trim().length > 0) {
          surveyAnswerModel.answerText = multipleOpenEndedControl.value.trim();
        } else {
          surveyAnswerModel.answerText = null;
        }
      }

      if (this.questionsModel[questionIndex].type === SurveyQuestionType.RatingScale) {
        const ratingControl = this.answerForm.controls[`q${questionIndex}-rating`];
        let surveyAnswerModel = this.surveyAnswerModel.find(c => c.questionId === this.questionsModel[questionIndex].id);

        if (ratingControl?.value > 0) {
          surveyAnswerModel.answerNumeric = ratingControl.value;
        } else {
          surveyAnswerModel.answerNumeric = null;
        }
      }
    }

    this.surveyAnswerModel = this.surveyAnswerModel.filter(c => c.answerText || c.answerNumeric);
  }

  createSurveyAnswerModel(question: SurveyQuestionModel, option?: SurveyQuestionOptionModel) {
    return {
      id: 0,
      answerText: null,
      answerNumeric: null,
      username: this.currentUser.email,
      questionId: question.id,
      surveyId: question.surveyId,
      optionId: option ? option.id : null,
      questionType: question.type,
      isIncludeOpenEndedText: question.isIncludeOpenEndedText
    } as SurveyAnswerModel
  }

  onClickStar(questionIndex: number, starRatingArray: any[], clickedIndex: number) {
    let ratingControl = this.answerForm.controls[`q${questionIndex}-rating`];
    if (clickedIndex === 0 && starRatingArray.filter(c => c.type === this.solidFaStar).length === 1) {
      starRatingArray[0].type = this.regularFaStar;
      ratingControl.setValue(null);
      return;
    }

    for (let index = 0; index < starRatingArray.length; index++) {
      if (index <= clickedIndex) {
        starRatingArray[index].type = this.solidFaStar;
      } else {
        starRatingArray[index].type = this.regularFaStar;
      }
    }

    ratingControl.setValue(clickedIndex + 1);
  }

  onFocusIncludeOpenEndedText() {

  }

  onChangeSingleAnswer(option: SurveyQuestionOptionModel) {
    let surveyAnswerModel = this.surveyAnswerModel.find(c => c.questionId === option.questionId);
    surveyAnswerModel.optionId = option.id;
  }

  get surveyName() {
    // User is creating new survey and click on preview, not save yet
    if (!StringHelper.isNullOrEmpty(this.surveyModel?.name)) {
      return this.surveyModel.name;
    }
    // After saved
    return this.questionsModel[0]?.survey?.name;
  }

  onPublish() {
    this.surveyQuestionAnswerService.publishSurvey(this.questionsModel[0].surveyId, this.surveyModel).subscribe(
      (data: SurveyModel) => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.survey');
        if (data.id !== 0) {
          this.router.navigateByUrl(`/surveys/view/${data.id}`);
          return;
        }

        this.router.navigateByUrl(`/surveys/view/${this.questionsModel[0].surveyId}`);
      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.survey');
      }
    )
  }

  onSubmit() {
    this.mapSurveyAnswerModel();
    this.surveyQuestionAnswerService.submitSurvey(this.questionsModel[0].surveyId, this.surveyAnswerModel).subscribe(
      data => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.survey');
        this.router.navigateByUrl('/surveys/template?mode=submitted')
      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.survey');
      }
    )
  }

  onCancel() {
    if (this.surveyModel.id === 0) {
      this.router.navigate(['/surveys/add/0']);
      this.surveyQuestionAnswerService.previewFormSubject.next(this.surveyModel);
      return;
    }
    this.router.navigateByUrl(`/surveys/view/${this.questionsModel[0].surveyId}`);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
