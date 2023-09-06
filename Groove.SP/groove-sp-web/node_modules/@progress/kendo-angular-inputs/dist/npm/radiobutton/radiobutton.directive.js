/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
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
        { type: core_1.Directive, args: [{
                    selector: 'input[kendoRadioButton]'
                },] },
    ];
    RadioButtonDirective.propDecorators = {
        kendoClass: [{ type: core_1.HostBinding, args: ['class.k-radio',] }]
    };
    return RadioButtonDirective;
}());
exports.RadioButtonDirective = RadioButtonDirective;
