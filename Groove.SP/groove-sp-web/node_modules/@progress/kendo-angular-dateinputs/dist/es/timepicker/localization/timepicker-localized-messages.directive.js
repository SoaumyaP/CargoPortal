/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TimePickerMessages } from './messages';
/**
 * @hidden
 */
var TimePickerLocalizedMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(TimePickerLocalizedMessagesDirective, _super);
    function TimePickerLocalizedMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    TimePickerLocalizedMessagesDirective.decorators = [
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: TimePickerMessages,
                            useExisting: forwardRef(function () { return TimePickerLocalizedMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoTimePickerLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    TimePickerLocalizedMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return TimePickerLocalizedMessagesDirective;
}(TimePickerMessages));
export { TimePickerLocalizedMessagesDirective };
