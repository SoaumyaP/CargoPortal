/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { CalendarMessages } from './calendar-messages';
/**
 * @hidden
 */
export class CalendarLocalizedMessagesDirective extends CalendarMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
CalendarLocalizedMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: CalendarMessages,
                        useExisting: forwardRef(() => CalendarLocalizedMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoCalendarLocalizedMessages]'
            },] },
];
/** @nocollapse */
CalendarLocalizedMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
