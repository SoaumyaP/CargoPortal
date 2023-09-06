/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { DatePickerMessages } from './messages';
/**
 * Custom component messages override default component messages ([see example]({% slug globalization_dateinputs %}#toc-custom-messages)).
 */
export class DatePickerCustomMessagesComponent extends DatePickerMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
DatePickerCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: DatePickerMessages,
                        useExisting: forwardRef(() => DatePickerCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-datepicker-messages',
                template: ``
            },] },
];
/** @nocollapse */
DatePickerCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
