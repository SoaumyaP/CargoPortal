/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, HostBinding } from '@angular/core';
/**
 * Represents the directive that renders the [Kendo UI RadioButton]({% slug overview_checkbox %}) input component.
 * The directive is placed on input type="radio" elements.
 *
 * @example
 * ```ts-no-run
 * <input type="radio" kendoRadioButton />
 * ```
 */
var RadioButtonDirective = /** @class */ (function () {
    function RadioButtonDirective() {
        this.kendoClass = true;
    }
    RadioButtonDirective.decorators = [
        { type: Directive, args: [{
                    selector: 'input[kendoRadioButton]'
                },] },
    ];
    RadioButtonDirective.propDecorators = {
        kendoClass: [{ type: HostBinding, args: ['class.k-radio',] }]
    };
    return RadioButtonDirective;
}());
export { RadioButtonDirective };
