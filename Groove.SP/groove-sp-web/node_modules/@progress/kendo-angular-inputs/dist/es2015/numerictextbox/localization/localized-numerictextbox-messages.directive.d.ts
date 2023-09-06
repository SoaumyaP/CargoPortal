/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { NumericTextBoxMessages } from './messages';
/**
 * @hidden
 */
export declare class LocalizedNumericTextBoxMessagesDirective extends NumericTextBoxMessages {
    protected service: LocalizationService;
    constructor(service: LocalizationService);
}
