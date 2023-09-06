/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { DateInputMessages } from './messages';
/**
 * @hidden
 */
export class DateInputLocalizedMessagesDirective extends DateInputMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
DateInputLocalizedMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: DateInputMessages,
                        useExisting: forwardRef(() => DateInputLocalizedMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoDateInputLocalizedMessages]'
            },] },
];
/** @nocollapse */
DateInputLocalizedMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
