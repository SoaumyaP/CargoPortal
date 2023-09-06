/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { Messages } from './messages';
/**
 * Custom component messages override default component messages.
 */
export class SwitchCustomMessagesComponent extends Messages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
SwitchCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: Messages,
                        useExisting: forwardRef(() => SwitchCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-switch-messages',
                template: ``
            },] },
];
/** @nocollapse */
SwitchCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
