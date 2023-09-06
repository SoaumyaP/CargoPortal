/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { RangeSliderMessages } from './messages';
/**
 * @hidden
 */
export class LocalizedRangeSliderMessagesDirective extends RangeSliderMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedRangeSliderMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: RangeSliderMessages,
                        useExisting: forwardRef(() => LocalizedRangeSliderMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoSliderLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedRangeSliderMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];
