/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, ElementRef, EventEmitter, Input, HostBinding, Renderer2, forwardRef, NgZone } from '@angular/core';
import { KendoInput } from '@progress/kendo-angular-common';
/**
 * Represents the [Kendo UI TextBox directive]({% slug overview_textbox %}) for the Inputs components for Angular.
 * Used to style the textbox of any `input` element.
 *
 * @example
 * ```ts-no-run
 * <input kendoTextBox />
 * <input kendoTextBox type="email" />
 * <input kendoTextBox type="password" />
 * ```
 */
export class TextBoxDirective {
    constructor(renderer, inputElement, ngZone) {
        this.renderer = renderer;
        this.inputElement = inputElement;
        this.ngZone = ngZone;
        this.hostClass = true;
        /**
         * @hidden
         */
        this.onFocus = new EventEmitter();
        /**
         * @hidden
         */
        this.onBlur = new EventEmitter();
        /**
         * @hidden
         */
        this.onValueChange = new EventEmitter();
        /**
         * @hidden
         */
        this.autoFillStart = new EventEmitter();
        /**
         * @hidden
         */
        this.autoFillEnd = new EventEmitter();
        this.listeners = [];
    }
    /**
     * @hidden
     */
    set value(text) {
        if (!this.inputElement) {
            return;
        }
        this.inputElement.nativeElement.value = (text === undefined || text === null) ? '' : text;
        this.onValueChange.emit();
    }
    /**
     * @hidden
     */
    get value() {
        return this.inputElement.nativeElement.value;
    }
    get id() {
        return this.inputElement.nativeElement.id;
    }
    set id(id) {
        this.renderer.setAttribute(this.inputElement.nativeElement, 'id', id);
    }
    ngAfterViewInit() {
        const input = this.inputElement.nativeElement;
        this.listeners = [
            this.renderer.listen(input, 'focus', () => this.onFocus.emit()),
            this.renderer.listen(input, 'blur', () => this.onBlur.emit())
        ];
        this.ngZone.runOutsideAngular(() => {
            this.renderer.listen(input, 'animationstart', (e) => {
                if (e.animationName === 'autoFillStart') {
                    this.autoFillStart.emit();
                }
                else if (e.animationName === 'autoFillEnd') {
                    this.autoFillEnd.emit();
                }
            });
        });
    }
    ngOnDestroy() {
        this.listeners.forEach(listener => listener());
    }
}
TextBoxDirective.decorators = [
    { type: Directive, args: [{
                selector: 'input[kendoTextBox]',
                providers: [{
                        provide: KendoInput,
                        useExisting: forwardRef(() => TextBoxDirective)
                    }]
            },] },
];
/** @nocollapse */
TextBoxDirective.ctorParameters = () => [
    { type: Renderer2 },
    { type: ElementRef },
    { type: NgZone }
];
TextBoxDirective.propDecorators = {
    hostClass: [{ type: HostBinding, args: ['class.k-textbox',] }],
    value: [{ type: Input }]
};
