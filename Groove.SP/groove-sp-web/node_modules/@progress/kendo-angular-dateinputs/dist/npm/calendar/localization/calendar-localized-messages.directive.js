/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var core_1 = require("@angular/core");
var kendo_angular_l10n_1 = require("@progress/kendo-angular-l10n");
var calendar_messages_1 = require("./calendar-messages");
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
        { type: core_1.Directive, args: [{
                    providers: [
                        {
                            provide: calendar_messages_1.CalendarMessages,
                            useExisting: core_1.forwardRef(function () { return CalendarLocalizedMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoCalendarLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    CalendarLocalizedMessagesDirective.ctorParameters = function () { return [
        { type: kendo_angular_l10n_1.LocalizationService }
    ]; };
    return CalendarLocalizedMessagesDirective;
}(calendar_messages_1.CalendarMessages));
exports.CalendarLocalizedMessagesDirective = CalendarLocalizedMessagesDirective;
