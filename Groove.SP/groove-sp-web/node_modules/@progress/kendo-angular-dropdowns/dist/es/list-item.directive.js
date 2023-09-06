/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, ElementRef } from '@angular/core';
/**
 * @hidden
 */
var ListItemDirective = /** @class */ (function () {
    function ListItemDirective(element) {
        this.element = element;
    }
    ListItemDirective.decorators = [
        { type: Directive, args: [{
                    selector: '"li[role=option], li[role=group]"' // tslint:disable-line
                },] },
    ];
    /** @nocollapse */
    ListItemDirective.ctorParameters = function () { return [
        { type: ElementRef }
    ]; };
    return ListItemDirective;
}());
export { ListItemDirective };
