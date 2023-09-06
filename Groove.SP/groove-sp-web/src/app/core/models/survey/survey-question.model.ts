import { SurveyQuestionType } from "../enums/enums";
import { SurveyQuestionOptionModel } from "./survey-question-option.model";
import { SurveyModel } from "./survey.model";

export interface SurveyQuestionModel {
    id: number;
    content: string;
    sequence: number;
    type: SurveyQuestionType;
    typeName: string;
    starRating: number | null;
    starRatingArray: any[] | null;
    lowValueLabel: string;
    highValueLabel: string;
    isIncludeOpenEndedText: boolean;
    placeHolderText: string;
    surveyId: number;
    survey?: SurveyModel;
    options: SurveyQuestionOptionModel[];

    // GUI only
    dragging: boolean | null;
}