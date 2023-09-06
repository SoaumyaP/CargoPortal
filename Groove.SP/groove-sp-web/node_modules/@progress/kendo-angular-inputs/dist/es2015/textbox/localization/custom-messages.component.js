/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TextBoxMessages } from './messages';
/**
 * Custom component messages override default component messages.
 */
export class TextBoxCustomMessagesComponent extends TextBoxMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
TextBoxCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: TextBoxMessages,
                        useExisting: forwardRef(() => TextBoxCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-textbox-messages',
                template: ``
            },] },
];
/** @nocollapse */
TextBoxCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
