/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, ElementRef, Input, NgZone } from '@angular/core';
/**
 * @hidden
 */
export class FilterInputDirective {
    constructor(element, zone) {
        this.element = element;
        this.zone = zone;
    }
    ngOnChanges() {
        if (this.focused) {
            this.nextTick(() => this.element.nativeElement.focus());
        }
    }
    nextTick(fn) {
        this.zone.runOutsideAngular(() => setTimeout(fn));
    }
}
FilterInputDirective.decorators = [
    { type: Directive, args: [{
                selector: '[filterInput]' // tslint:disable-line
            },] },
];
/** @nocollapse */
FilterInputDirective.ctorParameters = () => [
    { type: ElementRef },
    { type: NgZone }
];
FilterInputDirective.propDecorators = {
    focused: [{ type: Input, args: ['filterInput',] }]
};
