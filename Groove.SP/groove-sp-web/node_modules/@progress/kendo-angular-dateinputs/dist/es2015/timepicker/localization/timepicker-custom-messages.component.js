/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TimePickerMessages } from './messages';
/**
 * Custom component messages override default component messages ([see example]({% slug globalization_dateinputs %}#toc-custom-messages)).
 */
export class TimePickerCustomMessagesComponent extends TimePickerMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
TimePickerCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: TimePickerMessages,
                        useExisting: forwardRef(() => TimePickerCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-timepicker-messages',
                template: ``
            },] },
];
/** @nocollapse */
TimePickerCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
