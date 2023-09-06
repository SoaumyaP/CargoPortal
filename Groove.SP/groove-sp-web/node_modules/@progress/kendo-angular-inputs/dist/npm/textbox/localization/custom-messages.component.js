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
 * Custom component messages override default component messages.
 */
var TextBoxCustomMessagesComponent = /** @class */ (function (_super) {
    tslib_1.__extends(TextBoxCustomMessagesComponent, _super);
    function TextBoxCustomMessagesComponent(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    Object.defineProperty(TextBoxCustomMessagesComponent.prototype, "override", {
        get: function () {
            return true;
        },
        enumerable: true,
        configurable: true
    });
    TextBoxCustomMessagesComponent.decorators = [
        { type: core_1.Component, args: [{
                    providers: [
                        {
                            provide: messages_1.TextBoxMessages,
                            useExisting: core_1.forwardRef(function () { return TextBoxCustomMessagesComponent; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: 'kendo-textbox-messages',
                    template: ""
                },] },
    ];
    /** @nocollapse */
    TextBoxCustomMessagesComponent.ctorParameters = function () { return [
        { type: kendo_angular_l10n_1.LocalizationService }
    ]; };
    return TextBoxCustomMessagesComponent;
}(messages_1.TextBoxMessages));
exports.TextBoxCustomMessagesComponent = TextBoxCustomMessagesComponent;
