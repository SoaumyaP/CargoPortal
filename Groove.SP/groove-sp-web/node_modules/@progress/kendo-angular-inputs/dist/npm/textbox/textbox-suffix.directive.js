/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
/**
 * Specifies the adornments in the suffix container ([see examples]({% slug adornments_textbox %}#toc-suffixadornments)).
 *  @example
 * ```ts-no-run
 * _@Component({
 * selector: 'my-app',
 * template: `
 *  <kendo-textbox>
 *    <ng-template kendoTextBoxSuffixTemplate>
 *        <button kendoButton look="clear" icon="image"></button>
 *    </ng-template>
 *  </kendo-textbox>
 * `
 * })
 * class AppComponent {}
 * ```
 */
var TextBoxSuffixTemplateDirective = /** @class */ (function () {
    function TextBoxSuffixTemplateDirective(templateRef) {
        this.templateRef = templateRef;
    }
    TextBoxSuffixTemplateDirective.decorators = [
        { type: core_1.Directive, args: [{
                    selector: '[kendoTextBoxSuffixTemplate]'
                },] },
    ];
    /** @nocollapse */
    TextBoxSuffixTemplateDirective.ctorParameters = function () { return [
        { type: core_1.TemplateRef, decorators: [{ type: core_1.Optional }] }
    ]; };
    return TextBoxSuffixTemplateDirective;
}());
exports.TextBoxSuffixTemplateDirective = TextBoxSuffixTemplateDirective;
