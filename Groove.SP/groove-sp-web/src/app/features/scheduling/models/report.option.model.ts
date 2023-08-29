export interface ReportOptionModel {
    id: number;
    reportName: string;
    reportUrl: string;
    reportDescription: string;
    lastRunTime: string | null;
    reportGroup: string;
    telerikCategoryId: string;
    telerikCategoryName: string;
    telerikReportId: string;
}
