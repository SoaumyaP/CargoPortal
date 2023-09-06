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
var TimePickerMessages = /** @class */ (function (_super) {
    tslib_1.__extends(TimePickerMessages, _super);
    function TimePickerMessages() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    TimePickerMessages.propDecorators = {
        accept: [{ type: Input }],
        acceptLabel: [{ type: Input }],
        cancel: [{ type: Input }],
        cancelLabel: [{ type: Input }],
        now: [{ type: Input }],
        nowLabel: [{ type: Input }],
        toggle: [{ type: Input }]
    };
    return TimePickerMessages;
}(ComponentMessages));
export { TimePickerMessages };
