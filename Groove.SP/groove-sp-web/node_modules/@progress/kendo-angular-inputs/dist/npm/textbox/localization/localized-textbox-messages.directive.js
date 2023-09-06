/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var core_1 = require("@angular/core");
var kendo_angular_l10n_1 = require("@progress/kendo-angular-l10n");
var messages_1 = require("./messages");
/**
 * @hidden
 */
var LocalizedTextBoxMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(LocalizedTextBoxMessagesDirective, _super);
    function LocalizedTextBoxMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    LocalizedTextBoxMessagesDirective.decorators = [
        { type: core_1.Directive, args: [{
                    providers: [
                        {
                            provide: messages_1.TextBoxMessages,
                            useExisting: core_1.forwardRef(function () { return LocalizedTextBoxMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoTextBoxLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    LocalizedTextBoxMessagesDirective.ctorParameters = function () { return [
        { type: kendo_angular_l10n_1.LocalizationService }
    ]; };
    return LocalizedTextBoxMessagesDirective;
}(messages_1.TextBoxMessages));
exports.LocalizedTextBoxMessagesDirective = LocalizedTextBoxMessagesDirective;
