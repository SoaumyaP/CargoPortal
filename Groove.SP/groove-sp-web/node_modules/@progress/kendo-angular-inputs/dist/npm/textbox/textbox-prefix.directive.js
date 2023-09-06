/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
/**
 * Specifies the adornments in the prefix container ([see examples]({% slug adornments_textbox %}#toc-prefixadornments)).
 * @example
 * ```ts-no-run
 * _@Component({
 * selector: 'my-app',
 * template: `
 *  <kendo-textbox>
 *    <ng-template kendoTextBoxPrefixTemplate>
 *        <button kendoButton look="clear" icon="image"></button>
 *    </ng-template>
 *  </kendo-textbox>
 * `
 * })
 * class AppComponent {}
 * ```
 */
var TextBoxPrefixTemplateDirective = /** @class */ (function () {
    function TextBoxPrefixTemplateDirective(templateRef) {
        this.templateRef = templateRef;
    }
    TextBoxPrefixTemplateDirective.decorators = [
        { type: core_1.Directive, args: [{
                    selector: '[kendoTextBoxPrefixTemplate]'
                },] },
    ];
    /** @nocollapse */
    TextBoxPrefixTemplateDirective.ctorParameters = function () { return [
        { type: core_1.TemplateRef, decorators: [{ type: core_1.Optional }] }
    ]; };
    return TextBoxPrefixTemplateDirective;
}());
exports.TextBoxPrefixTemplateDirective = TextBoxPrefixTemplateDirective;
