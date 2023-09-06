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
export class SliderBase {
    constructor(localizationService, injector, renderer, ngZone, changeDetector, hostElement) {
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
        this.ifEnabled = (callback, event) => {
            if (!this.isDisabled) {
                callback.call(this, event);
            }
        };
        this.direction = localizationService.rtl ? 'rtl' : 'ltr';
    }
    get horizontalClass() {
        return !this.vertical;
    }
    get verticalClass() {
        return this.vertical;
    }
    get disabledClass() {
        return this.disabled;
    }
    /**
     * @hidden
     * Called when the status of the component changes to or from `disabled`.
     * Depending on the value, it enables or disables the appropriate DOM element.
     *
     * @param isDisabled
     */
    setDisabledState(isDisabled) {
        this.disabled = isDisabled;
    }
    ngOnInit() {
        this.subscriptions.add(this.localizationService
            .changes
            .subscribe(({ rtl }) => {
            this.direction = rtl ? 'rtl' : 'ltr';
            this.sizeComponent();
        }));
        if (this.hostElement) {
            this.renderer.removeAttribute(this.hostElement.nativeElement, "tabindex");
        }
        this.control = this.injector.get(NgControl, null);
    }
    /**
     * @hidden
     */
    get isDisabled() {
        return this.disabled || this.readonly;
    }
    /**
     * @hidden
     * Used by the FloatingLabel to determine if the component is empty.
     */
    isEmpty() {
        return false;
    }
    get reverse() {
        return this.localizationService.rtl && !this.vertical;
    }
    get keyBinding() {
        const reverse = this.reverse;
        return {
            [Keys.ArrowLeft]: reverse ? increment : decrement,
            [Keys.ArrowRight]: reverse ? decrement : increment,
            [Keys.ArrowDown]: decrement,
            [Keys.ArrowUp]: increment,
            [Keys.PageUp]: incrementLarge,
            [Keys.PageDown]: decrementLarge,
            [Keys.Home]: ({ min }) => min,
            [Keys.End]: ({ max }) => max
        };
    }
    resetStyles(elements) {
        elements.forEach(el => {
            if (el) {
                if (this.vertical) {
                    this.renderer.removeStyle(el, 'width');
                    this.renderer.removeStyle(el, 'left');
                    this.renderer.removeStyle(el, 'right');
                }
                else {
                    this.renderer.removeStyle(el, 'height');
                    this.renderer.removeStyle(el, 'bottom');
                }
                this.renderer.removeStyle(el, 'padding-top');
            }
        });
    }
}
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
