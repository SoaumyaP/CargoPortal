/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { Messages } from './multiview-calendar-messages';
/**
 * @hidden
 */
export class MultiViewCalendarLocalizedMessagesDirective extends Messages {
    constructor(service) {
        super();
        this.service = service;
    }
}
MultiViewCalendarLocalizedMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: Messages,
                        useExisting: forwardRef(() => MultiViewCalendarLocalizedMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoMultiViewCalendarLocalizedMessages]'
            },] },
];
/** @nocollapse */
MultiViewCalendarLocalizedMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
