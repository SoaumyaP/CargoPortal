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
var TimePickerMessages = /** @class */ (function (_super) {
    tslib_1.__extends(TimePickerMessages, _super);
    function TimePickerMessages() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    TimePickerMessages.propDecorators = {
        accept: [{ type: core_1.Input }],
        acceptLabel: [{ type: core_1.Input }],
        cancel: [{ type: core_1.Input }],
        cancelLabel: [{ type: core_1.Input }],
        now: [{ type: core_1.Input }],
        nowLabel: [{ type: core_1.Input }],
        toggle: [{ type: core_1.Input }]
    };
    return TimePickerMessages;
}(kendo_angular_l10n_1.ComponentMessages));
exports.TimePickerMessages = TimePickerMessages;
