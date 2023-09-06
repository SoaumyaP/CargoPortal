/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, Input, Output, EventEmitter, ElementRef, HostBinding } from '@angular/core';
import { getRGBA, parseColor, getColorFromRGBA } from './utils';
import { isPresent } from '../common/utils';
import { isDocumentAvailable } from '@progress/kendo-angular-common';
/**
 * @hidden
 */
var ColorInputComponent = /** @class */ (function () {
    function ColorInputComponent(host) {
        this.host = host;
        /**
         * Sets whether the alpha slider will be shown.
         */
        this.opacity = true;
        /**
         * Sets the disabled state of the ColorInput.
         */
        this.disabled = false;
        /**
         * Sets the read-only state of the ColorInput.
         */
        this.readonly = false;
        /**
         * Emits a parsed rgba string color.
         */
        this.valueChange = new EventEmitter();
        this.colorInputClass = true;
        /**
         * The rgba inputs values.
         */
        this.rgba = {};
    }
    Object.defineProperty(ColorInputComponent.prototype, "isFocused", {
        /**
         * Indicates whether any of the inputs are focused.
         */
        get: function () {
            if (!(isDocumentAvailable() && isPresent(this.host))) {
                return false;
            }
            var activeElement = document.activeElement;
            return this.host.nativeElement.contains(activeElement);
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ColorInputComponent.prototype, "rgbaInputValid", {
        /**
         * Indicates whether any of the rgba inputs have value.
         */
        get: function () {
            var _this = this;
            return Object.keys(this.rgba).every(function (key) { return isPresent(_this.rgba[key]); });
        },
        enumerable: true,
        configurable: true
    });
    ColorInputComponent.prototype.ngOnChanges = function (changes) {
        if (isPresent(changes.value) && !this.isFocused) {
            this.hex = parseColor(this.value, 'hex');
            this.rgba = getRGBA(this.value);
            this.rgba.a = parseColor(this.value, 'rgba') ? this.rgba.a : 1;
        }
    };
    ColorInputComponent.prototype.handleRgbaValueChange = function () {
        var color = getColorFromRGBA(this.rgba);
        if (!this.rgbaInputValid || color === this.value) {
            return;
        }
        this.value = color;
        this.rgba = getRGBA(this.value);
        this.hex = parseColor(color, 'hex');
        this.valueChange.emit(color);
    };
    ColorInputComponent.prototype.handleHexValueChange = function (hex) {
        this.hex = hex;
        var color = parseColor(hex, 'rgba');
        if (!isPresent(color) || color === this.value) {
            return;
        }
        this.value = color;
        this.rgba = getRGBA(color);
        this.valueChange.emit(color);
    };
    ColorInputComponent.prototype.handleRgbaInputBlur = function () {
        if (!this.rgbaInputValid) {
            this.rgba = getRGBA(this.value);
        }
    };
    ColorInputComponent.prototype.handleHexInputBlur = function () {
        this.hex = parseColor(this.value, 'hex');
    };
    ColorInputComponent.decorators = [
        { type: Component, args: [{
                    selector: 'kendo-colorinput',
                    template: "\n        <div class=\"k-hbox k-gradient-values\">\n            <input\n                #hexInput\n                class=\"k-textbox k-hex-value\"\n                [disabled]=\"disabled\"\n                [readonly]=\"readonly\"\n                [value]=\"hex || ''\"\n                placeholder=\"no color\"\n                (blur)=\"handleHexInputBlur()\"\n                (input)=\"handleHexValueChange(hexInput.value)\"\n            />\n            <kendo-numerictextbox\n                [disabled]=\"disabled\"\n                [readonly]=\"readonly\"\n                [min]=\"0\"\n                [max]=\"255\"\n                placeholder=\"R\"\n                [(value)]=\"rgba.r\"\n                [autoCorrect]=\"true\"\n                [spinners]=\"false\"\n                [format]=\"'n'\"\n                [decimals]=\"0\"\n                (blur)=\"handleRgbaInputBlur()\"\n                (valueChange)=\"handleRgbaValueChange()\"\n            >\n            </kendo-numerictextbox>\n            <kendo-numerictextbox\n                [disabled]=\"disabled\"\n                [readonly]=\"readonly\"\n                [min]=\"0\"\n                [max]=\"255\"\n                placeholder=\"G\"\n                [(value)]=\"rgba.g\"\n                [autoCorrect]=\"true\"\n                [spinners]=\"false\"\n                [format]=\"'n'\"\n                [decimals]=\"0\"\n                (blur)=\"handleRgbaInputBlur()\"\n                (valueChange)=\"handleRgbaValueChange()\"\n            >\n            </kendo-numerictextbox>\n            <kendo-numerictextbox\n                [disabled]=\"disabled\"\n                [readonly]=\"readonly\"\n                [min]=\"0\"\n                [max]=\"255\"\n                placeholder=\"B\"\n                [(value)]=\"rgba.b\"\n                [autoCorrect]=\"true\"\n                [spinners]=\"false\"\n                [format]=\"'n'\"\n                [decimals]=\"0\"\n                (blur)=\"handleRgbaInputBlur()\"\n                (valueChange)=\"handleRgbaValueChange()\"\n            >\n            </kendo-numerictextbox>\n            <kendo-numerictextbox\n                *ngIf=\"opacity\"\n                [disabled]=\"disabled\"\n                [readonly]=\"readonly\"\n                [min]=\"0\"\n                [max]=\"1\"\n                placeholder=\"A\"\n                [(value)]=\"rgba.a\"\n                [autoCorrect]=\"true\"\n                [spinners]=\"false\"\n                [step]=\"0.01\"\n                [format]=\"'n2'\"\n                [decimals]=\"2\"\n                (blur)=\"handleRgbaInputBlur()\"\n                (valueChange)=\"handleRgbaValueChange()\"\n            >\n            </kendo-numerictextbox>\n        </div>\n        <div class=\"k-hbox k-gradient-values\">\n            <div class=\"k-hex-value\">hex</div>\n            <div>r</div>\n            <div>g</div>\n            <div>b</div>\n            <div *ngIf=\"opacity\">a</div>\n        </div>\n    "
                },] },
    ];
    /** @nocollapse */
    ColorInputComponent.ctorParameters = function () { return [
        { type: ElementRef }
    ]; };
    ColorInputComponent.propDecorators = {
        value: [{ type: Input }],
        opacity: [{ type: Input }],
        disabled: [{ type: Input }],
        readonly: [{ type: Input }],
        valueChange: [{ type: Output }],
        colorInputClass: [{ type: HostBinding, args: ['class.k-colorinputs',] }]
    };
    return ColorInputComponent;
}());
export { ColorInputComponent };
