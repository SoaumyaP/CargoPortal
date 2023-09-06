/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ElementRef, NgZone, AfterContentInit } from '@angular/core';
/**
 * @hidden
 */
export declare class FocusOnDomReadyDirective implements AfterContentInit {
    private host;
    private ngZone;
    constructor(host: ElementRef, ngZone: NgZone);
    ngAfterContentInit(): void;
    focusOnNextTick(): void;
}
