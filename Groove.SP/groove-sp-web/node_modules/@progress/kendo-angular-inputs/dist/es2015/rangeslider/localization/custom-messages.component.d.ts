/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { RangeSliderMessages } from './messages';
/**
 * Custom component messages override default component messages.
 */
export declare class RangeSliderCustomMessagesComponent extends RangeSliderMessages {
    protected service: LocalizationService;
    constructor(service: LocalizationService);
    protected readonly override: boolean;
}
