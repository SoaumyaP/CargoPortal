/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, Optional, TemplateRef } from '@angular/core';
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
        { type: Directive, args: [{
                    selector: '[kendoTextBoxSuffixTemplate]'
                },] },
    ];
    /** @nocollapse */
    TextBoxSuffixTemplateDirective.ctorParameters = function () { return [
        { type: TemplateRef, decorators: [{ type: Optional }] }
    ]; };
    return TextBoxSuffixTemplateDirective;
}());
export { TextBoxSuffixTemplateDirective };
