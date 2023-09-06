/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { Messages } from './messages';
/**
 * @hidden
 */
var LocalizedSwitchMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(LocalizedSwitchMessagesDirective, _super);
    function LocalizedSwitchMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    LocalizedSwitchMessagesDirective.decorators = [
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: Messages,
                            useExisting: forwardRef(function () { return LocalizedSwitchMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoSwitchLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    LocalizedSwitchMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return LocalizedSwitchMessagesDirective;
}(Messages));
export { LocalizedSwitchMessagesDirective };
