/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { DateInputMessages } from './messages';
/**
 * @hidden
 */
var DateInputLocalizedMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(DateInputLocalizedMessagesDirective, _super);
    function DateInputLocalizedMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    DateInputLocalizedMessagesDirective.decorators = [
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: DateInputMessages,
                            useExisting: forwardRef(function () { return DateInputLocalizedMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoDateInputLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    DateInputLocalizedMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return DateInputLocalizedMessagesDirective;
}(DateInputMessages));
export { DateInputLocalizedMessagesDirective };
