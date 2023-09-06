/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, HostBinding, Input } from '@angular/core';
let serial = 0;
/**
 * Represents an error message that will be shown underneath
 * a Kendo control or native HTML form-bound component after a validation.
 */
export class ErrorComponent {
    constructor() {
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
        this.id = `kendo-error-${serial++}`;
        this.roleAttribute = 'alert';
    }
    get startClass() {
        return this.align === 'start';
    }
    get endClass() {
        return this.align === 'end';
    }
    get idAttribute() {
        return this.id;
    }
}
ErrorComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-formerror',
                template: `
        <ng-content></ng-content>
    `
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
