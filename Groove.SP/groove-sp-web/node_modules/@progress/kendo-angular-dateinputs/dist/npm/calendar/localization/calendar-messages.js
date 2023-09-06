/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var core_1 = require("@angular/core");
var kendo_angular_l10n_1 = require("@progress/kendo-angular-l10n");
/**
 * @hidden
 */
var CalendarMessages = /** @class */ (function (_super) {
    tslib_1.__extends(CalendarMessages, _super);
    function CalendarMessages() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    CalendarMessages.propDecorators = {
        today: [{ type: core_1.Input }]
    };
    return CalendarMessages;
}(kendo_angular_l10n_1.ComponentMessages));
exports.CalendarMessages = CalendarMessages;
