/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, HostBinding, Input } from '@angular/core';
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
        { type: Component, args: [{
                    selector: 'kendo-formerror',
                    template: "\n        <ng-content></ng-content>\n    "
                },] },
    ];
    ErrorComponent.propDecorators = {
        hostClass: [{ type: HostBinding, args: ['class.k-form-error',] }],
        align: [{ type: Input }],
        roleAttribute: [{ type: HostBinding, args: ['attr.role',] }],
        startClass: [{ type: HostBinding, args: ['class.k-text-start',] }],
        endClass: [{ type: HostBinding, args: ['class.k-text-end',] }],
        idAttribute: [{ type: HostBinding, args: ['attr.id',] }]
    };
    return ErrorComponent;
}());
export { ErrorComponent };
