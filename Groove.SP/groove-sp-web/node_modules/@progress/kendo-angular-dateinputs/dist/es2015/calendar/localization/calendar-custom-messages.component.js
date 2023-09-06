/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { CalendarMessages } from './calendar-messages';
/**
 * Custom component messages override default component messages ([see example]({% slug globalization_dateinputs %}#toc-custom-messages)).
 */
export class CalendarCustomMessagesComponent extends CalendarMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
CalendarCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: CalendarMessages,
                        useExisting: forwardRef(() => CalendarCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-calendar-messages',
                template: ``
            },] },
];
/** @nocollapse */
CalendarCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
