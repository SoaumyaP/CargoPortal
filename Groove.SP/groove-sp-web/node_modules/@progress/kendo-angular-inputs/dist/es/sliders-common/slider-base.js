/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { increment, decrement, incrementLarge, decrementLarge, identity } from './sliders-util';
import { Input, Output, EventEmitter, HostBinding, ElementRef, ViewChild, ContentChild } from '@angular/core';
import { Subscription } from 'rxjs';
import { NgControl } from '@angular/forms';
import { Keys } from '@progress/kendo-angular-common';
import { LabelTemplateDirective } from './label-template.directive';
/**
 * @hidden
 */
var SliderBase = /** @class */ (function () {
    function SliderBase(localizationService, injector, renderer, ngZone, changeDetector, hostElement) {
        var _this = this;
        this.localizationService = localizationService;
        this.injector = injector;
        this.renderer = renderer;
        this.ngZone = ngZone;
        this.changeDetector = changeDetector;
        this.hostElement = hostElement;
        /**
         * Defines the title of the ticks ([see example]({% slug ticks_slider %}#toc-titles)). The default title
         * for each tick is its Slider value. If you use a callback function, the function accepts an argument
         * that holds the value of the component and returns a string with the new title.
         */
        this.title = identity;
        /**
         * Denotes the location of the tick marks in the Slider ([see example]({% slug ticks_slider %}#toc-placement)).
         *
         * The available options are:
         * * `before`&mdash;The tick marks are located to the top side of the horizontal track or to the left side of a vertical track.
         * * `after`&mdash;The tick marks are located to the bottom side of the horizontal track or to the right side of the vertical track.
         * * `both`&mdash; (Default) The tick marks are located on both sides of the track.
         * * `none`&mdash;The tick marks are not visible. The actual elements are not added to the DOM tree.
         */
        this.tickPlacement = 'both';
        /**
         * If `vertical` is set to `true`, the orientation of the Slider changes from horizontal to vertical
         * ([see example]({% slug orientation_slider %})).
         */
        this.vertical = false;
        /**
         * The minimum value of the Slider ([see example]({% slug predefinedsteps_slider %}#toc-small-steps)).
         * The attribute accepts both integers and floating-point numbers.
         */
        this.min = 0;
        /**
         * The maximum value of the Slider ([see example]({% slug predefinedsteps_slider %}#toc-small-steps)).
         * The attribute accepts both integers and floating-point numbers.
         */
        this.max = 10;
        /**
         * The step value of the Slider ([see example]({% slug predefinedsteps_slider %}#toc-small-steps)).
         * Accepts positive values only. Can be an integer or a floating-point number.
         */
        this.smallStep = 1;
        /**
         * Specifies that every n<sup>th</sup> tick will be large and will have a label
         * ([see example]({% slug predefinedsteps_slider %}#toc-large-steps)).
         * Accepts positive integer values only.
         */
        this.largeStep = null;
        /**
         * Determines whether the Slider is disabled ([see example]({% slug disabledstate_slider %})).
         */
        this.disabled = false;
        /**
         * Determines whether the Slider is in its read-only state ([see example]({% slug readonly_slider %})).
         */
        this.readonly = false;
        /**
         * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the Slider.
         */
        this.tabindex = 0;
        /**
         * Fires each time the user focuses the component.
         */
        this.onFocus = new EventEmitter(); //tslint:disable-line:no-output-rename
        /**
         * Fires each time the component is blurred.
         */
        this.onBlur = new EventEmitter(); //tslint:disable-line:no-output-rename
        /**
         * Fires each time the user selects a new value.
         */
        this.valueChange = new EventEmitter();
        this.sliderClass = true;
        this.widgetClass = true;
        this.subscriptions = new Subscription();
        /**
         * @hidden
         */
        this.ifEnabled = function (callback, event) {
            if (!_this.isDisabled) {
                callback.call(_this, event);
            }
        };
        this.direction = localizationService.rtl ? 'rtl' : 'ltr';
    }
    Object.defineProperty(SliderBase.prototype, "horizontalClass", {
        get: function () {
            return !this.vertical;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(SliderBase.prototype, "verticalClass", {
        get: function () {
            return this.vertical;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(SliderBase.prototype, "disabledClass", {
        get: function () {
            return this.disabled;
        },
        enumerable: true,
        configurable: true
    });
    /**
     * @hidden
     * Called when the status of the component changes to or from `disabled`.
     * Depending on the value, it enables or disables the appropriate DOM element.
     *
     * @param isDisabled
     */
    SliderBase.prototype.setDisabledState = function (isDisabled) {
        this.disabled = isDisabled;
    };
    SliderBase.prototype.ngOnInit = function () {
        var _this = this;
        this.subscriptions.add(this.localizationService
            .changes
            .subscribe(function (_a) {
            var rtl = _a.rtl;
            _this.direction = rtl ? 'rtl' : 'ltr';
            _this.sizeComponent();
        }));
        if (this.hostElement) {
            this.renderer.removeAttribute(this.hostElement.nativeElement, "tabindex");
        }
        this.control = this.injector.get(NgControl, null);
    };
    Object.defineProperty(SliderBase.prototype, "isDisabled", {
        /**
         * @hidden
         */
        get: function () {
            return this.disabled || this.readonly;
        },
        enumerable: true,
        configurable: true
    });
    /**
     * @hidden
     * Used by the FloatingLabel to determine if the component is empty.
     */
    SliderBase.prototype.isEmpty = function () {
        return false;
    };
    Object.defineProperty(SliderBase.prototype, "reverse", {
        get: function () {
            return this.localizationService.rtl && !this.vertical;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(SliderBase.prototype, "keyBinding", {
        get: function () {
            var _a;
            var reverse = this.reverse;
            return _a = {},
                _a[Keys.ArrowLeft] = reverse ? increment : decrement,
                _a[Keys.ArrowRight] = reverse ? decrement : increment,
                _a[Keys.ArrowDown] = decrement,
                _a[Keys.ArrowUp] = increment,
                _a[Keys.PageUp] = incrementLarge,
                _a[Keys.PageDown] = decrementLarge,
                _a[Keys.Home] = function (_a) {
                    var min = _a.min;
                    return min;
                },
                _a[Keys.End] = function (_a) {
                    var max = _a.max;
                    return max;
                },
                _a;
        },
        enumerable: true,
        configurable: true
    });
    SliderBase.prototype.resetStyles = function (elements) {
        var _this = this;
        elements.forEach(function (el) {
            if (el) {
                if (_this.vertical) {
                    _this.renderer.removeStyle(el, 'width');
                    _this.renderer.removeStyle(el, 'left');
                    _this.renderer.removeStyle(el, 'right');
                }
                else {
                    _this.renderer.removeStyle(el, 'height');
                    _this.renderer.removeStyle(el, 'bottom');
                }
                _this.renderer.removeStyle(el, 'padding-top');
            }
        });
    };
    SliderBase.propDecorators = {
        title: [{ type: Input }],
        tickPlacement: [{ type: Input }],
        vertical: [{ type: Input }],
        min: [{ type: Input }],
        max: [{ type: Input }],
        smallStep: [{ type: Input }],
        largeStep: [{ type: Input }],
        fixedTickWidth: [{ type: Input }],
        disabled: [{ type: Input }],
        readonly: [{ type: Input }],
        tabindex: [{ type: Input }],
        onFocus: [{ type: Output, args: ['focus',] }],
        onBlur: [{ type: Output, args: ['blur',] }],
        valueChange: [{ type: Output }],
        direction: [{ type: HostBinding, args: ['attr.dir',] }],
        horizontalClass: [{ type: HostBinding, args: ['class.k-slider-horizontal',] }],
        verticalClass: [{ type: HostBinding, args: ['class.k-slider-vertical',] }],
        sliderClass: [{ type: HostBinding, args: ['class.k-slider',] }],
        widgetClass: [{ type: HostBinding, args: ['class.k-widget',] }],
        disabledClass: [{ type: HostBinding, args: ['class.k-state-disabled',] }],
        wrapper: [{ type: ViewChild, args: ['wrap', { static: true },] }],
        track: [{ type: ViewChild, args: ['track', { static: true },] }],
        sliderSelection: [{ type: ViewChild, args: ['sliderSelection', { static: true },] }],
        ticksContainer: [{ type: ViewChild, args: ['ticks', { read: ElementRef },] }],
        ticks: [{ type: ViewChild, args: ['ticks',] }],
        labelTemplate: [{ type: ContentChild, args: [LabelTemplateDirective,] }]
    };
    return SliderBase;
}());
export { SliderBase };
