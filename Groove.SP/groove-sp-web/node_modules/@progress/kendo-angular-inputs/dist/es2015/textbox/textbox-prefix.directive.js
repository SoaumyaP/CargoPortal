/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, Optional, TemplateRef } from '@angular/core';
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
export class TextBoxPrefixTemplateDirective {
    constructor(templateRef) {
        this.templateRef = templateRef;
    }
}
TextBoxPrefixTemplateDirective.decorators = [
    { type: Directive, args: [{
                selector: '[kendoTextBoxPrefixTemplate]'
            },] },
];
/** @nocollapse */
TextBoxPrefixTemplateDirective.ctorParameters = () => [
    { type: TemplateRef, decorators: [{ type: Optional }] }
];
