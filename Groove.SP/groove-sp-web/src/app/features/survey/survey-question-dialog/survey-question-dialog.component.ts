import { Component, EventEmitter, Input, OnChanges, OnDestroy, Output, SimpleChanges, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { DropdownListModel, FormMode, SurveyQuestionType } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { SurveyQuestionOptionModel } from 'src/app/core/models/survey/survey-question-option.model';
import { SurveyQuestionModel } from 'src/app/core/models/survey/survey-question.model';

@Component({
  selector: 'app-survey-question-dialog',
  templateUrl: './survey-question-dialog.component.html',
  styleUrls: ['./survey-question-dialog.component.scss']
})
export class SurveyQuestionDialogComponent implements OnChanges {

  @Input() public model: SurveyQuestionModel;
  @Input() public isFormOpen: boolean = false;
  @Input() public formMode: string;

  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() save: EventEmitter<any> = new EventEmitter<SurveyQuestionModel>();

  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  questionTypeOptions: DropdownListModel<number>[] = [
    {
      label: 'label.singleAnswer',
      value: SurveyQuestionType.SingleAnswer
    },
    {
      label: 'label.multipleAnswers',
      value: SurveyQuestionType.MultiAnswers
    },
    {
      label: 'label.openEnded',
      value: SurveyQuestionType.OpenEnded
    },
    {
      label: 'label.openEndedWithMultipleLines',
      value: SurveyQuestionType.OpenEndedWithMultiLines
    },
    {
      label: 'label.ratingScale',
      value: SurveyQuestionType.RatingScale
    }
  ]

  constructor() { }
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['model']?.currentValue) {
      this.resetAnswerSequence();
    }
  }

  get isEditMode(): boolean {
    return this.formMode == FormMode.Edit;
  };
  get isAddMode(): boolean {
    return this.formMode == FormMode.Add;
  };
  get isViewMode(): boolean {
    return this.formMode == FormMode.View;
  };
  get formTitle(): string {
    let title = "";
    if (this.isAddMode) {
      title = 'label.addNewQuestion';
    }
    else if (this.isEditMode) {
      title = 'label.editAQuestion';
    }
    else {
      title = 'label.questionDetail';
    }
    return title;
  }

  get isSingleAnswerType(): boolean {
    return this.model.type === SurveyQuestionType.SingleAnswer;
  }
  get isMultiAnswerType(): boolean {
    return this.model.type === SurveyQuestionType.MultiAnswers;
  }
  get isRatingScaleType(): boolean {
    return this.model.type === SurveyQuestionType.RatingScale;
  }
  get isOpenEndedType(): boolean {
    return this.model.type === SurveyQuestionType.OpenEnded;
  }
  get isOpenEndedWithMultiLines(): boolean {
    return this.model.type === SurveyQuestionType.OpenEndedWithMultiLines;
  }

  /**
   * Get current last item index.
   * */
  get lastAnswerIndex(): number {
    let result = -1;
    for (let i = 0; i < this.model.options.length; i++) {
      if (!this.model.options[i].removed) {
        result = i;
      }
    }
    return result;
  }

  questionTypeChange(value: SurveyQuestionType): void {
    switch (value) {
      case SurveyQuestionType.SingleAnswer:
        if (this.model.options.length === 0) {
          this.addDefaultAnswerRow(3);
        }
        break;
      case SurveyQuestionType.MultiAnswers:
        if (this.model.options.length === 0) {
          this.addDefaultAnswerRow(3);
        }
        break;
      case SurveyQuestionType.RatingScale:
        this.model.starRating = 5; // as default
        
        setTimeout(() => {
          this.validateLowValueLabel();
          this.validateHighValueLabel();
        }, 1); // make sure the form control has been initialized
        break;
      case SurveyQuestionType.OpenEnded:
        setTimeout(() => {
          this.validatePlaceholderText();
        }, 1); // make sure the form control has been initialized
        break;
      case SurveyQuestionType.OpenEndedWithMultiLines:
        setTimeout(() => {
          this.validatePlaceholderText();
        }, 1); // make sure the form control has been initialized
        break;
      default:
        break;
    }
    this.model.typeName = this.questionTypeOptions.find(x => x.value === value).label;
  }

  answerValueChange(value: string, rowIndex: number): void {
    let lastItem = this.lastAnswerIndex === rowIndex;
    if (value.length > 0 && lastItem) {
      this.addDefaultAnswerRow(1);
    }
  }

  addDefaultAnswerRow(qty: number): void {
    for (let i = 0; i < qty; i++) {
      this.model.options.push({
        id: 0,
        content: "",
        questionId: 0,
        removed: false,
        sequence: this.nextAnswerSequence
      });
    }
  }

  get nextAnswerSequence() {
    var res = 1;
    const lastIndex = this.lastAnswerIndex;
    return lastIndex === -1 ? res : this.model.options[lastIndex].sequence + 1;
  }

  resetAnswerSequence() {
    for (let i = 0; i < this.filteredOptions.length; i++) {
      this.filteredOptions[i].sequence = i + 1;
    }
  }

  deleteAnswer(rowIndex: number): void {
    if ((this.isSingleAnswerType && this.filteredOptions.length <= 2) || (this.isMultiAnswerType && this.filteredOptions.length <= 1)) {
      this.model.options[rowIndex].content = "";
    }
    else {
      this.model.options[rowIndex].removed = true;
      this.mainForm.controls['answer_' + rowIndex].setErrors(null);
      
      this.resetAnswerSequence();
    }
  }

  get filteredOptions(): SurveyQuestionOptionModel[] {
    return this.model.options.filter(x => !x.removed);
  }

  validatePlaceholderText() {
    if (this.model.placeHolderText.length > 50) {
      this.mainForm.controls['answer_placeholder'].markAsTouched();
      this.mainForm.controls['answer_placeholder'].setErrors({'invalid': true});
    }
  }

  validateLowValueLabel() {
    if (this.model.lowValueLabel.length > 50) {
      this.mainForm.controls['lowValueLabel'].markAsTouched();
      this.mainForm.controls['lowValueLabel'].setErrors({'invalid': true});
    }
  }

  validateHighValueLabel() {
    if (this.model.highValueLabel.length > 50) {
      this.mainForm.controls['highValueLabel'].markAsTouched();
      this.mainForm.controls['highValueLabel'].setErrors({'invalid': true});
    }
  }

  onFormClosed(): void {
    this.close.emit();
  }

  onFormSave(): void {
    FormHelper.ValidateAllFields(this.mainForm);
    if (this.mainForm.invalid) {
      return;
    }

    this.model.options = this.filteredOptions;
    this.save.emit(this.model);
  }
}
