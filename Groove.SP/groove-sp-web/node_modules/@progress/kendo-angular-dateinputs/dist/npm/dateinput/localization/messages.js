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
var DateInputMessages = /** @class */ (function (_super) {
    tslib_1.__extends(DateInputMessages, _super);
    function DateInputMessages() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DateInputMessages.propDecorators = {
        decrement: [{ type: core_1.Input }],
        increment: [{ type: core_1.Input }]
    };
    return DateInputMessages;
}(kendo_angular_l10n_1.ComponentMessages));
exports.DateInputMessages = DateInputMessages;
