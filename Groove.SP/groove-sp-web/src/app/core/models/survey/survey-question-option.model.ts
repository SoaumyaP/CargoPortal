export interface SurveyQuestionOptionModel {
    id: number;
    content: string;
    questionId: number;

    // GUI only
    removed: boolean | null;
    sequence: number | null;
}