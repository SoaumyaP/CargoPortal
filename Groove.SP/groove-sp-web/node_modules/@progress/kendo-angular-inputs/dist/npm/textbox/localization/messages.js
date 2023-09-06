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
var TextBoxMessages = /** @class */ (function (_super) {
    tslib_1.__extends(TextBoxMessages, _super);
    function TextBoxMessages() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    TextBoxMessages.propDecorators = {
        clear: [{ type: core_1.Input }]
    };
    return TextBoxMessages;
}(kendo_angular_l10n_1.ComponentMessages));
exports.TextBoxMessages = TextBoxMessages;
