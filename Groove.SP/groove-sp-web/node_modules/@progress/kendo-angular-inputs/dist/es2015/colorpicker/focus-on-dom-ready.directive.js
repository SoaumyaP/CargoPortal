/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, ElementRef, NgZone } from '@angular/core';
/**
 * @hidden
 */
export class FocusOnDomReadyDirective {
    constructor(host, ngZone) {
        this.host = host;
        this.ngZone = ngZone;
    }
    ngAfterContentInit() {
        this.focusOnNextTick();
    }
    focusOnNextTick() {
        this.ngZone.runOutsideAngular(() => setTimeout(() => this.host.nativeElement.focus()));
    }
}
FocusOnDomReadyDirective.decorators = [
    { type: Directive, args: [{
                selector: '[kendoFocusOnDomReady]'
            },] },
];
/** @nocollapse */
FocusOnDomReadyDirective.ctorParameters = () => [
    { type: ElementRef },
    { type: NgZone }
];
