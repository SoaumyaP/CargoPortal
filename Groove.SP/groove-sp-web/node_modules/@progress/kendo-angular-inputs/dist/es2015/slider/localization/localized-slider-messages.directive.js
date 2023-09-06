/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { SliderMessages } from './messages';
/**
 * @hidden
 */
export class LocalizedSliderMessagesDirective extends SliderMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedSliderMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: SliderMessages,
                        useExisting: forwardRef(() => LocalizedSliderMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoSliderLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedSliderMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
