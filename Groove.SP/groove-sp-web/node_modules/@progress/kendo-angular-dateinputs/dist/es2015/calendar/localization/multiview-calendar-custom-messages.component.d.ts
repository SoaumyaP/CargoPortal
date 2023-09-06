/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { Messages } from './multiview-calendar-messages';
/**
 * Custom component messages override default component messages ([see example]({% slug globalization_dateinputs %}#toc-custom-messages)).
 */
export declare class MultiViewCalendarCustomMessagesComponent extends Messages {
    protected service: LocalizationService;
    constructor(service: LocalizationService);
    protected readonly override: boolean;
}
