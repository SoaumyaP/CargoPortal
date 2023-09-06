/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { Messages } from './messages';
/**
 * Custom component messages override default component messages
 * ([see example]({% slug rtl_dropdowns %}#toc-messages)).
 */
export class CustomMessagesComponent extends Messages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
CustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: Messages,
                        useExisting: forwardRef(() => CustomMessagesComponent)
                    }
                ],
                selector: 'kendo-dropdownlist-messages, kendo-combobox-messages, kendo-autocomplete-messages, kendo-multiselect-messages',
                template: ``
            },] },
];
/** @nocollapse */
CustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];
