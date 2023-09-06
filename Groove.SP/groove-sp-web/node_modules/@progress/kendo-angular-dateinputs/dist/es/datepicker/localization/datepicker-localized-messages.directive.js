/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { DatePickerMessages } from './messages';
/**
 * @hidden
 */
var DatePickerLocalizedMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(DatePickerLocalizedMessagesDirective, _super);
    function DatePickerLocalizedMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    DatePickerLocalizedMessagesDirective.decorators = [
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: DatePickerMessages,
                            useExisting: forwardRef(function () { return DatePickerLocalizedMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoDatePickerLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    DatePickerLocalizedMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return DatePickerLocalizedMessagesDirective;
}(DatePickerMessages));
export { DatePickerLocalizedMessagesDirective };
