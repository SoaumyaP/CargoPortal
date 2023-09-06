/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { NumericTextBoxMessages } from './messages';
/**
 * @hidden
 */
export class LocalizedNumericTextBoxMessagesDirective extends NumericTextBoxMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedNumericTextBoxMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: NumericTextBoxMessages,
                        useExisting: forwardRef(() => LocalizedNumericTextBoxMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoNumericTextBoxLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedNumericTextBoxMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
