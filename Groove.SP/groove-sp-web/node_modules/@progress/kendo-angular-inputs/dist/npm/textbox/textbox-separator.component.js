/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
/**
 * Specifies a separator in the content of the TextBox ([see examples]({% slug adornments_textbox %}#toc-separator)).
 * @example
 * ```ts-no-run
 * _@Component({
 * selector: 'my-app',
 * template: `
 *  <kendo-textbox>
 *    <ng-template kendoTextBoxSuffixTemplate>
 *        <button kendoButton look="clear" icon="image"></button>
 *        <kendo-textbox-separator></kendo-textbox-separator>
 *    </ng-template>
 *  </kendo-textbox>
 * `
 * })
 * class AppComponent {}
 * ```
 */
var TextBoxSeparatorComponent = /** @class */ (function () {
    function TextBoxSeparatorComponent() {
        this.hostClass = true;
    }
    TextBoxSeparatorComponent.decorators = [
        { type: core_1.Component, args: [{
                    selector: 'kendo-textbox-separator',
                    template: ""
                },] },
    ];
    TextBoxSeparatorComponent.propDecorators = {
        hostClass: [{ type: core_1.HostBinding, args: ['class.k-textbox-separator',] }]
    };
    return TextBoxSeparatorComponent;
}());
exports.TextBoxSeparatorComponent = TextBoxSeparatorComponent;
