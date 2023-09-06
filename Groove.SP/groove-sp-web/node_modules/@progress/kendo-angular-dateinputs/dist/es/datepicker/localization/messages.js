/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Input } from '@angular/core';
import { ComponentMessages } from '@progress/kendo-angular-l10n';
/**
 * @hidden
 */
var DatePickerMessages = /** @class */ (function (_super) {
    tslib_1.__extends(DatePickerMessages, _super);
    function DatePickerMessages() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DatePickerMessages.propDecorators = {
        today: [{ type: Input }],
        toggle: [{ type: Input }]
    };
    return DatePickerMessages;
}(ComponentMessages));
export { DatePickerMessages };
