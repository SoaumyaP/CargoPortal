/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var core_1 = require("@angular/core");
var kendo_angular_l10n_1 = require("@progress/kendo-angular-l10n");
var multiview_calendar_messages_1 = require("./multiview-calendar-messages");
/**
 * @hidden
 */
var MultiViewCalendarLocalizedMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(MultiViewCalendarLocalizedMessagesDirective, _super);
    function MultiViewCalendarLocalizedMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    MultiViewCalendarLocalizedMessagesDirective.decorators = [
        { type: core_1.Directive, args: [{
                    providers: [
                        {
                            provide: multiview_calendar_messages_1.Messages,
                            useExisting: core_1.forwardRef(function () { return MultiViewCalendarLocalizedMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoMultiViewCalendarLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    MultiViewCalendarLocalizedMessagesDirective.ctorParameters = function () { return [
        { type: kendo_angular_l10n_1.LocalizationService }
    ]; };
    return MultiViewCalendarLocalizedMessagesDirective;
}(multiview_calendar_messages_1.Messages));
exports.MultiViewCalendarLocalizedMessagesDirective = MultiViewCalendarLocalizedMessagesDirective;
