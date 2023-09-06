/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Input } from '@angular/core';
import { ComponentMessages } from '@progress/kendo-angular-l10n';
/**
 * @hidden
 */
export class TimePickerMessages extends ComponentMessages {
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
