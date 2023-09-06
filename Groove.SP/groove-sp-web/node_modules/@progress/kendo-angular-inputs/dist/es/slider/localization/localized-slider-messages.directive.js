/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { SliderMessages } from './messages';
/**
 * @hidden
 */
var LocalizedSliderMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(LocalizedSliderMessagesDirective, _super);
    function LocalizedSliderMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    LocalizedSliderMessagesDirective.decorators = [
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: SliderMessages,
                            useExisting: forwardRef(function () { return LocalizedSliderMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoSliderLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    LocalizedSliderMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return LocalizedSliderMessagesDirective;
}(SliderMessages));
export { LocalizedSliderMessagesDirective };
