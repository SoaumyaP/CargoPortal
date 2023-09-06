/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ViewContainerRef } from '@angular/core';
/**
 * Used for configuring the options of the popup container.
 *
 * @example
 * ```ts
 * _@Component({
 * selector: 'my-app',
 * template: `
 *  <kendo-datepicker [popupSettings]="{ animate: false }">
 *  </kendo-datepicker>
 * `
 * })
 * export class AppComponent {
 * }
 * ```
 */
export interface PopupSettings {
    /**
     * Controls the popup animation. By default, the open and close animations are enabled.
     */
    animate?: boolean;
    /**
     * Controls the popup container. By default, the popup will be appended to the root component.
     */
    appendTo?: 'root' | 'component' | ViewContainerRef;
    /**
     * Specifies a list of CSS classes that are used for styling the popup.
     */
    popupClass?: string;
}
