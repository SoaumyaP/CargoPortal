import { SurveyQuestionType } from "../enums/enums";
import { SurveyQuestionModel } from "./survey-question.model";

export interface SurveyAnswerModel {
    id: number;
    answerText: string | null;
    answerNumeric: number | null;
    username: string;
    questionId: number;
    surveyId: number;
    optionId: number | null;
    questionType: SurveyQuestionType;
    isIncludeOpenEndedText: boolean;
}