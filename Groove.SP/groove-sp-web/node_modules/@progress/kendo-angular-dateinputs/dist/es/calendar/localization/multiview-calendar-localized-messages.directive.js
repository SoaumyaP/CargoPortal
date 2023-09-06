/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Directive, forwardRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { Messages } from './multiview-calendar-messages';
/**
 * @hidden
 */
var MultiViewCalendarLocalizedMessagesDirective = /** @class */ (function (_super) {
    tslib_1.__extends(MultiViewCalendarLocalizedMessagesDirective, _super);
    function MultiViewCalendarLocalizedMessagesDirective(service) {
        var _this = _super.call(this) || this;
        _this.service = service;
        return _this;
    }
    MultiViewCalendarLocalizedMessagesDirective.decorators = [
        { type: Directive, args: [{
                    providers: [
                        {
                            provide: Messages,
                            useExisting: forwardRef(function () { return MultiViewCalendarLocalizedMessagesDirective; }) // tslint:disable-line:no-forward-ref
                        }
                    ],
                    selector: '[kendoMultiViewCalendarLocalizedMessages]'
                },] },
    ];
    /** @nocollapse */
    MultiViewCalendarLocalizedMessagesDirective.ctorParameters = function () { return [
        { type: LocalizationService }
    ]; };
    return MultiViewCalendarLocalizedMessagesDirective;
}(Messages));
export { MultiViewCalendarLocalizedMessagesDirective };
