/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { RangeSliderMessages } from './messages';
/**
 * @hidden
 */
var LocalizedRangeSliderMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(LocalizedRangeSliderMessagesDirective, _super);
    function LocalizedRangeSliderMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    LocalizedRangeSliderMessagesDirective.decorators = [
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: RangeSliderMessages,
                            useExisting: forwardRef(function () { return LocalizedRangeSliderMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoSliderLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    LocalizedRangeSliderMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return LocalizedRangeSliderMessagesDirective;
}(RangeSliderMessages));
export { LocalizedRangeSliderMessagesDirective };
