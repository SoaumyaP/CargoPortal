/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
/**
 * @hidden
 */
var ListItemDirective = /** @class */ (function () {
    function ListItemDirective(element) {
        this.element = element;
    }
    ListItemDirective.decorators = [
        { type: core_1.Directive, args: [{
                    selector: '"li[role=option], li[role=group]"' // tslint:disable-line
                },] },
    ];
    /** @nocollapse */
    ListItemDirective.ctorParameters = function () { return [
        { type: core_1.ElementRef }
    ]; };
    return ListItemDirective;
}());
exports.ListItemDirective = ListItemDirective;
