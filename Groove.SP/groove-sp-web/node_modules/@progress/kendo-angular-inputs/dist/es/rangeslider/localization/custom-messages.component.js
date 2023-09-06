/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { RangeSliderMessages } from './messages';
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
        { type: Component, args: [{
                    providers: [
                        {
                            provide: RangeSliderMessages,
                            useExisting: forwardRef(function () { return RangeSliderCustomMessagesComponent; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: 'kendo-rangeslider-messages',
                    template: ""
                },] },
    ];
    /** @nocollapse */
    RangeSliderCustomMessagesComponent.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return RangeSliderCustomMessagesComponent;
}(RangeSliderMessages));
export { RangeSliderCustomMessagesComponent };
