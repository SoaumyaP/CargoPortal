/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { RangeSliderMessages } from './messages';
/**
 * Custom component messages override default component messages.
 */
export class RangeSliderCustomMessagesComponent extends RangeSliderMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
RangeSliderCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: RangeSliderMessages,
                        useExisting: forwardRef(() => RangeSliderCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-rangeslider-messages',
                template: ``
            },] },
];
/** @nocollapse */
RangeSliderCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
