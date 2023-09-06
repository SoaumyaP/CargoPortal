/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TextBoxMessages } from './messages';
/**
 * @hidden
 */
export class LocalizedTextBoxMessagesDirective extends TextBoxMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedTextBoxMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: TextBoxMessages,
                        useExisting: forwardRef(() => LocalizedTextBoxMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoTextBoxLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedTextBoxMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
