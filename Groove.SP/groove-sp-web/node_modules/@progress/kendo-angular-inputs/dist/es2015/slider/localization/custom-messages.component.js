/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { SliderMessages } from './messages';
/**
 * Custom component messages override default component messages.
 */
export class SliderCustomMessagesComponent extends SliderMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
SliderCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: SliderMessages,
                        useExisting: forwardRef(() => SliderCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-slider-messages',
                template: ``
            },] },
];
/** @nocollapse */
SliderCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
