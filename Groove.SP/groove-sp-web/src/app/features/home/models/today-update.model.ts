import { SurveyParticipantModel } from "src/app/core/models/survey/survey-participant.model";

export class TodayUpdateModel {
    totalAwaitingForSubmission: number;
    totalPendingForApproval: number;
    shortShip: number;
    surveyParticipants: SurveyParticipantModel[] = [];

    mobile_version: string;
    mobile_isNewRelease: boolean;
    mobile_packageUrl: string;

    // As loading data, it set to true to hide from GUI
    mobile_notificationDismissed: boolean = true;

    static get mobileNotificationDismissCookieName() { return 'dashboard.today.mobileNotificationDismissed'; }
    static get surveysNotificationDismissCookieName() { return 'dashboard.today.surveysNotificationDismissed'; }

}
