import { OrganizationType, SurveyParticipantType, SurveySendToUserType, SurveyStatus } from "../enums/enums";
import { SurveyQuestionModel } from "./survey-question.model";

export interface SurveyModel {
    id: number;
    name: string;
    description: string;
    participantType: SurveyParticipantType;
    userRole: number;
    organizationType: OrganizationType | null;
    specifiedOrganization: string;
    specifiedUser: string;
    sendToUser: SurveySendToUserType | null;
    isIncludeAffiliate: boolean;
    status: SurveyStatus;
    publishedDate: string | null;
    closedDate: string | null;
    questions: SurveyQuestionModel[];

    createdDate: Date;
    updatedDate: Date;
    createdBy: string;
    updatedBy: string;
}