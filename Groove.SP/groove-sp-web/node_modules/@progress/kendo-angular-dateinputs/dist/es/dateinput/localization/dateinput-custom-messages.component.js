/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { DateInputMessages } from './messages';
/**
 * Custom component messages override default component messages ([see example]({% slug globalization_dateinputs %}#toc-custom-messages)).
 */
var DateInputCustomMessagesComponent = /** @class */ (function (_super) {
    tslib_1.__extends(DateInputCustomMessagesComponent, _super);
    function DateInputCustomMessagesComponent(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    Object.defineProperty(DateInputCustomMessagesComponent.prototype, "override", {
        get: function () {
            return true;
        },
        enumerable: true,
        configurable: true
    });
    DateInputCustomMessagesComponent.decorators = [
        { type: Component, args: [{
                    providers: [
                        {
                            provide: DateInputMessages,
                            useExisting: forwardRef(function () { return DateInputCustomMessagesComponent; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: 'kendo-dateinput-messages',
                    template: ""
                },] },
    ];
    /** @nocollapse */
    DateInputCustomMessagesComponent.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return DateInputCustomMessagesComponent;
}(DateInputMessages));
export { DateInputCustomMessagesComponent };
