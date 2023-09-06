import { ReportOptionModel } from './report.option.model';
import { SubscriberModel } from './subscriber.model';
import { TelerikActivityModel } from './telerik-activity.model';

export interface SchedulingModel {
    /**
     * CSPortal scheduling id
     */
    id: number;
    canDelete: boolean;
    canEdit: boolean;
    category: string;
    categoryId: string;
    documentFormat: string;
    documentFormatDescr: string;
    enabled: boolean;
    isRecurrent: boolean;
    mailTemplateBody: string;
    mailTemplateSubject: string;
    name: string;
    nextOccurrence: string | null;
    parameters: string;
    recurrenceRule: string;
    report: string;
    reportId: string;
    startDate: Date;
    csPortalReportId: number;
    csPortalReport?: ReportOptionModel;
    subscribers?: Array<SubscriberModel>;
    activities?: Array<TelerikActivityModel>;
    status?: number;
    statusName?: string;
    telerikSchedulingId?: string;
}
