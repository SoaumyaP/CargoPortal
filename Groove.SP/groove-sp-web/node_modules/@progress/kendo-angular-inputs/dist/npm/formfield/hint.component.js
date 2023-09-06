/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var serial = 0;
/**
 * Represents a hint message that will be shown underneath a form-bound component.
 */
var HintComponent = /** @class */ (function () {
    function HintComponent() {
        /**
         * Specifies the alignment of the Hint message.
         *
         * The possible values are:
         * * (Default) `start`
         * * `end`
         */
        this.align = 'start';
        /**
         * @hidden
         */
        this.id = "kendo-hint-" + serial++;
        this.hostClass = true;
    }
    Object.defineProperty(HintComponent.prototype, "startClass", {
        get: function () {
            return this.align === 'start';
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(HintComponent.prototype, "endClass", {
        get: function () {
            return this.align === 'end';
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(HintComponent.prototype, "idAttribute", {
        get: function () {
            return this.id;
        },
        enumerable: true,
        configurable: true
    });
    HintComponent.decorators = [
        { type: core_1.Component, args: [{
                    selector: 'kendo-formhint',
                    template: "\n        <ng-content></ng-content>\n    "
                },] },
    ];
    HintComponent.propDecorators = {
        align: [{ type: core_1.Input }],
        hostClass: [{ type: core_1.HostBinding, args: ['class.k-form-hint',] }],
        startClass: [{ type: core_1.HostBinding, args: ['class.k-text-start',] }],
        endClass: [{ type: core_1.HostBinding, args: ['class.k-text-end',] }],
        idAttribute: [{ type: core_1.HostBinding, args: ['attr.id',] }]
    };
    return HintComponent;
}());
exports.HintComponent = HintComponent;
