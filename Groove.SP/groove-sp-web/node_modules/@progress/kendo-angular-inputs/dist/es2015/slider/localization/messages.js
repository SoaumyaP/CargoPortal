/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Input } from '@angular/core';
import { ComponentMessages } from '@progress/kendo-angular-l10n';
/**
 * @hidden
 */
export class SliderMessages extends ComponentMessages {
}
SliderMessages.propDecorators = {
    decrement: [{ type: Input }],
    increment: [{ type: Input }],
    dragHandle: [{ type: Input }]
};
