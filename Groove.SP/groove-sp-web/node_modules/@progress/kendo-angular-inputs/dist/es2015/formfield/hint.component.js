/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, HostBinding, Input } from '@angular/core';
let serial = 0;
/**
 * Represents a hint message that will be shown underneath a form-bound component.
 */
export class HintComponent {
    constructor() {
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
        this.id = `kendo-hint-${serial++}`;
        this.hostClass = true;
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
HintComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-formhint',
                template: `
        <ng-content></ng-content>
    `
            },] },
];
HintComponent.propDecorators = {
    align: [{ type: Input }],
    hostClass: [{ type: HostBinding, args: ['class.k-form-hint',] }],
    startClass: [{ type: HostBinding, args: ['class.k-text-start',] }],
    endClass: [{ type: HostBinding, args: ['class.k-text-end',] }],
    idAttribute: [{ type: HostBinding, args: ['attr.id',] }]
};
