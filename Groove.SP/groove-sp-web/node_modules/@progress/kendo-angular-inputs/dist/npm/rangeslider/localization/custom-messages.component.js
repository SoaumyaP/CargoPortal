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
var RangeSliderCustomMessagesComponent = /** @class */ (function (_super) {
    tslib_1.__extends(RangeSliderCustomMessagesComponent, _super);
    function RangeSliderCustomMessagesComponent(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    Object.defineProperty(RangeSliderCustomMessagesComponent.prototype, "override", {
        get: function () {
            return true;
        },
        enumerable: true,
        configurable: true
    });
    RangeSliderCustomMessagesComponent.decorators = [
        { type: core_1.Component, args: [{
                    providers: [
                        {
                            provide: messages_1.RangeSliderMessages,
                            useExisting: core_1.forwardRef(function () { return RangeSliderCustomMessagesComponent; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: 'kendo-rangeslider-messages',
                    template: ""
                },] },
    ];
    /** @nocollapse */
    RangeSliderCustomMessagesComponent.ctorParameters = function () { return [
        { type: kendo_angular_l10n_1.LocalizationService }
    ]; };
    return RangeSliderCustomMessagesComponent;
}(messages_1.RangeSliderMessages));
exports.RangeSliderCustomMessagesComponent = RangeSliderCustomMessagesComponent;
