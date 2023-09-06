/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { Messages } from './messages';
/**
 * @hidden
 */
export class LocalizedSwitchMessagesDirective extends Messages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedSwitchMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: Messages,
                        useExisting: forwardRef(() => LocalizedSwitchMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoSwitchLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedSwitchMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
