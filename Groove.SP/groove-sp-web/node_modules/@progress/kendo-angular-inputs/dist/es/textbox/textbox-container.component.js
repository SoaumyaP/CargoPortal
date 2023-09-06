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
var isFunction = function (x) { return Object.prototype.toString.call(x) === '[object Function]'; };
var ɵ0 = isFunction;
/**
 * @hidden
 */
var TextBoxContainerComponent = /** @class */ (function () {
    function TextBoxContainerComponent(elementRef, renderer, changeDetectorRef, rtl) {
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
    Object.defineProperty(TextBoxContainerComponent.prototype, "hostClasses", {
        get: function () {
            return true;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxContainerComponent.prototype, "textareaElementClass", {
        get: function () {
            return !!this.textarea;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxContainerComponent.prototype, "focusedClass", {
        get: function () {
            return this.focused;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxContainerComponent.prototype, "invalidClass", {
        get: function () {
            return this.invalid;
        },
        enumerable: true,
        configurable: true
    });
    /**
     * @hidden
     */
    TextBoxContainerComponent.prototype.ngAfterContentInit = function () {
        var _this = this;
        if (!this.formControl && !this.kendoInput) {
            if (isDevMode()) {
                throw new Error("The TextBoxContainer requires a Kendo Input component" +
                    " or a forms-bound component to function properly.");
            }
            return;
        }
        // add focus/blur/valueChange handlers
        var control = new FloatingLabelInputAdapter(this.kendoInput || this.formControl.valueAccessor, this.formControl);
        var setFocus = function (isFocused) { return function () {
            _this.focused = isFocused;
            _this.updateState();
        }; };
        this.subscribe(control, 'onFocus', setFocus(true));
        this.subscribe(control, 'onBlur', setFocus(false));
        this.subscribe(control, 'autoFillStart', function () {
            _this.autoFillStarted = true;
            _this.renderer.removeClass(_this.elementRef.nativeElement, 'k-state-empty');
        });
        this.subscribe(control, 'autoFillEnd', function () {
            if (_this.autoFillStarted) {
                _this.autoFillStarted = false;
                if (_this.empty) {
                    _this.renderer.addClass(_this.elementRef.nativeElement, 'k-state-empty');
                }
            }
        });
        var updateState = function () { return _this.updateState(); };
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
            var id = "_" + guid();
            control.focusableId = id;
            this.id = id;
        }
    };
    /**
     * @hidden
     */
    TextBoxContainerComponent.prototype.ngOnDestroy = function () {
        this._subscriptions.forEach(function (s) { return s.unsubscribe(); });
        this._subscriptions = [];
    };
    TextBoxContainerComponent.prototype.subscribe = function (control, eventName, handler) {
        if (control[eventName] instanceof EventEmitter) {
            var subscription = control[eventName].subscribe(handler);
            this._subscriptions.push(subscription);
        }
    };
    TextBoxContainerComponent.prototype.updateState = function () {
        var empty = function (value) {
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
        var formControl = this.formControl;
        if (formControl) {
            var valueAccessor = formControl.valueAccessor;
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
    };
    TextBoxContainerComponent.decorators = [
        { type: Component, args: [{
                    selector: 'kendo-textbox-container',
                    template: "\n        <ng-content></ng-content>\n        <label *ngIf=\"floatingLabel\" [for]=\"id\" class=\"k-label\">{{ floatingLabel }}</label>\n    "
                },] },
    ];
    /** @nocollapse */
    TextBoxContainerComponent.ctorParameters = function () { return [
        { type: ElementRef },
        { type: Renderer2 },
        { type: ChangeDetectorRef },
        { type: Boolean, decorators: [{ type: Optional }, { type: Inject, args: [RTL,] }] }
    ]; };
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
    return TextBoxContainerComponent;
}());
export { TextBoxContainerComponent };
export { ɵ0 };
