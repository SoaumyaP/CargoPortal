/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ContentChild, Component, ElementRef, EventEmitter, HostBinding, Input, Inject, Optional, Renderer2, isDevMode, ChangeDetectorRef } from '@angular/core';
import { RTL } from '@progress/kendo-angular-l10n';
import { TextAreaDirective } from './textarea.directive';
import { NgControl } from '@angular/forms';
import { guid, KendoInput } from '@progress/kendo-angular-common';
import { FloatingLabelInputAdapter } from './floating-label-input-adapter';
const isFunction = (x) => Object.prototype.toString.call(x) === '[object Function]';
const ɵ0 = isFunction;
/**
 * @hidden
 */
export class TextBoxContainerComponent {
    constructor(elementRef, renderer, changeDetectorRef, rtl) {
        this.elementRef = elementRef;
        this.renderer = renderer;
        this.changeDetectorRef = changeDetectorRef;
        /**
         * @hidden
         */
        this.focused = false;
        /**
         * @hidden
         */
        this.empty = true;
        /**
         * @hidden
         */
        this.invalid = false;
        this._subscriptions = [];
        this.autoFillStarted = false;
        this.direction = rtl ? 'rtl' : 'ltr';
        this.renderer.removeAttribute(this.elementRef.nativeElement, "id");
    }
    get hostClasses() {
        return true;
    }
    get textareaElementClass() {
        return !!this.textarea;
    }
    get focusedClass() {
        return this.focused;
    }
    get invalidClass() {
        return this.invalid;
    }
    /**
     * @hidden
     */
    ngAfterContentInit() {
        if (!this.formControl && !this.kendoInput) {
            if (isDevMode()) {
                throw new Error("The TextBoxContainer requires a Kendo Input component" +
                    " or a forms-bound component to function properly.");
            }
            return;
        }
        // add focus/blur/valueChange handlers
        const control = new FloatingLabelInputAdapter(this.kendoInput || this.formControl.valueAccessor, this.formControl);
        const setFocus = (isFocused) => () => {
            this.focused = isFocused;
            this.updateState();
        };
        this.subscribe(control, 'onFocus', setFocus(true));
        this.subscribe(control, 'onBlur', setFocus(false));
        this.subscribe(control, 'autoFillStart', () => {
            this.autoFillStarted = true;
            this.renderer.removeClass(this.elementRef.nativeElement, 'k-state-empty');
        });
        this.subscribe(control, 'autoFillEnd', () => {
            if (this.autoFillStarted) {
                this.autoFillStarted = false;
                if (this.empty) {
                    this.renderer.addClass(this.elementRef.nativeElement, 'k-state-empty');
                }
            }
        });
        const updateState = () => this.updateState();
        updateState();
        this.subscribe(control, 'onValueChange', updateState);
        // set label id for floating label
        if (this.id && control.focusableId) {
            // input wins
            this.id = control.focusableId;
        }
        else if (this.id) {
            control.focusableId = this.id;
        }
        else if (control.focusableId) {
            this.id = control.focusableId;
        }
        else {
            const id = "_" + guid();
            control.focusableId = id;
            this.id = id;
        }
    }
    /**
     * @hidden
     */
    ngOnDestroy() {
        this._subscriptions.forEach(s => s.unsubscribe());
        this._subscriptions = [];
    }
    subscribe(control, eventName, handler) {
        if (control[eventName] instanceof EventEmitter) {
            const subscription = control[eventName].subscribe(handler);
            this._subscriptions.push(subscription);
        }
    }
    updateState() {
        const empty = value => {
            // zero is not an empty value (e.g., NumericTextBox)
            if (value === 0 || value === false) {
                return false;
            }
            // empty arrays are an empty value (e.g., MultiSelect)
            if (Array.isArray(value) && !value.length) {
                return true;
            }
            return !value;
        };
        const formControl = this.formControl;
        if (formControl) {
            const valueAccessor = formControl.valueAccessor;
            if (isFunction(valueAccessor.isEmpty)) {
                this.empty = valueAccessor.isEmpty();
            }
            else {
                this.empty = empty(formControl.value);
            }
            this.invalid = formControl.invalid && (formControl.touched || formControl.dirty);
        }
        else {
            this.empty = isFunction(this.kendoInput.isEmpty) ?
                this.kendoInput.isEmpty() : empty(this.kendoInput.value);
        }
        if (this.empty) {
            this.renderer.addClass(this.elementRef.nativeElement, 'k-state-empty');
        }
        else {
            this.renderer.removeClass(this.elementRef.nativeElement, 'k-state-empty');
        }
        this.changeDetectorRef.markForCheck();
    }
}
TextBoxContainerComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-textbox-container',
                template: `
        <ng-content></ng-content>
        <label *ngIf="floatingLabel" [for]="id" class="k-label">{{ floatingLabel }}</label>
    `
            },] },
];
/** @nocollapse */
TextBoxContainerComponent.ctorParameters = () => [
    { type: ElementRef },
    { type: Renderer2 },
    { type: ChangeDetectorRef },
    { type: Boolean, decorators: [{ type: Optional }, { type: Inject, args: [RTL,] }] }
];
TextBoxContainerComponent.propDecorators = {
    hostClasses: [{ type: HostBinding, args: ['class.k-textbox-container',] }],
    textareaElementClass: [{ type: HostBinding, args: ['class.k-textarea-wrapper',] }],
    focusedClass: [{ type: HostBinding, args: ['class.k-state-focused',] }],
    invalidClass: [{ type: HostBinding, args: ['class.k-state-invalid',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    id: [{ type: Input }],
    floatingLabel: [{ type: Input }],
    kendoInput: [{ type: ContentChild, args: [KendoInput,] }],
    textarea: [{ type: ContentChild, args: [TextAreaDirective,] }],
    formControl: [{ type: ContentChild, args: [NgControl,] }]
};
export { ɵ0 };
