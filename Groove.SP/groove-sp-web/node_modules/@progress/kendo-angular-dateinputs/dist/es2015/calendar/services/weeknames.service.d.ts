/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { IntlService } from '@progress/kendo-angular-intl';
/**
 * @hidden
 */
export declare class WeekNamesService {
    private intl;
    constructor(intl: IntlService);
    getWeekNames(includeWeekNumber?: boolean): string[];
}
