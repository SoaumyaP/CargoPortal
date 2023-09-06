/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { NumericTextBoxMessages } from './messages';
/**
 * Custom component messages override default component messages.
 */
export class NumericTextBoxCustomMessagesComponent extends NumericTextBoxMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
NumericTextBoxCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: NumericTextBoxMessages,
                        useExisting: forwardRef(() => NumericTextBoxCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-numerictextbox-messages',
                template: ``
            },] },
];
/** @nocollapse */
NumericTextBoxCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
