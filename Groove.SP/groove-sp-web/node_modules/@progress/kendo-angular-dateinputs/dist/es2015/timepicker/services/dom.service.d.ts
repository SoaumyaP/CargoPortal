/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ElementRef } from '@angular/core';
/**
 * @hidden
 */
export declare class TimePickerDOMService {
    itemHeight: number;
    timeListHeight: number;
    ensureHeights(): void;
    calculateHeights(container?: HTMLElement): void;
    isActive(element: ElementRef): boolean;
}
