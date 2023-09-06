/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Component, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TextBoxMessages } from './messages';
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
        { type: Component, args: [{
                    providers: [
                        {
                            provide: TextBoxMessages,
                            useExisting: forwardRef(function () { return TextBoxCustomMessagesComponent; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: 'kendo-textbox-messages',
                    template: ""
                },] },
    ];
    /** @nocollapse */
    TextBoxCustomMessagesComponent.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return TextBoxCustomMessagesComponent;
}(TextBoxMessages));
export { TextBoxCustomMessagesComponent };
