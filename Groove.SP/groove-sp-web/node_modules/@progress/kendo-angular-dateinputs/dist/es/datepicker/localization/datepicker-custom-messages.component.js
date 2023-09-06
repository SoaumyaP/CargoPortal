/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { DatePickerMessages } from './messages';
/**
 * Custom component messages override default component messages ([see example]({% slug globalization_dateinputs %}#toc-custom-messages)).
 */
var DatePickerCustomMessagesComponent = /** @class */ (function (_super) {
    tslib_1.__extends(DatePickerCustomMessagesComponent, _super);
    function DatePickerCustomMessagesComponent(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    Object.defineProperty(DatePickerCustomMessagesComponent.prototype, "override", {
        get: function () {
            return true;
        },
        enumerable: true,
        configurable: true
    });
    DatePickerCustomMessagesComponent.decorators = [
        { type: Component, args: [{
                    providers: [
                        {
                            provide: DatePickerMessages,
                            useExisting: forwardRef(function () { return DatePickerCustomMessagesComponent; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: 'kendo-datepicker-messages',
                    template: ""
                },] },
    ];
    /** @nocollapse */
    DatePickerCustomMessagesComponent.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return DatePickerCustomMessagesComponent;
}(DatePickerMessages));
export { DatePickerCustomMessagesComponent };
