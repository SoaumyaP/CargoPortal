/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ElementRef, NgZone, OnChanges } from '@angular/core';
/**
 * @hidden
 */
export declare class FilterInputDirective implements OnChanges {
    private element;
    private zone;
    focused: boolean;
    constructor(element: ElementRef, zone: NgZone);
    ngOnChanges(): void;
    private nextTick;
}
