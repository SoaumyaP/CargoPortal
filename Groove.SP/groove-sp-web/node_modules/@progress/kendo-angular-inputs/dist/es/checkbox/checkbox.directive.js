/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, HostBinding } from '@angular/core';
/**
 * Represents the directive that renders the [Kendo UI CheckBox]({% slug overview_checkbox %}) input component.
 * The directive is placed on input type="checkbox" elements.
 *
 * @example
 * ```ts-no-run
 * <input type="checkbox" kendoCheckBox />
 * ```
 */
var CheckBoxDirective = /** @class */ (function () {
    function CheckBoxDirective() {
        this.kendoClass = true;
    }
    CheckBoxDirective.decorators = [
        { type: Directive, args: [{
                    selector: 'input[kendoCheckBox]'
                },] },
    ];
    CheckBoxDirective.propDecorators = {
        kendoClass: [{ type: HostBinding, args: ['class.k-checkbox',] }]
    };
    return CheckBoxDirective;
}());
export { CheckBoxDirective };
