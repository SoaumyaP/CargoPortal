import { TelerikAccessTokenModel } from './telerik-access-token.model';
import { TelerikReportParameterModel } from './telerik-report-parameter.model';

export class TelerikReportSourceModel {
    name?: string;
    reportServerUrl?: string;
    token?: TelerikAccessTokenModel;
    accessToken?: string;
    clientId?: number;
    parameters?: Array<TelerikReportParameterModel>;

    // properties needed to created new scheduling
    category?: string;
    categoryId?: string;
    report?: string;
    reportId?: string;
  }
