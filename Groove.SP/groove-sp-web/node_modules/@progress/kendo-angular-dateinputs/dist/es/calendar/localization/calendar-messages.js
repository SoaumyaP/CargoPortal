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
var CalendarMessages = /** @class */ (function (_super) {
    tslib_1.__extends(CalendarMessages, _super);
    function CalendarMessages() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    CalendarMessages.propDecorators = {
        today: [{ type: Input }]
    };
    return CalendarMessages;
}(ComponentMessages));
export { CalendarMessages };
