/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var serial = 0;
/**
 * Represents an error message that will be shown underneath
 * a Kendo control or native HTML form-bound component after a validation.
 */
var ErrorComponent = /** @class */ (function () {
    function ErrorComponent() {
        this.hostClass = true;
        /**
         * Specifies the alignment of the Error message.
         *
         * The possible values are:
         * * (Default) `start`
         * * `end`
         */
        this.align = 'start';
        /**
         * @hidden
         */
        this.id = "kendo-error-" + serial++;
        this.roleAttribute = 'alert';
    }
    Object.defineProperty(ErrorComponent.prototype, "startClass", {
        get: function () {
            return this.align === 'start';
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ErrorComponent.prototype, "endClass", {
        get: function () {
            return this.align === 'end';
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ErrorComponent.prototype, "idAttribute", {
        get: function () {
            return this.id;
        },
        enumerable: true,
        configurable: true
    });
    ErrorComponent.decorators = [
        { type: core_1.Component, args: [{
                    selector: 'kendo-formerror',
                    template: "\n        <ng-content></ng-content>\n    "
                },] },
    ];
    ErrorComponent.propDecorators = {
        hostClass: [{ type: core_1.HostBinding, args: ['class.k-form-error',] }],
        align: [{ type: core_1.Input }],
        roleAttribute: [{ type: core_1.HostBinding, args: ['attr.role',] }],
        startClass: [{ type: core_1.HostBinding, args: ['class.k-text-start',] }],
        endClass: [{ type: core_1.HostBinding, args: ['class.k-text-end',] }],
        idAttribute: [{ type: core_1.HostBinding, args: ['attr.id',] }]
    };
    return ErrorComponent;
}());
exports.ErrorComponent = ErrorComponent;
