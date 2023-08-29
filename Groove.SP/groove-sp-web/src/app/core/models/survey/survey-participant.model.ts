import { SurveyModel } from "./survey.model";

export interface SurveyParticipantModel {
    username: string;
    surveyId: number;
    isSubmitted: boolean;
    submittedOn: string | null;

    survey: SurveyModel
}