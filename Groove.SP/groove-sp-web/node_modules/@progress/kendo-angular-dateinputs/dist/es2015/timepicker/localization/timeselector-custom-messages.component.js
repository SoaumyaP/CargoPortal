/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TimePickerMessages } from './messages';
/**
 * @hidden
 *
 * Custom component messages override default component messages.
 */
export class TimeSelectorCustomMessagesComponent extends TimePickerMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
TimeSelectorCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: TimePickerMessages,
                        useExisting: forwardRef(() => TimeSelectorCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-timeselector-messages',
                template: ``
            },] },
];
/** @nocollapse */
TimeSelectorCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
