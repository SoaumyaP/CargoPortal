/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { CalendarMessages } from './calendar-messages';
/**
 * @hidden
 */
var CalendarLocalizedMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(CalendarLocalizedMessagesDirective, _super);
    function CalendarLocalizedMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    CalendarLocalizedMessagesDirective.decorators = [
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: CalendarMessages,
                            useExisting: forwardRef(function () { return CalendarLocalizedMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoCalendarLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    CalendarLocalizedMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return CalendarLocalizedMessagesDirective;
}(CalendarMessages));
export { CalendarLocalizedMessagesDirective };
