/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { DateInputMessages } from './messages';
/**
 * Custom component messages override default component messages ([see example]({% slug globalization_dateinputs %}#toc-custom-messages)).
 */
export class DateInputCustomMessagesComponent extends DateInputMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
DateInputCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: DateInputMessages,
                        useExisting: forwardRef(() => DateInputCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-dateinput-messages',
                template: ``
            },] },
];
/** @nocollapse */
DateInputCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
