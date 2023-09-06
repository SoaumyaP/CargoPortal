/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TextBoxMessages } from './messages';
/**
 * Custom component messages override default component messages.
 */
export declare class TextBoxCustomMessagesComponent extends TextBoxMessages {
    protected service: LocalizationService;
    constructor(service: LocalizationService);
    protected readonly override: boolean;
}
