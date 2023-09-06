/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TimePickerMessages } from './messages';
/**
 * @hidden
 */
export class TimeSelectorLocalizedMessagesDirective extends TimePickerMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
TimeSelectorLocalizedMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: TimePickerMessages,
                        useExisting: forwardRef(() => TimeSelectorLocalizedMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoTimeSelectorLocalizedMessages]'
            },] },
];
/** @nocollapse */
TimeSelectorLocalizedMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
