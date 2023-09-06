/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { TextBoxMessages } from './messages';
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
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: TextBoxMessages,
                            useExisting: forwardRef(function () { return LocalizedTextBoxMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoTextBoxLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    LocalizedTextBoxMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return LocalizedTextBoxMessagesDirective;
}(TextBoxMessages));
export { LocalizedTextBoxMessagesDirective };
