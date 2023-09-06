/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
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
export declare class TextBoxSeparatorComponent {
    hostClass: boolean;
}
