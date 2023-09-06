/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { isDevMode, Directive, TemplateRef, Optional, EventEmitter, Input, Output, HostBinding, ViewChild, ElementRef, ContentChild, Component, forwardRef, Injector, Renderer2, NgZone, ChangeDetectorRef, Inject, Injectable, HostListener, ViewChildren, NgModule, ViewContainerRef, ContentChildren } from '@angular/core';
import { NgControl, NG_VALUE_ACCESSOR, NG_VALIDATORS, RadioControlValueAccessor } from '@angular/forms';
import { Subscription, fromEvent, interval, merge, Observable } from 'rxjs';
import { take, filter, concatMap, startWith, takeUntil, skip, debounceTime } from 'rxjs/operators';
import { LocalizationService, L10N_PREFIX, RTL, ComponentMessages } from '@progress/kendo-angular-l10n';
import { Keys, guid, hasObservers, anyChanged, isDocumentAvailable, KendoInput, isChanged, DraggableModule, EventsModule, ResizeSensorModule } from '@progress/kendo-angular-common';
import { browser, mobileOS } from '@progress/kendo-common';
import { IntlService } from '@progress/kendo-angular-intl';
import { CommonModule } from '@angular/common';
import { PopupService, PopupModule } from '@progress/kendo-angular-popup';
import { Color, parseColor, namedColors } from '@progress/kendo-drawing';

/**
 * @hidden
 *
 * Checks if the value is `null` or `undefined`. Falsy values like '', 0, false, NaN, etc. are regarded as present.
 */
const isPresent = (value) => value !== null && value !== undefined;
/**
 * @hidden
 */
const areSame = (value1, value2) => value1 === value2 || (value1 === null && value2 === undefined) || (value1 === undefined && value2 === null);
/**
 * @hidden
 */
const requiresZoneOnBlur = (ngControl) => ngControl &&
    (!ngControl.touched || (ngControl.control && ngControl.control.updateOn === 'blur'));
/**
 * @hidden
 *
 * Fits the contender number into the specified bounds. If the number is NaN or null, the min is returned.
 *
 * @param contender Represents the number you want to fit into specified bounds.
 * @param min The inclusive lower bound number.
 * @param max The inclusive upper bound number.
 */
const fitIntoBounds = (contender, min, max) => {
    if (!isPresent(contender) || isNaN(contender)) {
        return min;
    }
    return contender <= min ? min : contender >= max ? max : contender;
};

/**
 * @hidden
 */
const MAX_PRECISION = 20;
/**
 * @hidden
 */
const limitPrecision = (precision) => Math.min(precision, MAX_PRECISION);
/**
 * @hidden
 */
const fractionLength = (value) => {
    return (String(value).split('.')[1] || "").length;
};
const maxFractionLength = (value1, value2) => {
    return Math.max(fractionLength(value1), fractionLength(value2));
};
/**
 * @hidden
 */
const toFixedPrecision = (value, precision) => {
    const maxPrecision = limitPrecision(precision);
    return parseFloat(value.toFixed(maxPrecision));
};
/**
 * @hidden
 */
const add = (value1, value2) => {
    const maxPrecision = maxFractionLength(value1, value2);
    return toFixedPrecision(value1 + value2, maxPrecision);
};
/**
 * @hidden
 */
const subtract = (value1, value2) => {
    return add(value1, -value2);
};
/**
 * @hidden
 */
const multiply = (value1, value2) => {
    const maxPrecision = fractionLength(value1) + fractionLength(value2);
    return toFixedPrecision(value1 * value2, maxPrecision);
};
/**
 * @hidden
 */
const divide = (dividend, divisor) => {
    if (divisor === 0) {
        return NaN;
    }
    const power = maxFractionLength(dividend, divisor);
    const correctionValue = Math.pow(10, power);
    return ((correctionValue * dividend) / (correctionValue * divisor));
};
/**
 * @hidden
 */
const remainder = (dividend, divisor) => {
    return Math.abs(subtract(dividend, multiply(divisor, Math.floor(divide(dividend, divisor)))));
};

/**
 * @hidden
 */
const calculateFixedTrackSize = ({ max, min, smallStep, fixedTickWidth }) => ((max - min) / smallStep) * fixedTickWidth;
/**
 * @hidden
 */
const calculateTrackSize = (wrapperWidth, offset, showButtons = true) => {
    const BUTTONS_COUNT = 2;
    const trackOffset = showButtons ? parseFloat(offset) * BUTTONS_COUNT : 0;
    const trackWidth = wrapperWidth - trackOffset;
    return Math.floor(trackWidth);
};
/**
 * @hidden
 */
const calculateTicksCount = (min = 0, max = 0, smallStep = 1) => {
    if (smallStep <= 0) {
        throw new Error('Invalid argument: smallStep must be a positive number');
    }
    const adjustedRange = Math.abs(subtract(max, min));
    const adjustedRatio = Math.floor(divide(adjustedRange, smallStep));
    const result = add(adjustedRatio, 1);
    return result;
};
/**
 * @hidden
 */
const calculateValueFromTick = (index, { max, min, smallStep, reverse, vertical }) => {
    const value = add(min, multiply(index, smallStep));
    return vertical || reverse ? Math.abs(subtract(value, max)) : value;
};
/**
 * @hidden
 */
const calculateHandlePosition = ({ handleWidth, trackWidth, min, max, reverse, value }) => {
    const halfHandleWidth = Math.floor(handleWidth / 2);
    const step = trackWidth / Math.abs(max - min);
    let pos = isPresent(value) ? step * (value - min) : min;
    if (reverse) {
        pos = trackWidth - pos;
    }
    return Math.floor(pos - halfHandleWidth);
};
/**
 * @hidden
 */
const decreaseValueToStep = (value, { max, min, smallStep, largeStep }, large = false) => {
    const step = large && largeStep ? multiply(smallStep, largeStep) : smallStep;
    const stepValue = subtract(value, min);
    let result;
    const stepRemainder = remainder(stepValue, step);
    if (stepRemainder === 0) {
        result = subtract(stepValue, step);
    }
    else {
        result = subtract(stepValue, stepRemainder);
    }
    return limitValue(add(result, min), min, max);
};
/**
 * @hidden
 */
const increaseValueToStep = (value, { max, min, smallStep, largeStep }, large = false) => {
    const step = large && largeStep ? multiply(smallStep, largeStep) : smallStep;
    const stepValue = subtract(value, min);
    const stepRemainder = remainder(stepValue, step);
    const result = add(subtract(stepValue, stepRemainder), step);
    return limitValue(add(result, min), min, max);
};
/**
 * @hidden
 */
const isStartHandle = (dragHandle) => dragHandle.id.indexOf('k-start-handle') > -1;
/**
 * @hidden
 */
const snapValue = (value, options) => {
    const { smallStep, min, max } = options;
    const limited = limitValue(value, min, max);
    if (value !== limited) {
        return limited;
    }
    const left = decreaseValueToStep(value, options);
    const right = increaseValueToStep(value, options);
    if ((value - min) % smallStep === 0) {
        return value;
    }
    if (right - value <= (right - left) / 2) {
        return right;
    }
    return left;
};
/**
 * @hidden
 */
const trimValue = (max, min, value) => {
    if (value > max) {
        return max;
    }
    if (value < min) {
        return min;
    }
    return value;
};
/**
 * @hidden
 */
const trimValueRange = (max, min, value) => {
    return value ? [trimValue(max, min, value[0]), trimValue(max, min, value[1])] : [min, min];
};
/**
 * @hidden
 */
const identity = (value) => value;
/**
 * @hidden
 */
const isSameRange = (value1, value2) => areSame(value1[0], value2[0]) && areSame(value1[1], value2[1]);
/**
 * @hidden
 */
const elementOffset = (element) => {
    const box = element.getBoundingClientRect();
    const documentElement = document.documentElement;
    return {
        left: box.left + (window.pageXOffset || documentElement.scrollLeft) - (documentElement.clientLeft || 0),
        top: box.top + (window.pageYOffset || documentElement.scrollTop) - (documentElement.clientTop || 0)
    };
};
/**
 * @hidden
 */
const limitValue = (value, min, max) => {
    return Math.max(Math.min(value, max), min);
};
/**
 * @hidden
 */
const eventValue = (eventArgs, scaleElement, options) => {
    const { min, max, vertical, rtl } = options;
    const trackOffset = elementOffset(scaleElement);
    const offset = vertical ? eventArgs.pageY - trackOffset.top : eventArgs.pageX - trackOffset.left;
    const scale = (max - min) / (vertical ? scaleElement.clientHeight : scaleElement.clientWidth);
    const offsetValue = offset * scale;
    let value = rtl || vertical ? max - offsetValue : min + offsetValue;
    const stepFractionLength = fractionLength(options.smallStep);
    value = toFixedPrecision(value, stepFractionLength + 1);
    return snapValue(value, options);
};
/**
 * @hidden
 */
const isButton = (element) => {
    return element.className.indexOf('k-button-increase') >= 0 || element.className.indexOf('k-button-decrease') >= 0;
};
/**
 * @hidden
 */
const increment = (options) => {
    return increaseValueToStep(options.value, options);
};
/**
 * @hidden
 */
const decrement = (options) => {
    return decreaseValueToStep(options.value, options);
};
/**
 * @hidden
 */
const incrementLarge = (options) => {
    return increaseValueToStep(options.value, options, true);
};
/**
 * @hidden
 */
const decrementLarge = (options) => {
    return decreaseValueToStep(options.value, options, true);
};
/**
 * @hidden
 */
const validateValue = (value) => {
    if (isDevMode && value && value[0] > value[1]) {
        throw new Error('[RangeSlider] The start value should not be greater than the end value.');
    }
};

/**
 * @hidden
 */
class SliderModelBase {
    constructor(props, wrapper, track, renderer) {
        this.props = props;
        this.wrapper = wrapper;
        this.track = track;
        this.renderer = renderer;
        this.props = props;
        this.wrapper = wrapper;
        this.track = track;
        this.tickSizes = this.getTickSizes();
    }
    resizeTrack() {
        const orientation = this.props.vertical ? 'height' : 'width';
        const trackWidth = this.trackWidth();
        this.track.style[orientation] = `${trackWidth}px`;
    }
    resizeTicks(ticksContainer, ticks) {
        const dimension = this.props.vertical ? "height" : "width";
        [...ticks].map((tick, index) => tick.style[dimension] = `${this.tickSizes[index]}px`);
        if (this.props.vertical) {
            this.adjustPadding(ticksContainer);
        }
    }
    resizeWrapper() {
        const dimension = this.props.vertical ? "height" : "width";
        const wrapperSize = this.elementSize(this.wrapper);
        const trackWidth = calculateTrackSize(wrapperSize, this.elementOffset(this.track));
        const fixedTrackWidth = calculateFixedTrackSize(this.props);
        const wrapperParentEl = this.wrapper.parentElement;
        if (trackWidth > fixedTrackWidth) {
            wrapperParentEl.style[dimension] = `${wrapperSize - (trackWidth - fixedTrackWidth)}px`;
        }
        else {
            wrapperParentEl.style[dimension] = `${wrapperSize + (fixedTrackWidth - trackWidth)}px`;
        }
    }
    trackWidth() {
        if (this.props.fixedTickWidth) {
            return calculateFixedTrackSize(this.props);
        }
        return calculateTrackSize(this.elementSize(this.wrapper), this.elementOffset(this.track), this.props.buttons);
    }
    getTickSizes() {
        const { min, max, smallStep } = this.props;
        const count = calculateTicksCount(min, max, smallStep);
        const trackSize = this.trackWidth();
        const distStep = trackSize / subtract(max, min);
        const result = [];
        let usedSpace = 0;
        let endPoint = 0;
        for (let i = 0; i < count; i++) {
            if (i === 0 || i === count - 1) {
                endPoint += (smallStep / 2) * distStep;
            }
            else {
                endPoint += smallStep * distStep;
            }
            // ensure that the sum of the tick sizes does not exceed the track width
            endPoint = +endPoint.toFixed(2) - 0.01;
            const size = Math.round(endPoint - usedSpace);
            result.push(size);
            usedSpace += size;
        }
        if (usedSpace >= trackSize) {
            result[result.length - 1] -= 1;
        }
        return result;
    }
    adjustPadding(ticksContainer) {
        const totalTickSize = this.tickSizes.reduce((prev, curr) => prev + curr, 0);
        const trackWidth = this.trackWidth();
        const reminder = trackWidth - totalTickSize;
        if (reminder !== 0) {
            const padding = reminder + this.elementOffset(this.track);
            ticksContainer.style.paddingTop = `${padding}px`;
        }
    }
    elementOffset(element) {
        const { vertical } = this.props;
        const style = getComputedStyle(element);
        return parseInt(vertical ? style.bottom : style.left, 10);
    }
    elementSize(element) {
        const { vertical } = this.props;
        return vertical ? element.clientHeight : element.clientWidth;
    }
}

/**
 * @hidden
 */
class SliderModel extends SliderModelBase {
    positionHandle(dragHandle) {
        const { max, min, reverse, vertical } = this.props;
        const position = vertical ? 'bottom' : 'left';
        const trackWidth = this.trackWidth();
        const value = trimValue(max, min, this.props.value);
        this.handlePosition = calculateHandlePosition({
            min,
            max,
            reverse,
            value,
            trackWidth,
            handleWidth: dragHandle.offsetWidth
        });
        this.renderer.setStyle(dragHandle, position, `${this.handlePosition}px`);
    }
    positionSelection(dragHandle, selection) {
        const { reverse, vertical } = this.props;
        const dimension = vertical ? 'height' : 'width';
        const handleWidth = Math.floor(dragHandle.offsetWidth / 2);
        let size = this.handlePosition + handleWidth;
        if (reverse) {
            size = this.trackWidth() - size;
        }
        this.renderer.setStyle(selection, dimension, `${size}px`);
    }
}

const UNTOUCHED = 'ng-untouched';
const toClassList = (classNames) => String(classNames).trim().split(' ');
/**
 * @hidden
 */
const hasClass = (element, className) => Boolean(toClassList(element.className).find((name) => name === className));
/**
 * @hidden
 */
function invokeElementMethod(element, name, ...args) {
    if (element && element.nativeElement) {
        return element.nativeElement[name].apply(element.nativeElement, args);
    }
}
/**
 * @hidden
 */
const isUntouched = (element) => element && element.nativeElement && hasClass(element.nativeElement, UNTOUCHED);
/**
 * @hidden
 */
const containsFocus = (hostElement, contender) => hostElement && contender && (hostElement === contender || hostElement.contains(contender));
/**
 * @hidden
 */
const closest = (node, predicate) => {
    while (node && !predicate(node)) {
        node = node.parentNode;
    }
    return node;
};

/**
 * Represents the template for the labels of the Slider.
 * To define the labels template, nest an `<ng-template>` tag with the `kendoSliderLabelTemplate` directive inside
 * the `<kendo-slider>` tag. The template context is passed to the `label` value.
 *
 * @example
 * ```ts-no-run
 *
 * _@Component({
 *     selector: 'my-app',
 *     template: `
 *         <kendo-slider [largeStep]="2">
 *           <ng-template kendoSliderLabelTemplate let-value="value">
 *             <b>{{value}}</b>
 *           </ng-template>
 *         </kendo-slider>
 *     `
 * })
 *
 * class AppComponent {
 * }
 *
 * ```
 */
class LabelTemplateDirective {
    constructor(templateRef) {
        this.templateRef = templateRef;
    }
}
LabelTemplateDirective.decorators = [
    { type: Directive, args: [{
                selector: '[kendoSliderLabelTemplate]'
            },] },
];
/** @nocollapse */
LabelTemplateDirective.ctorParameters = () => [
    { type: TemplateRef, decorators: [{ type: Optional }] }
];

/**
 * @hidden
 */
class SliderBase {
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

const FOCUSED = 'k-state-focused';
const PRESSED = 'k-pressed';
/**
 * Represents the [Kendo UI Slider component for Angular]({% slug overview_slider %}).
 */
class SliderComponent extends SliderBase {
    constructor(localization, injector, renderer, ngZone, changeDetector, hostElement) {
        super(localization, injector, renderer, ngZone, changeDetector, hostElement);
        this.localization = localization;
        this.injector = injector;
        this.renderer = renderer;
        this.ngZone = ngZone;
        this.changeDetector = changeDetector;
        this.hostElement = hostElement;
        /**
         * @hidden
         */
        this.focusableId = `k-${guid()}`;
        /**
         * Determines if the animation will be played on value change.
         * Regardless of this setting, no animation will be played during the initial rendering.
         */
        this.animate = true;
        /**
         * Renders the arrow side buttons of the Slider ([see example]({% slug sidebuttons_slider %}#toc-hidden-state)).
         * When `showButtons` is set to `false`, the buttons are not displayed.
         */
        this.showButtons = true;
        /**
         * The current value of the Slider when it is initially displayed.
         * The component can use either NgModel or the `value` binding but not both of them at the same time.
         */
        this.value = this.min;
        /**
         * @hidden
         */
        this.handleFocus = () => {
            this.focused = true;
            if (hasObservers(this.onFocus)) {
                this.ngZone.run(() => {
                    this.onFocus.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.handleBlur = () => {
            this.changeDetector.markForCheck();
            this.focused = false;
            if (hasObservers(this.onBlur) || requiresZoneOnBlur(this.control)) {
                this.ngZone.run(() => {
                    this.ngTouched();
                    this.onBlur.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.onWrapClick = (args) => {
            const target = args.target;
            if (!this.isDisabled && !(isButton(target) || isButton(target.parentNode))) {
                const value = eventValue(args, this.track.nativeElement, this.getProps());
                this.changeValue(value);
            }
        };
        /**
         * @hidden
         */
        this.onKeyDown = (e) => {
            const options = this.getProps();
            const { max, min } = options;
            const handler = this.keyBinding[e.keyCode];
            if (this.isDisabled || !handler) {
                return;
            }
            const value = handler(options);
            this.changeValue(trimValue(max, min, value));
            e.preventDefault();
        };
        this.ngChange = (_) => { };
        this.ngTouched = () => { };
        this.decreaseValue = () => {
            this.changeValue(decreaseValueToStep(this.value, this.getProps()));
        };
        this.increaseValue = () => {
            this.changeValue(increaseValueToStep(this.value, this.getProps()));
        };
    }
    /**
     * @hidden
     */
    set tabIndex(tabIndex) {
        this.tabindex = tabIndex;
    }
    get tabIndex() {
        return this.tabindex;
    }
    /**
     * Focuses the Slider.
     *
     * @example
     * ```ts-no-run
     * _@Component({
     * selector: 'my-app',
     * template: `
     *  <button (click)="slider.focus()">Focus</button>
     *  <kendo-slider #slider></kendo-slider>
     * `
     * })
     * class AppComponent { }
     * ```
     */
    focus() {
        invokeElementMethod(this.wrapper, 'focus');
    }
    /**
     * Blurs the Slider.
     */
    blur() {
        invokeElementMethod(this.wrapper, 'blur');
    }
    ngOnChanges(changes) {
        if (anyChanged(['value', 'fixedTickWidth', 'tickPlacement'], changes, true)) {
            this.ngZone.onStable.asObservable().pipe(take(1)).subscribe(() => {
                this.sizeComponent(false);
            });
        }
    }
    ngAfterViewInit() {
        if (!isDocumentAvailable()) {
            return;
        }
        if (this.showButtons) {
            this.setValueChangeInterval(this.increaseButton.nativeElement, () => this.increaseValue());
            this.setValueChangeInterval(this.decreaseButton.nativeElement, () => this.decreaseValue());
        }
        this.sizeComponent(false);
        if (this.ticks) {
            this.ticks.tickElements
                .changes
                .subscribe(() => this.sizeComponent(false));
        }
    }
    ngOnDestroy() {
        this.subscriptions.unsubscribe();
    }
    /**
     * @hidden
     */
    get incrementMessage() {
        return this.incrementTitle || this.localizationService.get('increment');
    }
    /**
     * @hidden
     */
    get decrementMessage() {
        return this.decrementTitle || this.localizationService.get('decrement');
    }
    /**
     * @hidden
     */
    get dragHandleMessage() {
        return this.dragHandleTitle || this.localizationService.get('dragHandle');
    }
    /**
     * @hidden
     */
    handleDragPress(args) {
        if (args.originalEvent) {
            args.originalEvent.preventDefault();
        }
        this.focus();
        this.renderer.removeClass(this.hostElement.nativeElement, 'k-slider-transitions');
    }
    /**
     * @hidden
     */
    onHandleDrag(args) {
        this.dragging = true;
        this.changeValue(eventValue(args, this.track.nativeElement, this.getProps()));
    }
    /**
     * @hidden
     */
    onHandleRelease() {
        this.dragging = false; //needed for animation
        this.renderer.addClass(this.hostElement.nativeElement, 'k-slider-transitions');
    }
    //ngModel binding
    /**
     * @hidden
     */
    writeValue(value) {
        this.value = value;
        this.sizeComponent(this.animate);
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.ngChange = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.ngTouched = fn;
    }
    /**
     * @hidden
     */
    changeValue(value) {
        if (!areSame(this.value, value)) {
            this.ngZone.run(() => {
                this.value = value;
                this.ngChange(value);
                this.valueChange.emit(value);
                this.sizeComponent(this.animate);
            });
        }
    }
    /**
     * @hidden
     */
    sizeComponent(animate) {
        if (!isDocumentAvailable()) {
            return;
        }
        const wrapper = this.wrapper.nativeElement;
        const track = this.track.nativeElement;
        const selectionEl = this.sliderSelection.nativeElement;
        const dragHandleEl = this.draghandle.nativeElement;
        const ticks = this.ticks ? this.ticksContainer.nativeElement : null;
        if (!animate) {
            this.renderer.removeClass(this.hostElement.nativeElement, 'k-slider-transitions');
        }
        this.resetStyles([track, selectionEl, dragHandleEl, ticks, this.hostElement.nativeElement]);
        const props = this.getProps();
        const model = new SliderModel(props, wrapper, track, this.renderer);
        model.resizeTrack();
        if (this.ticks) { //for case when tickPlacement: none
            model.resizeTicks(this.ticksContainer.nativeElement, this.ticks.tickElements.map(element => element.nativeElement));
        }
        model.positionHandle(dragHandleEl);
        model.positionSelection(dragHandleEl, selectionEl);
        if (!animate) {
            this.hostElement.nativeElement.getBoundingClientRect();
            this.renderer.addClass(this.hostElement.nativeElement, 'k-slider-transitions');
        }
        if (this.fixedTickWidth) {
            model.resizeWrapper();
        }
    }
    set focused(value) {
        if (this.isFocused !== value && this.hostElement) {
            const element = this.hostElement.nativeElement;
            if (value) {
                this.renderer.addClass(element, FOCUSED);
            }
            else {
                this.renderer.removeClass(element, FOCUSED);
            }
            this.isFocused = value;
        }
    }
    set dragging(value) {
        if (this.isDragged !== value && this.sliderSelection && this.draghandle) {
            const sliderSelection = this.sliderSelection.nativeElement;
            const draghandle = this.draghandle.nativeElement;
            if (value) {
                this.renderer.addClass(sliderSelection, PRESSED);
                this.renderer.addClass(draghandle, PRESSED);
            }
            else {
                this.renderer.removeClass(sliderSelection, PRESSED);
                this.renderer.removeClass(draghandle, PRESSED);
            }
            this.isDragged = value;
        }
    }
    setValueChangeInterval(element, callback) {
        this.ngZone.runOutsideAngular(() => {
            const mousedown = fromEvent(element, 'mousedown');
            const mouseup = fromEvent(element, 'mouseup');
            const mouseout = fromEvent(element, 'mouseout');
            const subscription = mousedown.pipe(filter((e) => e.button === 0 && !this.isDisabled), concatMap(() => interval(150).pipe(startWith(-1), takeUntil(merge(mouseup, mouseout))))).subscribe(() => {
                this.focus();
                callback();
            });
            this.subscriptions.add(subscription);
        });
    }
    getProps() {
        return {
            buttons: this.showButtons,
            disabled: this.disabled,
            fixedTickWidth: this.fixedTickWidth,
            largeStep: this.largeStep,
            max: this.max,
            min: this.min,
            readonly: this.readonly,
            reverse: this.reverse,
            rtl: this.localizationService.rtl,
            smallStep: this.smallStep,
            value: trimValue(this.max, this.min, this.value),
            vertical: this.vertical
        };
    }
}
SliderComponent.decorators = [
    { type: Component, args: [{
                exportAs: 'kendoSlider',
                providers: [
                    LocalizationService,
                    { provide: L10N_PREFIX, useValue: 'kendo.slider' },
                    { multi: true, provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SliderComponent) },
                    { provide: KendoInput, useExisting: forwardRef(() => SliderComponent) }
                ],
                selector: 'kendo-slider',
                template: `
        <ng-container kendoSliderLocalizedMessages
            i18n-increment="kendo.slider.increment|The title of the **Increase** button of the Slider."
            increment="increment"
            i18n-decrement="kendo.slider.decrement|The title of the **Decrease** button of the Slider."
            decrement="decrement"
            i18n-dragHandle="kendo.slider.dragHandle|The title of the drag handle of the Slider."
            dragHandle="Drag"
        >
        <div class="k-slider-wrap" #wrap
            [id]="focusableId"
            [class.k-slider-buttons]="showButtons"
            [class.k-slider-topleft]="tickPlacement === 'before'"
            [class.k-slider-bottomright]="tickPlacement === 'after'"
            [attr.tabindex]="disabled ? undefined : tabIndex"
            [kendoEventsOutsideAngular]="{ focus: handleFocus, blur: handleBlur, click: onWrapClick, keydown: onKeyDown }"
            >
            <a
                #decreaseButton
                *ngIf="showButtons"
                class="k-button k-button-decrease"
                [title]="decrementMessage"
                [attr.aria-label]="decrementMessage"
            >
                <span class="k-icon"
                    [class.k-i-arrow-w]="!vertical"
                    [class.k-i-arrow-s]="vertical"
                >
                </span>
            </a>
            <a
                *ngIf="showButtons"
                #increaseButton
                class="k-button k-button-increase"
                [title]="incrementMessage"
                [attr.aria-label]="incrementMessage"
            >
                <span class="k-icon"
                    [class.k-i-arrow-e]="!vertical"
                    [class.k-i-arrow-n]="vertical"
                >
                </span>
            </a>
            <ul kendoSliderTicks
                #ticks
                *ngIf="tickPlacement !== 'none'"
                [tickTitle]="title"
                [vertical]="vertical"
                [step]="smallStep"
                [largeStep]="largeStep"
                [min]="min"
                [max]="max"
                [labelTemplate]="labelTemplate?.templateRef"
            >
            </ul>
        <div #track class="k-slider-track">
            <div #sliderSelection class="k-slider-selection">
            </div>
                <a #draghandle
                    role="slider"
                    [attr.aria-valuemin]="min"
                    [attr.aria-valuemax]="max"
                    [attr.aria-valuenow]="value"
                    [attr.aria-disabled]="disabled ? true : undefined"
                    [attr.aria-readonly]="readonly ? true : undefined"
                    [attr.aria-orientation]="vertical ? 'vertical' : 'horizontal'"
                    [style.touch-action]="isDisabled ? '' : 'none'"
                    class="k-draghandle"
                    [title]="dragHandleMessage"
                    kendoDraggable
                    (kendoPress)="ifEnabled(handleDragPress, $event)"
                    (kendoDrag)="ifEnabled(onHandleDrag, $event)"
                    (kendoRelease)="ifEnabled(onHandleRelease, $event)"
                ></a>
            </div>
            <kendo-resize-sensor (resize)="sizeComponent(false)"></kendo-resize-sensor>
        </div>
  `
            },] },
];
/** @nocollapse */
SliderComponent.ctorParameters = () => [
    { type: LocalizationService },
    { type: Injector },
    { type: Renderer2 },
    { type: NgZone },
    { type: ChangeDetectorRef },
    { type: ElementRef }
];
SliderComponent.propDecorators = {
    focusableId: [{ type: Input }],
    dragHandleTitle: [{ type: Input }],
    incrementTitle: [{ type: Input }],
    animate: [{ type: Input }],
    decrementTitle: [{ type: Input }],
    showButtons: [{ type: Input }],
    value: [{ type: Input }],
    tabIndex: [{ type: Input }],
    draghandle: [{ type: ViewChild, args: ['draghandle', { static: true },] }],
    decreaseButton: [{ type: ViewChild, args: ['decreaseButton',] }],
    increaseButton: [{ type: ViewChild, args: ['increaseButton',] }]
};

/**
 * @hidden
 */
class RangeSliderModel extends SliderModelBase {
    positionHandle(dragHandle) {
        if (!dragHandle.id) {
            return;
        }
        const { max, min, reverse, vertical } = this.props;
        const position = vertical ? 'bottom' : 'left';
        const trackWidth = this.trackWidth();
        const value = isStartHandle(dragHandle) ? trimValueRange(max, min, this.props.value)[0]
            : trimValueRange(max, min, this.props.value)[1];
        if (isStartHandle(dragHandle)) {
            this.startHandlePosition = calculateHandlePosition({
                min,
                max,
                reverse,
                value,
                trackWidth,
                handleWidth: dragHandle.offsetWidth
            });
            this.renderer.setStyle(dragHandle, position, `${this.startHandlePosition}px`);
        }
        else {
            this.endHandlePosition = calculateHandlePosition({
                min,
                max,
                reverse,
                value,
                trackWidth,
                handleWidth: dragHandle.offsetWidth
            });
            this.renderer.setStyle(dragHandle, position, `${this.endHandlePosition}px`);
        }
    }
    positionSelection(dragHandle, selection) {
        const { reverse, vertical } = this.props;
        const dimension = vertical ? 'height' : 'width';
        const position = vertical ? 'bottom' : reverse ? 'right' : 'left';
        const handleWidth = Math.floor(dragHandle.offsetWidth / 2);
        const size = Math.abs(this.endHandlePosition - this.startHandlePosition);
        const currentSelectionPosition = vertical ? dragHandle.style.bottom : dragHandle.style.left;
        this.renderer.setStyle(selection, dimension, `${size}px`);
        this.renderer.setStyle(selection, position, reverse ? this.trackWidth() - parseFloat(currentSelectionPosition) - handleWidth + 'px'
            : parseFloat(currentSelectionPosition) + handleWidth + 'px');
    }
}

const PRESSED$1 = 'k-pressed';
/**
 * Represents the [Kendo UI RangeSlider component for Angular]({% slug overview_rangeslider %}).
 */
class RangeSliderComponent extends SliderBase {
    constructor(localization, injector, renderer, ngZone, changeDetector, hostElement) {
        super(localization, injector, renderer, ngZone, changeDetector, hostElement);
        this.localization = localization;
        this.injector = injector;
        this.renderer = renderer;
        this.ngZone = ngZone;
        this.changeDetector = changeDetector;
        this.hostElement = hostElement;
        /**
         * @hidden
         */
        this.startHandleId = `k-start-handle-${guid()}`;
        /**
         * @hidden
         */
        this.endHandleId = `k-end-handle-${guid()}`;
        /**
         * @hidden
         */
        this.focusableId = this.startHandleId;
        this.handleZIndex = 0;
        this.activeHandle = 'startHandle';
        this.focusChangedProgrammatically = false;
        /**
         * @hidden
         */
        this.onWrapClick = (args) => {
            if (!this.isDisabled) {
                this.value = this.value || [this.min, this.min];
                const trackValue = eventValue(args, this.track.nativeElement, this.getProps());
                let newRangeValue;
                const [startValue, endValue] = newRangeValue = this.value;
                if (trackValue <= startValue) {
                    newRangeValue = [trackValue, endValue];
                    this.activeHandle = 'startHandle';
                }
                else if (startValue < trackValue && trackValue < endValue) {
                    if (trackValue < (startValue + endValue) / 2) {
                        newRangeValue = [trackValue, endValue];
                        this.activeHandle = 'startHandle';
                    }
                    else {
                        newRangeValue = [startValue, trackValue];
                        this.activeHandle = 'endHandle';
                    }
                }
                else if (trackValue >= endValue) {
                    newRangeValue = [startValue, trackValue];
                    this.activeHandle = 'endHandle';
                }
                const activeHandle = this.activeHandle === 'startHandle' ? this.draghandleStart : this.draghandleEnd;
                invokeElementMethod(activeHandle, 'focus');
                this.changeValue(newRangeValue);
            }
        };
        /**
         * @hidden
         */
        this.onKeyDown = (e) => {
            this.value = this.value || [this.min, this.min];
            const options = this.getProps();
            const { max, min } = options;
            const handler = this.keyBinding[e.keyCode];
            if (this.isDisabled || !handler) {
                return;
            }
            this.renderer.setStyle(e.target, 'zIndex', ++this.handleZIndex);
            const startHandleIsActive = isStartHandle(e.target);
            const value = handler(Object.assign({}, options, { value: startHandleIsActive ? this.value[0] : this.value[1] }));
            if (startHandleIsActive) {
                if (value > this.value[1]) {
                    this.value[1] = value;
                }
            }
            else {
                if (value < this.value[0]) {
                    this.value[0] = value;
                }
            }
            const trimmedValue = trimValue(max, min, value);
            const newValue = startHandleIsActive ? [trimmedValue, this.value[1]]
                : [this.value[0], trimmedValue];
            this.changeValue(newValue);
            e.preventDefault();
        };
        this.ngChange = (_) => { };
        this.ngTouched = () => { };
        this.handleBlur = () => {
            this.changeDetector.markForCheck();
            this.focused = false;
            if (hasObservers(this.onBlur) || requiresZoneOnBlur(this.control)) {
                this.ngZone.run(() => {
                    this.ngTouched();
                    if (!this.focusChangedProgrammatically) {
                        this.onBlur.emit();
                    }
                });
            }
        };
    }
    /**
     * Focuses the RangeSlider.
     *
     * @example
     * ```ts-no-run
     * _@Component({
     * selector: 'my-app',
     * template: `
     *     <div>
     *         <button class="k-button" (click)="slider.focus()">Focus</button>
     *     </div>
     *     <kendo-rangeslider #slider></kendo-rangeslider>
     * `
     * })
     * class AppComponent { }
     * ```
     */
    focus() {
        this.focusChangedProgrammatically = true;
        invokeElementMethod(this.draghandleStart, 'focus');
        this.focusChangedProgrammatically = false;
    }
    /**
     * Blurs the RangeSlider.
     */
    blur() {
        this.focusChangedProgrammatically = true;
        const activeHandle = this.activeHandle === 'startHandle' ? this.draghandleStart : this.draghandleEnd;
        invokeElementMethod(activeHandle, 'blur');
        this.handleBlur();
        this.focusChangedProgrammatically = false;
    }
    ngOnInit() {
        if (!this.value) {
            this.value = [this.min, this.max];
        }
        super.ngOnInit();
    }
    ngOnChanges(changes) {
        if (anyChanged(['value', 'fixedTickWidth', 'tickPlacement'], changes, true)) {
            if (changes.value && changes.value.currentValue) {
                validateValue(changes.value.currentValue);
            }
            this.ngZone.onStable.asObservable().pipe(take(1)).subscribe(() => {
                this.sizeComponent();
            });
        }
    }
    ngAfterViewInit() {
        if (!isDocumentAvailable()) {
            return;
        }
        this.sizeComponent();
        if (this.ticks) {
            this.ticks.tickElements
                .changes
                .subscribe(() => this.sizeComponent());
        }
        this.attachElementEventHandlers();
    }
    ngOnDestroy() {
        this.subscriptions.unsubscribe();
    }
    /**
     * @hidden
     */
    textFor(key) {
        return this.localization.get(key);
    }
    /**
     * @hidden
     */
    get valueText() {
        return this.value ? `${this.value[0]} - ${this.value[1]}` : '';
    }
    /**
     * @hidden
     */
    handleDragPress(args) {
        if (args.originalEvent) {
            args.originalEvent.preventDefault();
        }
        const target = args.originalEvent.target;
        this.draggedHandle = target;
        this.renderer.setStyle(target, 'zIndex', ++this.handleZIndex);
    }
    /**
     * @hidden
     */
    onHandleDrag(args) {
        this.value = this.value || [this.min, this.min];
        const target = args.originalEvent.target;
        const lastCoords = this.draggedHandle.getBoundingClientRect();
        this.lastHandlePosition = { x: lastCoords.left, y: lastCoords.top };
        this.dragging = { value: true, target };
        const left = args.pageX < this.lastHandlePosition.x;
        const right = args.pageX > this.lastHandlePosition.x;
        const up = args.pageY > this.lastHandlePosition.y;
        const moveStartHandle = () => this.changeValue([eventValue(args, this.track.nativeElement, this.getProps()), this.value[1]]);
        const moveEndHandle = () => this.changeValue([this.value[0], eventValue(args, this.track.nativeElement, this.getProps())]);
        const moveBothHandles = () => this.changeValue([eventValue(args, this.track.nativeElement, this.getProps()), eventValue(args, this.track.nativeElement, this.getProps())]);
        const activeStartHandle = isStartHandle(this.draggedHandle);
        const vertical = this.vertical;
        const horizontal = !vertical;
        const forward = (vertical && up) || (this.reverse ? horizontal && right : horizontal && left);
        // const forward = (this.reverse ? (!this.vertical && !left) : (!this.vertical && left)) || (this.vertical && up);
        if (this.value[0] === this.value[1]) {
            if (forward) {
                activeStartHandle ? moveStartHandle() : moveBothHandles();
            }
            else {
                activeStartHandle ? moveBothHandles() : moveEndHandle();
            }
        }
        else {
            activeStartHandle ? moveStartHandle() : moveEndHandle();
        }
    }
    /**
     * @hidden
     */
    onHandleRelease(args) {
        this.dragging = { value: false, target: args.originalEvent.target }; //needed for animation
        this.draggedHandle = undefined;
    }
    //ngModel binding
    /**
     * @hidden
     */
    writeValue(value) {
        validateValue(value);
        this.value = value;
        this.sizeComponent();
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.ngChange = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.ngTouched = fn;
    }
    /**
     * @hidden
     */
    changeValue(value) {
        if (!this.value || !isSameRange(this.value, value)) {
            this.ngZone.run(() => {
                this.value = value;
                this.ngChange(value);
                if (this.value) {
                    this.valueChange.emit(value);
                }
                this.sizeComponent();
            });
        }
    }
    /**
     * @hidden
     */
    sizeComponent() {
        if (!isDocumentAvailable()) {
            return;
        }
        const wrapper = this.wrapper.nativeElement;
        const track = this.track.nativeElement;
        const selectionEl = this.sliderSelection.nativeElement;
        const dragHandleStartEl = this.draghandleStart.nativeElement;
        const dragHandleEndEl = this.draghandleEnd.nativeElement;
        const ticks = this.ticks ? this.ticksContainer.nativeElement : null;
        this.resetStyles([track, selectionEl, dragHandleStartEl, dragHandleEndEl, ticks, this.hostElement.nativeElement]);
        const props = this.getProps();
        const model = new RangeSliderModel(props, wrapper, track, this.renderer);
        model.resizeTrack();
        if (this.ticks) { //for case when tickPlacement: none
            model.resizeTicks(this.ticksContainer.nativeElement, this.ticks.tickElements.map(element => element.nativeElement));
        }
        model.positionHandle(dragHandleStartEl);
        model.positionHandle(dragHandleEndEl);
        model.positionSelection(dragHandleStartEl, selectionEl);
        if (this.fixedTickWidth) {
            model.resizeWrapper();
        }
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
    set focused(value) {
        if (this.isFocused !== value && this.hostElement) {
            this.isFocused = value;
        }
    }
    set dragging(data) {
        if (this.isDragged !== data.value && this.sliderSelection && this.draghandleStart && this.draghandleEnd) {
            const sliderSelection = this.sliderSelection.nativeElement;
            const draghandle = data.target;
            if (data.value) {
                this.renderer.addClass(sliderSelection, PRESSED$1);
                this.renderer.addClass(draghandle, PRESSED$1);
            }
            else {
                this.renderer.removeClass(sliderSelection, PRESSED$1);
                this.renderer.removeClass(draghandle, PRESSED$1);
            }
            this.isDragged = data.value;
        }
    }
    getProps() {
        return {
            disabled: this.disabled,
            fixedTickWidth: this.fixedTickWidth,
            largeStep: this.largeStep,
            max: this.max,
            min: this.min,
            readonly: this.readonly,
            reverse: this.reverse,
            rtl: this.localizationService.rtl,
            smallStep: this.smallStep,
            value: trimValueRange(this.max, this.min, this.value),
            vertical: this.vertical,
            buttons: false
        };
    }
    attachElementEventHandlers() {
        const hostElement = this.hostElement.nativeElement;
        let tabbing = false;
        let cursorInsideWrapper = false;
        this.ngZone.runOutsideAngular(() => {
            // focusIn and focusOut are relative to the host element
            this.subscriptions.add(this.renderer.listen(hostElement, 'focusin', () => {
                if (!this.isFocused) {
                    this.ngZone.run(() => {
                        if (!this.focusChangedProgrammatically) {
                            this.onFocus.emit();
                        }
                        this.focused = true;
                    });
                }
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'focusout', (args) => {
                if (!this.isFocused) {
                    return;
                }
                if (tabbing) {
                    if (args.relatedTarget !== this.draghandleStart.nativeElement && args.relatedTarget !== this.draghandleEnd.nativeElement) {
                        this.handleBlur();
                    }
                    tabbing = false;
                }
                else {
                    if (!cursorInsideWrapper) {
                        this.handleBlur();
                    }
                }
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'mouseenter', () => {
                cursorInsideWrapper = true;
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'mouseleave', () => {
                cursorInsideWrapper = false;
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'keydown', (args) => {
                if (args.keyCode === Keys.Tab) {
                    tabbing = true;
                }
                else {
                    tabbing = false;
                }
            }));
        });
    }
}
RangeSliderComponent.decorators = [
    { type: Component, args: [{
                exportAs: 'kendoRangeSlider',
                providers: [
                    LocalizationService,
                    { provide: L10N_PREFIX, useValue: 'kendo.rangeslider' },
                    { multi: true, provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RangeSliderComponent) },
                    { provide: KendoInput, useExisting: forwardRef(() => RangeSliderComponent) }
                ],
                selector: 'kendo-rangeslider',
                template: `
        <ng-container kendoSliderLocalizedMessages
            i18n-dragHandleStart="kendo.rangeslider.dragHandleStart|The title of the **Start** drag handle of the Slider."
            dragHandleStart="Drag"
            i18n-dragHandleEnd="kendo.rangeslider.dragHandleEnd|The title of the **End** drag handle of the Slider."
            dragHandleEnd="Drag"
        >

        <div class="k-slider-wrap" #wrap
            [class.k-slider-topleft]="tickPlacement === 'before'"
            [class.k-slider-bottomright]="tickPlacement === 'after'"
            [kendoEventsOutsideAngular]="{ click: onWrapClick, keydown: onKeyDown }"
            >
            <ul kendoSliderTicks
                #ticks
                *ngIf="tickPlacement !== 'none'"
                [tickTitle]="title"
                [vertical]="vertical"
                [step]="smallStep"
                [largeStep]="largeStep"
                [min]="min"
                [max]="max"
                [labelTemplate]="labelTemplate?.templateRef"
            >
            </ul>
            <div #track class="k-slider-track">
                <div #sliderSelection class="k-slider-selection">
                </div>
                <a #draghandleStart
                    role="slider"
                    [id]="startHandleId"
                    [attr.tabindex]="disabled ? undefined : tabindex"
                    [attr.aria-valuemin]="min"
                    [attr.aria-valuemax]="max"
                    [attr.aria-valuenow]="value ? value[0] : null"
                    [attr.aria-valuetext]="valueText"
                    [attr.aria-disabled]="disabled ? true : undefined"
                    [attr.aria-readonly]="readonly ? true : undefined"
                    [attr.aria-orientation]="vertical ? 'vertical' : 'horizontal'"
                    [style.touch-action]="isDisabled ? '' : 'none'"
                    class="k-draghandle"
                    [title]="textFor('dragHandleStart')"
                    kendoDraggable
                    (kendoPress)="ifEnabled(handleDragPress ,$event)"
                    (kendoDrag)="ifEnabled(onHandleDrag ,$event)"
                    (kendoRelease)="ifEnabled(onHandleRelease, $event)"
                ></a>
                <a #draghandleEnd
                    role="slider"
                    [id]="endHandleId"
                    [attr.tabindex]="disabled ? undefined : tabindex"
                    [attr.aria-valuemin]="min"
                    [attr.aria-valuemax]="max"
                    [attr.aria-valuenow]="value ? value[1] : null"
                    [attr.aria-valuetext]="valueText"
                    [attr.aria-disabled]="disabled ? true : undefined"
                    [attr.aria-readonly]="readonly ? true : undefined"
                    [attr.aria-orientation]="vertical ? 'vertical' : 'horizontal'"
                    [style.touch-action]="isDisabled ? '' : 'none'"
                    class="k-draghandle"
                    [title]="textFor('dragHandleEnd')"
                    kendoDraggable
                    (kendoPress)="ifEnabled(handleDragPress ,$event)"
                    (kendoDrag)="ifEnabled(onHandleDrag ,$event)"
                    (kendoRelease)="ifEnabled(onHandleRelease, $event)"
                ></a>
            </div>

            <kendo-resize-sensor (resize)="sizeComponent()"></kendo-resize-sensor>
        </div>
  `
            },] },
];
/** @nocollapse */
RangeSliderComponent.ctorParameters = () => [
    { type: LocalizationService },
    { type: Injector },
    { type: Renderer2 },
    { type: NgZone },
    { type: ChangeDetectorRef },
    { type: ElementRef }
];
RangeSliderComponent.propDecorators = {
    value: [{ type: Input }],
    draghandleStart: [{ type: ViewChild, args: ['draghandleStart', { static: true },] }],
    draghandleEnd: [{ type: ViewChild, args: ['draghandleEnd', { static: true },] }]
};

const FOCUSED$1 = 'k-state-focused';
/**
 * Represents the [Kendo UI Switch component for Angular]({% slug overview_switch %}).
 */
class SwitchComponent {
    constructor(renderer, hostElement, localizationService, injector, changeDetector, ngZone) {
        this.renderer = renderer;
        this.hostElement = hostElement;
        this.localizationService = localizationService;
        this.injector = injector;
        this.changeDetector = changeDetector;
        this.ngZone = ngZone;
        /**
         * @hidden
         */
        this.focusableId = `k-${guid()}`;
        /**
         * Sets the current value of the Switch when it is initially displayed.
         */
        this.checked = false;
        /**
         * Determines whether the Switch is disabled ([see example]({% slug disabled_switch %})).
         */
        this.disabled = false;
        /**
         * Determines whether the Switch is in its read-only state ([see example]({% slug readonly_switch %})).
         */
        this.readonly = false;
        /**
         * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the Switch.
         */
        this.tabindex = 0;
        /**
         * Fires each time the user focuses the `input` element.
         */
        this.onFocus = new EventEmitter(); //tslint:disable-line:no-output-rename
        /**
         * Fires each time the `input` element gets blurred.
         */
        this.onBlur = new EventEmitter(); //tslint:disable-line:no-output-rename
        /**
         * Fires each time the user selects a new value.
         */
        this.valueChange = new EventEmitter();
        this.ngChange = (_) => { };
        this.ngTouched = () => { };
        /**
         * @hidden
         */
        this.handleFocus = () => {
            this.focused = true;
            if (hasObservers(this.onFocus)) {
                this.ngZone.run(() => {
                    this.onFocus.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.handleBlur = () => {
            this.changeDetector.markForCheck();
            this.focused = false;
            if (hasObservers(this.onBlur) || requiresZoneOnBlur(this.control)) {
                this.ngZone.run(() => {
                    this.ngTouched();
                    this.onBlur.emit();
                });
            }
        };
        this.direction = localizationService.rtl ? 'rtl' : 'ltr';
        this.keyDownHandler = this.keyDownHandler.bind(this);
        this.clickHandler = this.clickHandler.bind(this);
    }
    /**
     * @hidden
     */
    set tabIndex(tabIndex) {
        this.tabindex = tabIndex;
    }
    get tabIndex() {
        return this.tabindex;
    }
    get ieClass() {
        return browser && browser.msie;
    }
    get ariaDisabled() {
        return this.disabled ? true : undefined;
    }
    get ariaReadonly() {
        return this.readonly;
    }
    get hostClasses() {
        return true;
    }
    get switchOnClass() {
        return this.checked;
    }
    get switchOffClass() {
        return !this.checked;
    }
    get disabledClass() {
        return this.disabled;
    }
    /**
     * @hidden
     */
    get onLabelMessage() {
        return this.onLabel || this.localizationService.get('on');
    }
    /**
     * @hidden
     */
    get offLabelMessage() {
        return this.offLabel || this.localizationService.get('off');
    }
    get isEnabled() {
        return !this.disabled && !this.readonly;
    }
    ngOnInit() {
        if (this.hostElement) {
            const wrapper = this.hostElement.nativeElement;
            this.renderer.removeAttribute(wrapper, "tabindex");
        }
        this.localizationChangeSubscription = this.localizationService
            .changes
            .pipe(skip(1))
            .subscribe(({ rtl }) => {
            this.direction = rtl ? 'rtl' : 'ltr';
        });
        this.control = this.injector.get(NgControl, null);
    }
    ngOnDestroy() {
        if (this.localizationChangeSubscription) {
            this.localizationChangeSubscription.unsubscribe();
        }
    }
    /**
     * Focuses the Switch.
     *
     * @example
     * ```ts-no-run
     * _@Component({
     * selector: 'my-app',
     * template: `
     *  <button (click)="switch.focus()">Focus</button>
     *  <kendo-switch #switch></kendo-switch>
     * `
     * })
     * class AppComponent { }
     * ```
     */
    focus() {
        if (!this.wrapper) {
            return;
        }
        this.wrapper.nativeElement.focus();
    }
    /**
     * Blurs the Switch.
     */
    blur() {
        if (!this.wrapper) {
            return;
        }
        this.wrapper.nativeElement.blur();
    }
    /**
     * @hidden
     * Called when the status of the component changes to or from `disabled`.
     * Depending on the value, it enables or disables the appropriate DOM element.
     */
    setDisabledState(isDisabled) {
        this.disabled = isDisabled;
        this.changeDetector.markForCheck();
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.checked = value === null ? false : value;
        this.changeDetector.markForCheck();
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.ngChange = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.ngTouched = fn;
    }
    /**
     * @hidden
     */
    keyDownHandler(e) {
        const keyCode = e.keyCode;
        if (this.isEnabled && (keyCode === Keys.Space || keyCode === Keys.Enter)) {
            this.changeValue(!this.checked);
            e.preventDefault();
        }
    }
    /**
     * @hidden
     */
    clickHandler() {
        if (this.isEnabled) {
            this.changeValue(!this.checked);
        }
    }
    /**
     * @hidden
     * Used by the TextBoxContainer to determine if the component is empty.
     */
    isEmpty() {
        return false;
    }
    changeValue(value) {
        if (this.checked !== value) {
            this.ngZone.run(() => {
                this.checked = value;
                this.ngChange(value);
                this.valueChange.emit(value);
                this.changeDetector.markForCheck();
            });
        }
    }
    set focused(value) {
        if (this.isFocused !== value && this.hostElement) {
            const element = this.hostElement.nativeElement;
            if (value) {
                this.renderer.addClass(element, FOCUSED$1);
            }
            else {
                this.renderer.removeClass(element, FOCUSED$1);
            }
            this.isFocused = value;
        }
    }
}
SwitchComponent.decorators = [
    { type: Component, args: [{
                exportAs: 'kendoSwitch',
                providers: [
                    LocalizationService,
                    { provide: L10N_PREFIX, useValue: 'kendo.switch' },
                    {
                        multi: true,
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => SwitchComponent) /* tslint:disable-line */
                    },
                    {
                        provide: KendoInput,
                        useExisting: forwardRef(() => SwitchComponent)
                    }
                ],
                selector: 'kendo-switch',
                template: `
        <ng-container kendoSwitchLocalizedMessages
            i18n-on="kendo.switch.on|The **On** label of the Switch."
            on="ON"
            i18n-off="kendo.switch.off|The **Off** label of the Switch."
            off="OFF"
        >

        <span
            #wrapper
            class="k-switch-container"
            [id]="focusableId"
            role="switch"
            [attr.aria-checked]="checked"
            [attr.tabindex]="(disabled ? undefined : tabIndex)"
            [kendoEventsOutsideAngular]="{ click: clickHandler, keydown: keyDownHandler, focus: handleFocus, blur: handleBlur }"
        >
            <span class="k-switch-label-on" [attr.aria-hidden]="true" >{{onLabelMessage}}</span>
            <span class="k-switch-label-off" [attr.aria-hidden]="true">{{offLabelMessage}}</span>
            <span class="k-switch-handle"></span>
        </span>
  `
            },] },
];
/** @nocollapse */
SwitchComponent.ctorParameters = () => [
    { type: Renderer2 },
    { type: ElementRef },
    { type: LocalizationService },
    { type: Injector },
    { type: ChangeDetectorRef },
    { type: NgZone }
];
SwitchComponent.propDecorators = {
    focusableId: [{ type: Input }],
    onLabel: [{ type: Input }],
    offLabel: [{ type: Input }],
    checked: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    tabindex: [{ type: Input }],
    tabIndex: [{ type: Input }],
    onFocus: [{ type: Output, args: ['focus',] }],
    onBlur: [{ type: Output, args: ['blur',] }],
    valueChange: [{ type: Output }],
    wrapper: [{ type: ViewChild, args: ['wrapper',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    ieClass: [{ type: HostBinding, args: ['class.k-ie',] }],
    ariaDisabled: [{ type: HostBinding, args: ['attr.aria-disabled',] }],
    ariaReadonly: [{ type: HostBinding, args: ['attr.aria-readonly',] }],
    hostClasses: [{ type: HostBinding, args: ['class.k-widget',] }, { type: HostBinding, args: ['class.k-switch',] }],
    switchOnClass: [{ type: HostBinding, args: ['class.k-switch-on',] }],
    switchOffClass: [{ type: HostBinding, args: ['class.k-switch-off',] }],
    disabledClass: [{ type: HostBinding, args: ['class.k-state-disabled',] }]
};

/**
 * Represents the [Kendo UI TextArea directive for the Inputs components for Angular]({% slug overview_textarea %}).
 * Provides floating labels to `textarea` elements.
 *
 * @example
 * ```ts-no-run
 * <textarea kendoTextArea></textarea>
 * ```
 */
class TextAreaDirective {
    constructor(renderer, element, zone, changeDetector, injector, rtl) {
        this.renderer = renderer;
        this.element = element;
        this.zone = zone;
        this.changeDetector = changeDetector;
        this.injector = injector;
        this.elementClass = true;
        this.autofillClass = true;
        /**
         * Fires each time the textarea value is changed.
         */
        this.valueChange = new EventEmitter();
        /**
         * Specifies if the `textarea` element will resize its height automatically
         * ([see example]({% slug overview_textarea %}#toc-auto-resizing)).
         *
         * @default false
         */
        this.autoSize = false;
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
        this.ngChange = (_) => { };
        this.ngTouched = () => { };
        this.direction = rtl ? 'rtl' : 'ltr';
    }
    get id() {
        return this.element.nativeElement.id;
    }
    set id(id) {
        this.renderer.setAttribute(this.element.nativeElement, 'id', id);
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.elementValue = value;
        this.resize();
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.ngChange = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.ngTouched = fn;
    }
    /**
     * @hidden
     */
    setDisabledState(isDisabled) {
        this.setElementProperty('disabled', isDisabled);
    }
    ngOnInit() {
        const element = this.element.nativeElement;
        this.zone.runOutsideAngular(() => {
            this.listeners = [
                this.renderer.listen(element, 'focus', this.handleFocus.bind(this)),
                this.renderer.listen(element, 'blur', this.handleBlur.bind(this)),
                this.renderer.listen(element, 'animationstart', (e) => {
                    if (e.animationName === 'autoFillStart') {
                        this.autoFillStart.emit();
                    }
                    else if (e.animationName === 'autoFillEnd') {
                        this.autoFillEnd.emit();
                    }
                })
            ];
            if (isDocumentAvailable() && this.autoSize) {
                this.resizeSubscription = fromEvent(window, 'resize')
                    .pipe((debounceTime(50)))
                    .subscribe(() => this.resize());
            }
            this.inputSubscription = fromEvent(element, 'input')
                .subscribe(this.handleInput.bind(this));
        });
        this.control = this.injector.get(NgControl, null);
    }
    ngOnChanges(changes) {
        const element = this.element.nativeElement;
        if (changes.value) {
            this.elementValue = this.value;
        }
        if (changes.autoSize) {
            if (this.autoSize) {
                this.initialHeight = element.offsetHeight;
                this.renderer.setStyle(element, 'resize', 'none');
            }
            else {
                this.renderer.setStyle(element, 'overflow-y', 'auto');
                this.renderer.setStyle(element, 'resize', 'both');
                element.style.height = `${this.initialHeight}px`;
            }
        }
        this.resize();
    }
    ngOnDestroy() {
        this.listeners.forEach(listener => listener());
        if (this.inputSubscription) {
            this.inputSubscription.unsubscribe();
        }
        if (this.resizeSubscription) {
            this.resizeSubscription.unsubscribe();
        }
    }
    get elementValue() {
        if (this.element) {
            return this.element.nativeElement.value;
        }
        return '';
    }
    set elementValue(value) {
        this.setElementProperty('value', (value === undefined || value === null) ? '' : value);
    }
    setElementProperty(name, value) {
        if (this.element) {
            this.renderer.setProperty(this.element.nativeElement, name, value);
        }
    }
    resize() {
        if (!this.autoSize) {
            return;
        }
        const element = this.element.nativeElement;
        this.renderer.setStyle(element, 'overflow-y', 'hidden');
        element.style.height = `${this.initialHeight}px`;
        const scrollHeight = element.scrollHeight;
        if (scrollHeight > this.initialHeight) {
            element.style.height = `${scrollHeight}px`;
        }
    }
    handleInput() {
        const value = this.elementValue;
        this.value = value;
        if (this.control || hasObservers(this.onValueChange) || hasObservers(this.valueChange)) {
            this.zone.run(() => {
                this.ngChange(value);
                this.onValueChange.emit(value);
                this.valueChange.emit(value);
                this.changeDetector.markForCheck();
            });
        }
        this.resize();
    }
    handleFocus() {
        if (hasObservers(this.onFocus)) {
            this.zone.run(() => {
                this.onFocus.emit();
            });
        }
    }
    handleBlur() {
        if (hasObservers(this.onBlur) || requiresZoneOnBlur(this.control)) {
            this.zone.run(() => {
                this.ngTouched();
                this.onBlur.emit();
                this.changeDetector.markForCheck();
            });
        }
    }
}
TextAreaDirective.decorators = [
    { type: Directive, args: [{
                providers: [{
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => TextAreaDirective),
                        multi: true
                    }, {
                        provide: KendoInput,
                        useExisting: forwardRef(() => TextAreaDirective)
                    }],
                selector: 'textarea[kendoTextArea]'
            },] },
];
/** @nocollapse */
TextAreaDirective.ctorParameters = () => [
    { type: Renderer2 },
    { type: ElementRef },
    { type: NgZone },
    { type: ChangeDetectorRef },
    { type: Injector },
    { type: Boolean, decorators: [{ type: Optional }, { type: Inject, args: [RTL,] }] }
];
TextAreaDirective.propDecorators = {
    elementClass: [{ type: HostBinding, args: ['class.k-textarea',] }],
    autofillClass: [{ type: HostBinding, args: ['class.k-autofill',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    valueChange: [{ type: Output }],
    autoSize: [{ type: Input }],
    value: [{ type: Input }]
};

/**
 * @hidden
 */
class FloatingLabelInputAdapter {
    constructor(component, formControl) {
        this.component = component;
        const isObservableOrEventEmitter = (event) => event instanceof Observable || event instanceof EventEmitter;
        if (isObservableOrEventEmitter(component.onFocus)) {
            this.onFocus = component.onFocus;
        }
        if (isObservableOrEventEmitter(component.autoFillStart)) {
            this.autoFillStart = component.autoFillStart;
        }
        if (isObservableOrEventEmitter(component.autoFillEnd)) {
            this.autoFillEnd = component.autoFillEnd;
        }
        if (isObservableOrEventEmitter(component.onBlur)) {
            this.onBlur = component.onBlur;
        }
        if (formControl) {
            this.onValueChange = formControl.valueChanges;
        }
        else if (component.onValueChange) {
            this.onValueChange = component.onValueChange;
        }
    }
    get focusableId() {
        const component = this.component;
        if ('focusableId' in component) {
            return component.focusableId;
        }
        else if ('id' in component) {
            return component.id;
        }
        return "";
    }
    set focusableId(value) {
        const component = this.component;
        if ('focusableId' in component) {
            component.focusableId = value;
        }
        else if ('id' in component) {
            component.id = value;
        }
    }
}

const isFunction = (x) => Object.prototype.toString.call(x) === '[object Function]';
/**
 * @hidden
 */
class TextBoxContainerComponent {
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
class TextBoxDirective {
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

/**
 * @hidden
 */
const createMaxValidator = (maxValue) => {
    return (c) => {
        if (!isPresent(maxValue) || !isPresent(c.value) || c.value <= maxValue) {
            return null;
        }
        return {
            maxError: {
                maxValue: maxValue,
                value: c.value
            }
        };
    };
};

/**
 * @hidden
 */
const createMinValidator = (minValue) => {
    return (c) => {
        if (!isPresent(minValue) || !isPresent(c.value) || c.value >= minValue) {
            return null;
        }
        return {
            minError: {
                minValue: minValue,
                value: c.value
            }
        };
    };
};

/**
 * @hidden
 */
const MIN_DOC_LINK = 'http://www.telerik.com/kendo-angular-ui/components/inputs/api/NumericTextBoxComponent/#toc-min';
/**
 * @hidden
 */
const MAX_DOC_LINK = 'http://www.telerik.com/kendo-angular-ui/components/inputs/api/NumericTextBoxComponent/#toc-max';
/**
 * @hidden
 */
const POINT = ".";
/**
 * @hidden
 */
const INITIAL_SPIN_DELAY = 500;
/**
 * @hidden
 */
const SPIN_DELAY = 50;
/**
 * @hidden
 */
const EXPONENT_REGEX = /[eE][\-+]?([0-9]+)/;

/**
 * @hidden
 */
const numericRegex = (options) => {
    const { autoCorrect, decimals, min } = options;
    let separator = options.separator;
    if (separator === POINT) {
        separator = '\\' + separator;
    }
    const signPattern = autoCorrect && min !== null && min >= 0 ? '' : '-?';
    let numberPattern;
    if (decimals === 0) {
        numberPattern = '\\d*';
    }
    else {
        numberPattern = `(?:(?:\\d+(${separator}\\d*)?)|(?:${separator}\\d*))?`;
    }
    return new RegExp(`^${signPattern}${numberPattern}$`);
};
/**
 * @hidden
 */
const decimalPart = (value) => {
    return value >= 0 ? Math.floor(value) : Math.ceil(value);
};
/**
 * @hidden
 */
const noop = (_) => { }; // tslint:disable-line:no-empty
/**
 * @hidden
 */
const defined = (value) => {
    return typeof value !== 'undefined';
};
/**
 * @hidden
 */
const isNumber = (value) => {
    return !isNaN(value) && value !== null;
};
/**
 * @hidden
 */
function pad(value, digits) {
    const count = digits - String(value).length;
    let result = value;
    if (count > 0) {
        const padString = new Array(count + 1).join("0");
        result = parseFloat(value + padString);
    }
    return result;
}
/**
 * @hidden
 */
const getDeltaFromMouseWheel = (e) => {
    let delta = 0;
    if (e.wheelDelta) {
        delta = e.wheelDelta / 120;
        delta = delta > 0 ? Math.ceil(delta) : Math.floor(delta);
    }
    else if (e.detail) {
        delta = Math.round(-e.detail / 3);
    }
    return delta;
};
/**
 * @hidden
 */
const getCaretPosition = (element) => element.selectionStart;
/**
 * @hidden
 */
const extractSignificantNumericChars = (formattedString, separator) => {
    const significantCharacters = `${separator}0123456789-`;
    return formattedString.split('').reduce((acc, curr) => significantCharacters.includes(curr) ? ++acc : acc, 0);
};

/**
 * @hidden
 */
var ArrowDirection;
(function (ArrowDirection) {
    ArrowDirection[ArrowDirection["Down"] = -1] = "Down";
    ArrowDirection[ArrowDirection["None"] = 0] = "None";
    ArrowDirection[ArrowDirection["Up"] = 1] = "Up";
})(ArrowDirection || (ArrowDirection = {}));

const PARSABLE_OPTIONS = ['min', 'max', 'step', 'decimals'];
const PARSABLE_DEFAULTS = {
    decimals: null,
    max: null,
    min: null,
    step: 1
};
const FOCUSED$2 = 'k-state-focused';
const FORMATTED_VALUE = 'k-formatted-value';
/**
 * Represents the [Kendo UI NumericTextBox component for Angular]({% slug overview_numerictextbox %}).
 */
class NumericTextBoxComponent {
    constructor(intl, renderer, localizationService, injector, ngZone, changeDetector, hostElement) {
        this.intl = intl;
        this.renderer = renderer;
        this.localizationService = localizationService;
        this.injector = injector;
        this.ngZone = ngZone;
        this.changeDetector = changeDetector;
        this.hostElement = hostElement;
        /**
         * @hidden
         */
        this.focusableId = `k-${guid()}`;
        /**
         * Determines whether the NumericTextBox is disabled ([see example]({% slug disabled_numerictextbox %})).
         */
        this.disabled = false;
        /**
         * Determines whether the NumericTextBox is in its read-only state ([see example]({% slug readonly_numerictextbox %})).
         */
        this.readonly = false;
        /**
         * Sets the title of the `input` element of the NumericTextBox.
         */
        this.title = '';
        /**
         * Specifies whether the value will be auto-corrected based on the minimum and maximum values
         * ([see example]({% slug precision_numerictextbox %})).
         */
        this.autoCorrect = false;
        /**
         * Specifies the number of decimals that the user can enter when the input is focused
         * ([see example]({% slug precision_numerictextbox %})).
         */
        this.decimals = null;
        /**
         * Specifies the value that is used to increment or decrement the component value
         * ([see example]({% slug predefinedsteps_numerictextbox %})).
         */
        this.step = 1;
        /**
         * Specifies whether the **Up** and **Down** spin buttons will be rendered
         * ([see example]({% slug spinbuttons_numerictextbox %})).
         */
        this.spinners = true;
        /**
         * Determines whether the built-in minimum or maximum validators are enforced when a form is validated.
         *
         * > The 4.2.0 Angular version introduces the `min` and `max` validation directives. As a result, even if you set `rangeValidation`
         * to `false`, the built-in Angular validators will be executed.
         */
        this.rangeValidation = true;
        /**
         * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the component.
         */
        this.tabindex = 0;
        /**
         * Determines whether the whole value will be selected when the NumericTextBox is clicked. Defaults to `true`.
         */
        this.selectOnFocus = true;
        /**
         * Specifies the value of the NumericTextBox
         * ([see example]({% slug formats_numerictextbox %})).
         */
        this.value = null;
        /**
         * Fires each time the user selects a new value ([see example]({% slug overview_numerictextbox %}#toc-events)).
         */
        this.valueChange = new EventEmitter();
        /**
         * Fires each time the user focuses the `input` element ([see example]({% slug overview_numerictextbox %}#toc-events)).
         */
        this.onFocus = new EventEmitter();
        /**
         * Fires each time the `input` element gets blurred ([see example]({% slug overview_numerictextbox %}#toc-events)).
         */
        this.onBlur = new EventEmitter();
        /**
         * @hidden
         */
        this.ArrowDirection = ArrowDirection;
        /**
         * @hidden
         */
        this.arrowDirection = ArrowDirection.None;
        this.inputValue = '';
        this.minValidateFn = noop;
        this.maxValidateFn = noop;
        this._format = "n2";
        this.isPasted = false;
        this.mouseDown = false;
        this.ngChange = noop;
        this.ngTouched = noop;
        this.ngValidatorChange = noop;
        /**
         * @hidden
         */
        this.increasePress = (e) => {
            this.arrowPress(ArrowDirection.Up, e);
        };
        /**
         * @hidden
         */
        this.decreasePress = (e) => {
            this.arrowPress(ArrowDirection.Down, e);
        };
        /**
         * @hidden
         */
        this.releaseArrow = () => {
            clearTimeout(this.spinTimeout);
            if (this.arrowDirection !== ArrowDirection.None) {
                this.arrowDirection = ArrowDirection.None;
                this.changeDetector.detectChanges();
            }
        };
        /**
         * @hidden
         */
        this.handlePaste = () => {
            this.isPasted = true;
        };
        /**
         * @hidden
         */
        this.handleInput = () => {
            const input = this.numericInput.nativeElement;
            let { selectionStart, selectionEnd, value: inputValue } = input;
            if (this.pressedKey === Keys.NumpadDecimal) {
                inputValue = this.replaceNumpadDotValue();
            }
            if (this.isPasted) {
                inputValue = this.formatInputValue(this.intl.parseNumber(inputValue));
            }
            if (!this.isValid(inputValue)) {
                input.value = this.inputValue;
                this.setSelection(selectionStart - 1, selectionEnd - 1);
                return;
            }
            const parsedValue = this.intl.parseNumber(inputValue);
            let value = this.restrictDecimals(parsedValue);
            if (this.autoCorrect) {
                const limited = this.limitInputValue(value);
                value = limited.value;
                selectionStart = limited.selectionStart;
                selectionEnd = limited.selectionEnd;
            }
            if (parsedValue !== value || this.hasTrailingZeros(inputValue) || !this.focused) {
                this.setInputValue(value);
                this.setSelection(selectionStart, selectionEnd);
            }
            else {
                this.inputValue = inputValue;
            }
            if (this.isPasted) {
                input.value = this.inputValue;
            }
            this.updateValue(value);
            this.previousSelection = null;
            this.isPasted = false;
        };
        /**
         * @hidden
         */
        this.handleDragEnter = () => {
            if (!this.focused && !this.isDisabled) {
                this.setInputValue(this.value, true);
            }
        };
        /**
         * @hidden
         */
        this.handleMouseDown = () => {
            this.mouseDown = true;
        };
        /**
         * @hidden
         */
        this.handleFocus = () => {
            if (!this.focused) {
                this.focused = true;
                if (!this.isDisabled) {
                    const shouldSelectAll = this.selectOnFocus || !this.mouseDown;
                    this.ngZone.runOutsideAngular(() => {
                        setTimeout(() => {
                            if (shouldSelectAll) {
                                this.selectAll();
                            }
                            else {
                                this.selectCaret();
                            }
                        }, 0);
                    });
                }
            }
            this.mouseDown = false;
            if (hasObservers(this.onFocus)) {
                this.ngZone.run(() => {
                    this.onFocus.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.handleBlur = () => {
            this.changeDetector.markForCheck();
            this.focused = false;
            //blur is thrown before input when dragging the input text in IE
            if (this.inputValue !== this.elementValue) {
                this.handleInput();
            }
            this.setInputValue();
            if (hasObservers(this.onBlur) || requiresZoneOnBlur(this.control)) {
                this.ngZone.run(() => {
                    this.ngTouched();
                    this.onBlur.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.handleKeyDown = (e) => {
            if (this.isDisabled) {
                return;
            }
            let step;
            if (e.keyCode === Keys.ArrowDown) {
                step = -1;
            }
            else if (e.keyCode === Keys.ArrowUp) {
                step = 1;
            }
            if (step && this.step) {
                e.preventDefault();
                this.addStep(step);
            }
            const input = this.numericInput.nativeElement;
            this.previousSelection = {
                end: input.selectionEnd,
                start: input.selectionStart
            };
            this.pressedKey = e.keyCode;
        };
        /**
         * @hidden
         */
        this.handleWheel = (e) => {
            if (this.focused && !this.isDisabled) {
                e.preventDefault();
                const delta = getDeltaFromMouseWheel(e);
                this.addStep(delta);
            }
        };
        this.direction = localizationService.rtl ? 'rtl' : 'ltr';
    }
    /**
     * Specifies the number format which is used when the NumericTextBox is not focused
     * ([see example]({% slug formats_numerictextbox %})).
     * If `format` is set to `null` or `undefined`, the default format will be used.
     */
    get format() {
        const format = this._format;
        return format !== null && format !== undefined ? format : 'n2';
    }
    set format(value) {
        this._format = value;
    }
    /**
     * @hidden
     */
    set tabIndex(tabIndex) {
        this.tabindex = tabIndex;
    }
    get tabIndex() {
        return this.tabindex;
    }
    get widgetClasses() {
        return true;
    }
    ngOnInit() {
        this.subscriptions = this.localizationService
            .changes
            .subscribe(({ rtl }) => {
            this.direction = rtl ? 'rtl' : 'ltr';
        });
        this.subscriptions.add(this.intl.changes.subscribe(this.intlChange.bind(this)));
        if (this.hostElement) {
            this.renderer.removeAttribute(this.hostElement.nativeElement, "tabindex");
        }
        this.control = this.injector.get(NgControl, null);
    }
    /**
     * @hidden
     */
    ngOnChanges(changes) {
        if (anyChanged(PARSABLE_OPTIONS, changes, false)) {
            this.parseOptions(PARSABLE_OPTIONS.filter(option => changes[option]));
        }
        this.verifySettings();
        if (anyChanged(['min', 'max', 'rangeValidation'], changes, false)) {
            this.minValidateFn = this.rangeValidation ? createMinValidator(this.min) : noop;
            this.maxValidateFn = this.rangeValidation ? createMaxValidator(this.max) : noop;
            this.ngValidatorChange();
        }
        if (anyChanged(['autoCorrect', 'decimals', 'min'], changes)) {
            delete this.numericRegex;
        }
        if (anyChanged(['value', 'format'], changes, false)) {
            this.verifyValue(this.value);
            this.value = this.restrictModelValue(this.value);
            if (!this.focused || (this.intl.parseNumber(this.elementValue) !== this.value)) {
                this.setInputValue();
            }
        }
    }
    /**
     * @hidden
     */
    ngOnDestroy() {
        if (this.subscriptions) {
            this.subscriptions.unsubscribe();
        }
        clearTimeout(this.spinTimeout);
    }
    /**
     * @hidden
     */
    validate(control) {
        return this.minValidateFn(control) || this.maxValidateFn(control);
    }
    /**
     * @hidden
     */
    registerOnValidatorChange(fn) {
        this.ngValidatorChange = fn;
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.verifyValue(value);
        let restrictedValue = this.restrictModelValue(value);
        this.value = restrictedValue;
        this.setInputValue();
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.ngChange = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.ngTouched = fn;
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
    /**
     * Focuses the NumericTextBox.
     *
     * @example
     * ```ts-no-run
     * _@Component({
     * selector: 'my-app',
     * template: `
     *  <button (click)="numerictextbox.focus()">Focus NumericTextBox</button>
     *  <kendo-numerictextbox #numerictextbox></kendo-numerictextbox>
     * `
     * })
     * class AppComponent { }
     * ```
     */
    focus() {
        invokeElementMethod(this.numericInput, 'focus');
    }
    /**
     * Blurs the NumericTextBox.
     */
    blur() {
        invokeElementMethod(this.numericInput, 'blur');
    }
    /**
     * Notifies the `NumericTextBoxComponent` that the input value should be changed.
     * Can be used to update the input after setting the component properties directly.
     */
    notifyValueChange() {
        this.setInputValue();
    }
    /**
     * @hidden
     */
    get incrementTitle() {
        return this.localizationService.get('increment');
    }
    /**
     * @hidden
     */
    get decrementTitle() {
        return this.localizationService.get('decrement');
    }
    get decimalSeparator() {
        const numberSymbols = this.intl.numberSymbols();
        return numberSymbols.decimal;
    }
    get elementValue() {
        return this.numericInput.nativeElement.value;
    }
    set elementValue(value) {
        this.renderer.setProperty(this.numericInput.nativeElement, 'value', value);
    }
    get focused() {
        return this.isFocused;
    }
    get hasDecimals() {
        return this.decimals !== null && this.decimals >= 0;
    }
    set focused(value) {
        if (this.isFocused !== value && this.numericWrap) {
            const wrap = this.numericWrap.nativeElement;
            const input = this.numericInput.nativeElement;
            if (value) {
                this.renderer.addClass(wrap, FOCUSED$2);
                if (!this.isDisabled) {
                    this.renderer.removeClass(input, FORMATTED_VALUE);
                }
            }
            else {
                this.renderer.removeClass(wrap, FOCUSED$2);
                this.renderer.addClass(input, FORMATTED_VALUE);
            }
            this.isFocused = value;
        }
    }
    get isDisabled() {
        return this.disabled || this.readonly;
    }
    arrowPress(direction, e) {
        e.preventDefault();
        if (this.isDisabled) {
            return;
        }
        if (!mobileOS) {
            this.focused = true;
            this.focus();
        }
        if (this.arrowDirection !== direction) {
            this.arrowDirection = direction;
            this.changeDetector.detectChanges();
        }
        if (this.step) {
            this.spin(direction, INITIAL_SPIN_DELAY);
        }
        else {
            this.setInputValue();
        }
    }
    updateValue(value) {
        if (!areSame(this.value, value)) {
            this.ngZone.run(() => {
                this.value = value;
                this.ngChange(value);
                this.valueChange.emit(value);
                this.changeDetector.markForCheck();
            });
        }
    }
    replaceNumpadDotValue() {
        let value = this.inputValue || "";
        if (this.previousSelection) {
            const input = this.numericInput.nativeElement;
            const { selectionStart, selectionEnd } = input;
            const { start, end } = this.previousSelection;
            input.value = value = value.substring(0, start) + this.decimalSeparator + value.substring(end);
            this.setSelection(selectionStart, selectionEnd);
        }
        return value;
    }
    isValid(value) {
        if (!this.numericRegex) {
            this.numericRegex = numericRegex({
                autoCorrect: this.autoCorrect,
                decimals: this.decimals,
                min: this.min,
                separator: this.decimalSeparator
            });
        }
        return this.numericRegex.test(value);
    }
    spin(step, timeout) {
        clearTimeout(this.spinTimeout);
        this.spinTimeout = window.setTimeout(() => {
            this.spin(step, SPIN_DELAY);
        }, timeout);
        this.addStep(step);
    }
    addStep(step) {
        let value = add(this.value || 0, this.step * step);
        value = this.limitValue(value);
        value = this.restrictDecimals(value);
        this.setInputValue(value);
        this.updateValue(value);
    }
    setSelection(start, end) {
        if (this.focused) {
            invokeElementMethod(this.numericInput, 'setSelectionRange', start, end);
        }
    }
    limitValue(value) {
        let result = value;
        if (!this.isInRange(value)) {
            if (isNumber(this.max) && value > this.max) {
                result = this.max;
            }
            if (isNumber(this.min) && value < this.min) {
                result = this.min;
            }
        }
        return result;
    }
    limitInputValue(value) {
        let { selectionStart, selectionEnd, value: enteredValue } = this.numericInput.nativeElement;
        let limitedValue = value;
        let selectToEnd = false;
        if (!this.isInRange(value)) {
            const lengthChange = enteredValue.length - String(this.inputValue).length;
            const { min, max } = this;
            const hasMax = isNumber(max);
            const hasMin = isNumber(min);
            let padLimit, replaceNext;
            let correctedValue = value;
            if (selectionStart === 0 && this.inputValue.substr(1) === enteredValue) {
                return {
                    selectionEnd: selectionEnd,
                    selectionStart: selectionStart,
                    value: null
                };
            }
            if (hasMax && value > max) {
                if (value > 0) {
                    replaceNext = true;
                }
                else {
                    padLimit = max;
                }
            }
            else if (hasMin && value < min) {
                if (value > 0) {
                    padLimit = min;
                }
                else {
                    replaceNext = true;
                }
            }
            if (padLimit) {
                const paddedValue = this.tryPadValue(value, padLimit);
                if (paddedValue && decimalPart(value) !== decimalPart(padLimit)) {
                    correctedValue = paddedValue;
                    selectToEnd = true;
                }
            }
            else if (replaceNext) {
                if (this.inputValue && selectionStart !== enteredValue.length) {
                    correctedValue = parseFloat(enteredValue.substr(0, selectionStart) +
                        enteredValue.substr(selectionStart + lengthChange));
                }
            }
            limitedValue = this.limitValue(correctedValue);
            selectToEnd = (selectToEnd || limitedValue !== correctedValue) && this.previousSelection &&
                (this.previousSelection.end - this.previousSelection.start + lengthChange) > 0;
        }
        return {
            selectionEnd: selectToEnd ? String(limitedValue).length : selectionEnd,
            selectionStart: selectionStart,
            value: limitedValue
        };
    }
    tryPadValue(value, limit) {
        const limitLength = String(Math.floor(limit)).length;
        const zeroPadded = pad(value, limitLength);
        const zeroPaddedNext = pad(value, limitLength + 1);
        let result;
        if (this.isInRange(zeroPadded)) {
            result = zeroPadded;
        }
        else if (this.isInRange(zeroPaddedNext)) {
            result = zeroPaddedNext;
        }
        return result;
    }
    isInRange(value) {
        return !isNumber(value) || ((!isNumber(this.min) || this.min <= value) && (!isNumber(this.max) || value <= this.max));
    }
    restrictModelValue(value) {
        let result = this.restrictDecimals(value, true);
        if (this.autoCorrect && this.limitValue(result) !== result) {
            result = null;
        }
        return result;
    }
    restrictDecimals(value, round) {
        let result = value;
        if (value && this.hasDecimals) {
            const decimals = this.decimals;
            const stringValue = String(value);
            if (round || EXPONENT_REGEX.test(stringValue)) {
                result = toFixedPrecision(value, decimals);
            }
            else {
                const parts = stringValue.split(POINT);
                let fraction = parts[1];
                if (fraction && fraction.length > decimals) {
                    fraction = fraction.substr(0, decimals);
                    result = parseFloat(`${parts[0]}${POINT}${fraction}`);
                }
            }
        }
        return result;
    }
    formatInputValue(value) {
        let stringValue = String(value);
        const exponentMatch = EXPONENT_REGEX.exec(stringValue);
        if (exponentMatch) {
            stringValue = value.toFixed(limitPrecision(parseInt(exponentMatch[1], 10)));
        }
        return stringValue.replace(POINT, this.decimalSeparator);
    }
    formatValue(value, focused) {
        let formattedValue;
        if (value === null || !defined(value) || value === '') {
            formattedValue = '';
        }
        else if (focused && !this.readonly) {
            formattedValue = this.formatInputValue(value);
        }
        else {
            formattedValue = this.intl.formatNumber(value, this.format);
        }
        return formattedValue;
    }
    setInputValue(value = this.value, focused = this.focused) {
        const formattedValue = this.formatValue(value, focused);
        this.elementValue = formattedValue;
        this.inputValue = formattedValue;
    }
    verifySettings() {
        if (!isDevMode()) {
            return;
        }
        if (this.min !== null && this.max !== null && this.min > this.max) {
            throw new Error(`The max value should be bigger than the min. See ${MIN_DOC_LINK} and ${MAX_DOC_LINK}.`);
        }
    }
    verifyValue(value) {
        if (isDevMode() && value && typeof value !== 'number') {
            throw new Error(`The NumericTextBox component requires value of type Number and ${JSON.stringify(value)} was set.`);
        }
    }
    parseOptions(options) {
        for (let idx = 0; idx < options.length; idx++) {
            const name = options[idx];
            const value = this[name];
            if (typeof value === 'string') {
                const parsed = parseFloat(value);
                const valid = !isNaN(parsed);
                if (isDevMode() && !valid && value !== '') {
                    throw new Error('The NumericTextBox component requires value of type Number or a String representing ' +
                        `a number for the ${name} property and ${JSON.stringify(value)} was set.`);
                }
                this[name] = valid ? parsed : PARSABLE_DEFAULTS[name];
            }
        }
    }
    intlChange() {
        delete this.numericRegex;
        if (this.numericInput && (!this.focused || !this.isValid(this.elementValue))) {
            this.setInputValue();
        }
    }
    hasTrailingZeros(inputValue) {
        if (this.hasDecimals && this.focused) {
            const fraction = inputValue.split(this.decimalSeparator)[1];
            return fraction && fraction.length > this.decimals && fraction.lastIndexOf('0') === fraction.length - 1;
        }
    }
    selectAll() {
        this.setInputValue();
        this.setSelection(0, this.inputValue.length);
    }
    selectCaret() {
        const caretPosition = getCaretPosition(this.numericInput.nativeElement);
        const formattedValue = this.elementValue;
        const partialValue = formattedValue.substring(0, caretPosition);
        this.setInputValue();
        if (partialValue.length) {
            const significantCharsInFormattedValue = extractSignificantNumericChars(partialValue, this.decimalSeparator);
            const adjustedSignificantChars = this.adjustSignificantChars(formattedValue, significantCharsInFormattedValue);
            this.setSelection(adjustedSignificantChars, adjustedSignificantChars);
        }
        else {
            this.setSelection(0, 0);
        }
    }
    numberOfLeadingZeroes(formattedValue) {
        const separatorIndex = formattedValue.indexOf(this.decimalSeparator);
        const matchedLeadingZeroes = formattedValue.match(/^[^1-9]*?(0+)/);
        if (matchedLeadingZeroes) {
            const lengthOfMatch = matchedLeadingZeroes[0].length;
            const lengthOfLeadingZeroesMatch = matchedLeadingZeroes[1].length;
            return lengthOfMatch === separatorIndex ? lengthOfLeadingZeroesMatch - 1 : lengthOfLeadingZeroesMatch;
        }
        return 0;
    }
    adjustSignificantChars(formattedValue, significantChars) {
        const leadingZeroes = this.numberOfLeadingZeroes(formattedValue);
        if (leadingZeroes > 0) {
            return Math.max(0, significantChars - leadingZeroes);
        }
        return significantChars;
    }
}
NumericTextBoxComponent.decorators = [
    { type: Component, args: [{
                exportAs: 'kendoNumericTextBox',
                providers: [
                    LocalizationService,
                    { provide: L10N_PREFIX, useValue: 'kendo.numerictextbox' },
                    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => NumericTextBoxComponent), multi: true },
                    { provide: NG_VALIDATORS, useExisting: forwardRef(() => NumericTextBoxComponent), multi: true },
                    { provide: KendoInput, useExisting: forwardRef(() => NumericTextBoxComponent) }
                ],
                selector: 'kendo-numerictextbox',
                template: `
        <ng-container kendoNumericTextBoxLocalizedMessages
            i18n-increment="kendo.numerictextbox.increment|The title for the **Increment** button in the NumericTextBox"
            increment="Increase value"
            i18n-decrement="kendo.numerictextbox.decrement|The title for the **Decrement** button in the NumericTextBox"
            decrement="Decrease value"
        >
        </ng-container>
        <span
            class="k-numeric-wrap"
            [class.k-state-disabled]="disabled"
            [kendoEventsOutsideAngular]="{ mousewheel: handleWheel, DOMMouseScroll: handleWheel }"
            #numericWrap>
            <input
            role="spinbutton"
            class="k-input k-formatted-value"
            autocomplete="off"
            autocorrect="off"
            [id]="focusableId"
            [attr.aria-valuemin]="min"
            [attr.aria-valuemax]="max"
            [attr.aria-valuenow]="value"
            [attr.title]="title"
            [attr.placeholder]="placeholder"
            [tabindex]="tabIndex"
            [disabled]="disabled"
            [readonly]="readonly"
            [kendoEventsOutsideAngular]="{
                mousedown: handleMouseDown,
                dragenter: handleDragEnter,
                keydown: handleKeyDown,
                input: handleInput,
                focus: handleFocus,
                blur: handleBlur,
                paste: handlePaste
            }"
            #numericInput />
            <span class="k-select" *ngIf="spinners" [kendoEventsOutsideAngular]="{ mouseup: releaseArrow, mouseleave: releaseArrow }">
                <span
                    [kendoEventsOutsideAngular]="{ mousedown: increasePress }"
                    [attr.aria-label]="incrementTitle"
                    [title]="incrementTitle"
                    [class.k-state-active]="arrowDirection === ArrowDirection.Up"
                    class="k-link k-link-increase"
                >
                    <span class="k-icon k-i-arrow-n"></span>
                </span>
                <span
                    [kendoEventsOutsideAngular]="{ mousedown: decreasePress }"
                    [attr.aria-label]="decrementTitle"
                    [title]="decrementTitle"
                    [class.k-state-active]="arrowDirection === ArrowDirection.Down"
                    class="k-link k-link-decrease"
                >
                    <span class="k-icon k-i-arrow-s"></span>
                </span>
            </span>
        </span>
      `
            },] },
];
/** @nocollapse */
NumericTextBoxComponent.ctorParameters = () => [
    { type: IntlService },
    { type: Renderer2 },
    { type: LocalizationService },
    { type: Injector },
    { type: NgZone },
    { type: ChangeDetectorRef },
    { type: ElementRef }
];
NumericTextBoxComponent.propDecorators = {
    focusableId: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    title: [{ type: Input }],
    autoCorrect: [{ type: Input }],
    format: [{ type: Input }],
    max: [{ type: Input }],
    min: [{ type: Input }],
    decimals: [{ type: Input }],
    placeholder: [{ type: Input }],
    step: [{ type: Input }],
    spinners: [{ type: Input }],
    rangeValidation: [{ type: Input }],
    tabindex: [{ type: Input }],
    tabIndex: [{ type: Input }],
    selectOnFocus: [{ type: Input }],
    value: [{ type: Input }],
    valueChange: [{ type: Output }],
    onFocus: [{ type: Output, args: ['focus',] }],
    onBlur: [{ type: Output, args: ['blur',] }],
    numericInput: [{ type: ViewChild, args: ['numericInput', { static: true },] }],
    numericWrap: [{ type: ViewChild, args: ['numericWrap',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    widgetClasses: [{ type: HostBinding, args: ['class.k-widget',] }, { type: HostBinding, args: ['class.k-numerictextbox',] }]
};

/**
 * @hidden
 */
class NumericTextBoxMessages extends ComponentMessages {
}
NumericTextBoxMessages.propDecorators = {
    decrement: [{ type: Input }],
    increment: [{ type: Input }]
};

/**
 * Custom component messages override default component messages.
 */
class NumericTextBoxCustomMessagesComponent extends NumericTextBoxMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
NumericTextBoxCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: NumericTextBoxMessages,
                        useExisting: forwardRef(() => NumericTextBoxCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-numerictextbox-messages',
                template: ``
            },] },
];
/** @nocollapse */
NumericTextBoxCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * @hidden
 */
var ResultType;
(function (ResultType) {
    ResultType[ResultType["Literal"] = 0] = "Literal";
    ResultType[ResultType["Mask"] = 1] = "Mask";
    ResultType[ResultType["Undefined"] = 2] = "Undefined";
})(ResultType || (ResultType = {}));
/**
 * @hidden
 */
class Result {
    constructor(value, rest, type = ResultType.Undefined) {
        this.value = value;
        this.rest = rest;
        this.type = type;
    }
    //map :: Functor f => f a ~> (a -> b) -> f b
    map(fn) {
        return new Result(fn(this.value), this.rest);
    }
    //chain :: Chain m => m a ~> (a -> m b) -> m b
    chain(fn) {
        return fn(this.value, this.rest);
    }
    fold(s, _ /*we don't need it*/) {
        return s(this.value, this.rest);
    }
    concat(r) {
        return this.map((vs, _) => r.chain((v, __) => vs.concat([v])));
    }
    toString() {
        return `Result({ value: '${this.value}', rest: ${this.rest} })`;
    }
}

/**
 * @hidden
 */
class Stream {
    constructor(input = [], control = []) {
        this.input = input;
        this.control = control;
        this.inputCursor = 0;
        this.controlCursor = 0;
    }
    eof() {
        return this.inputCursor >= this.input.length;
    }
    // Get the first value from the input.
    next() {
        return {
            char: this.input[this.inputCursor++],
            control: this.control[this.controlCursor++]
        };
    }
    peek() {
        return {
            char: this.input[this.inputCursor],
            control: this.control[this.controlCursor]
        };
    }
    eat_input() {
        this.inputCursor++;
    }
    eat_control() {
        this.controlCursor++;
    }
    eat() {
        this.inputCursor++;
        this.controlCursor++;
    }
}

const toArray = (value) => (value || '').split('');
const ESCAPE_CHARACTER = '\\';
/**
 * @hidden
 */
class Parser {
    constructor(parse) {
        this.parse = parse;
    }
    run(input, control = '') {
        if (input instanceof Stream) {
            return this.parse(input);
        }
        else {
            return this.parse(new Stream(toArray(input), toArray(control)));
        }
    }
    //map :: Functor f => f a ~> (a -> b) -> f b
    map(f) {
        return new Parser(stream => this.parse(stream).map(f));
    }
    //chain :: Chain m => m a ~> (a -> m b) -> m b
    chain(f) {
        return new Parser(stream => this.parse(stream).chain((v, s) => f(v).run(s)));
    }
    isLiteral(c) {
        return this.run(c).type === ResultType.Literal;
    }
}
/**
 * @hidden
 */
const mask = ({ prompt, promptPlaceholder }) => rule => new Parser(stream => {
    while (!stream.eof()) {
        const { char, control } = stream.peek();
        if (char === control && control === prompt) {
            stream.eat();
            return new Result(prompt, stream, ResultType.Mask);
        }
        if (rule.test(char)) {
            stream.eat();
            return new Result(char, stream, ResultType.Mask);
        }
        if (char === promptPlaceholder) {
            stream.eat();
            return new Result(prompt, stream, ResultType.Mask);
        }
        stream.eat_input();
    }
    stream.eat();
    return new Result(prompt, stream, ResultType.Mask);
});
/**
 * @hidden
 */
const literal = _token => new Parser(stream => {
    //    let {char, control} = stream.peek();
    let char = stream.peek().char;
    if (char === _token) {
        stream.eat();
        return new Result(_token, stream, ResultType.Literal);
    }
    //    if (control === _token) {
    //        while (!stream.eof() && char !== _token) {
    //            stream.eat_input();
    //            char = stream.peek().char;
    //        }
    //    }
    //
    //    if (control !== undefined) {
    //        stream.eat();
    //    }
    return new Result(_token, stream, ResultType.Literal);
});
/**
 * @hidden
 */
const unmask = prompt => rule => new Parser(stream => {
    while (!stream.eof()) {
        const { char, control } = stream.peek();
        if (char === prompt && control === prompt) {
            stream.eat();
            return new Result(char, stream);
        }
        if (rule.test(char)) {
            stream.eat();
            return new Result(char, stream);
        }
        stream.eat_input();
    }
    stream.eat();
    return new Result('', stream);
});
/**
 * @hidden
 */
const unliteral = _token => new Parser(stream => {
    if (stream.eof()) {
        return new Result('', stream);
    }
    const { char } = stream.peek();
    if (char === _token) {
        stream.eat();
    }
    return new Result(_token, stream);
});
/**
 * @hidden
 */
const token = (rules, creator) => new Parser(stream => {
    let { char } = stream.next();
    const rule = rules[char];
    if (char === ESCAPE_CHARACTER) {
        char = stream.next().char;
        return new Result(creator.literal(char), stream);
    }
    if (!rule) {
        return new Result(creator.literal(char), stream);
    }
    return new Result(creator.mask(rule), stream);
});
/**
 * @hidden
 */
const rawMask = ({ prompt, promptPlaceholder }) => new Parser(stream => {
    let { char } = stream.next();
    if (char === prompt) {
        return new Result(promptPlaceholder, stream);
    }
    return new Result(char, stream);
});
/**
 * @hidden
 */
const rawLiteral = includeLiterals => new Parser(stream => {
    let { char } = stream.next();
    if (includeLiterals) {
        return new Result(char, stream);
    }
    return new Result('', stream);
});

/**
 * @hidden
 */
const always = value => new Parser(stream => new Result(value, stream));
/**
 * @hidden
 */
const append = (p1, p2) => p1.chain(vs => p2.map(v => vs.concat([v])));
/**
 * @hidden
 */
const sequence = list => list.reduce((acc, parser) => append(acc, parser), always([]));
/**
 * @hidden
 */
const greedy = parser => new Parser(stream => {
    let result = new Result([], stream);
    while (!stream.eof()) {
        result = result.concat(parser.run(stream));
    }
    return result;
});

/**
 * @hidden
 */
class MaskingService {
    constructor() {
        this.rules = {};
        this.prompt = "_";
        this.mask = "";
        this.promptPlaceholder = " ";
        this.includeLiterals = false;
        this.maskTokens = [];
        this.unmaskTokens = [];
        this.rawTokens = [];
        this.validationTokens = [];
    }
    update({ mask: mask$$1 = '', prompt = '', promptPlaceholder = ' ', rules = {}, includeLiterals = false }) {
        this.mask = mask$$1;
        this.prompt = prompt;
        this.promptPlaceholder = promptPlaceholder;
        this.rules = rules;
        this.includeLiterals = includeLiterals;
        this.tokenize();
    }
    validationValue(maskedValue = '') {
        let value = maskedValue;
        sequence(this.validationTokens)
            .run(maskedValue)
            .fold(unmasked => {
            value = unmasked.join('');
        });
        return value;
    }
    rawValue(maskedValue = '') {
        let value = maskedValue;
        if (!this.rawTokens.length) {
            return value;
        }
        sequence(this.rawTokens)
            .run(maskedValue)
            .fold(unmasked => {
            value = unmasked.join('');
        });
        return value;
    }
    /**
     * @hidden
     */
    maskRaw(rawValue = '') {
        let value = rawValue;
        if (!this.maskTokens.length) {
            return value;
        }
        sequence(this.maskTokens)
            .run(rawValue)
            .fold(masked => {
            value = masked.join('');
        });
        return value;
    }
    maskInput(input, control, splitPoint) {
        if (input.length < control.length) {
            return this.maskRemoved(input, control, splitPoint);
        }
        return this.maskInserted(input, control, splitPoint);
    }
    maskInRange(pasted, oldValue, start, end) {
        let value = '';
        let selection = end;
        const beforeChange = oldValue.split('').slice(0, start);
        const afterChange = oldValue.split('').slice(end);
        sequence(this.maskTokens.slice(start, end))
            .run(pasted)
            .fold(masked => {
            value = beforeChange
                .concat(masked)
                .concat(afterChange)
                .join('');
        });
        return {
            selection,
            value
        };
    }
    maskRemoved(input, control, splitPoint) {
        let value = '';
        let selection = splitPoint;
        const unchanged = input.split('').slice(splitPoint);
        const changed = input.split('').slice(0, splitPoint).join('');
        const take$$1 = this.maskTokens.length - (input.length - splitPoint);
        sequence(this.maskTokens.slice(0, take$$1))
            .run(changed, control)
            .fold(masked => {
            selection = this.adjustPosition(masked, selection);
            value = masked.concat(unchanged).join('');
        });
        return {
            selection,
            value
        };
    }
    adjustPosition(input, selection) {
        const caretChar = input[selection];
        const isLiteral = this.maskTokens[selection].isLiteral(caretChar);
        if (!isLiteral && caretChar !== this.prompt) {
            return selection + 1;
        }
        return selection;
    }
    maskInserted(input, control, splitPoint) {
        let value = '';
        let selection = splitPoint;
        const changed = input.slice(0, splitPoint);
        sequence(this.unmaskTokens)
            .run(changed, control)
            .chain(unmasked => {
            selection = unmasked.join('').length;
            const unchanged = control.slice(selection);
            return sequence(this.maskTokens)
                .run(unmasked.join('') + unchanged, control);
        })
            .fold(masked => {
            value = masked.join('');
        });
        return {
            selection,
            value
        };
    }
    get maskTokenCreator() {
        const { prompt, promptPlaceholder } = this;
        return {
            literal: rule => literal(rule),
            mask: rule => mask({ prompt, promptPlaceholder })(rule)
        };
    }
    get unmaskTokenCreator() {
        return {
            literal: rule => unliteral(rule),
            mask: rule => unmask(this.prompt)(rule)
        };
    }
    get rawTokenCreator() {
        const { prompt, promptPlaceholder, includeLiterals } = this;
        return {
            literal: _ => rawLiteral(includeLiterals),
            mask: _ => rawMask({ prompt, promptPlaceholder })
        };
    }
    get validationTokenCreator() {
        const { prompt } = this;
        return {
            literal: _ => rawLiteral(false),
            mask: _ => rawMask({ prompt, promptPlaceholder: '' })
        };
    }
    tokenize() {
        greedy(token(this.rules, this.maskTokenCreator))
            .run(this.mask)
            .fold((tokens, _) => {
            this.maskTokens = tokens;
        });
        greedy(token(this.rules, this.unmaskTokenCreator))
            .run(this.mask)
            .fold((tokens, _) => {
            this.unmaskTokens = tokens;
        });
        greedy(token(this.rules, this.rawTokenCreator))
            .run(this.mask)
            .fold((tokens, _) => {
            this.rawTokens = tokens;
        });
        greedy(token(this.rules, this.validationTokenCreator))
            .run(this.mask)
            .fold((tokens, _) => {
            this.validationTokens = tokens;
        });
    }
}
MaskingService.decorators = [
    { type: Injectable },
];

const resolvedPromise = Promise.resolve(null);
const FOCUSED$3 = 'k-state-focused';
/**
 * Represents the [Kendo UI MaskedTextBox component for Angular]({% slug overview_maskedtextbox %}).
 *
 * @example
 * ```ts-no-run
 *
 * _@Component({
 *     selector: 'my-app',
 *     template: `
 *      <kendo-maskedtextbox
 *          [mask]="mask"
 *          [value]="value">
 *      </kendo-maskedtextbox>
 *     `
 * })
 *
 * class AppComponent {
 *  public value: string = "9580128055807792";
 *  public mask: string = "0000-0000-0000-0000";
 * }
 * ```
 */
class MaskedTextBoxComponent {
    constructor(service, renderer, hostElement, ngZone, injector, changeDetector, rtl) {
        this.service = service;
        this.renderer = renderer;
        this.hostElement = hostElement;
        this.ngZone = ngZone;
        this.injector = injector;
        this.changeDetector = changeDetector;
        /**
         * @hidden
         */
        this.focusableId = `k-${guid()}`;
        /**
         * Determines whether the MaskedTextBox is disabled ([see example]({% slug disabled_maskedtextbox %})).
         */
        this.disabled = false;
        /**
         * Determines whether the MaskedTextBox is in its read-only state ([see example]({% slug readonly_maskedtextbox %})).
         */
        this.readonly = false;
        this.hostClasses = true;
        /**
         * Represents a prompt character for the masked value.
         * @default `_`
         */
        this.prompt = '_';
        /**
         * Indicates a character which represents an empty position in the raw value.
         * @default ' '
         */
        this.promptPlaceholder = ' ';
        /**
         * Indicates whether to include literals in the raw value  ([see example]({% slug value_maskedtextbox %})).
         * @default false
         */
        this.includeLiterals = false;
        /**
         * Specifies if the mask should be shown on focus for empty value.
         */
        this.maskOnFocus = false;
        /**
         * Determines whether the built-in mask validator is enforced when a form is validated
         * ([see example]({% slug validation_maskedtextbox %})).
         * @default true
         */
        this.maskValidation = true;
        /**
         * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the component.
         */
        this.tabindex = 0;
        /**
         * Fires each time the user focuses the `input` element.
         *
         * > To wire the event programmatically, use the `onFocus` property.
         *
         * @example
         * ```ts-no-run
         * _@Component({
         * selector: 'my-app',
         * template: `
         *  <kendo-maskedtextbox (focus)="handleFocus()"></kendo-maskedtextbox>
         * `
         * })
         * class AppComponent {
         *   public handleFocus(): void {
         *      console.log("Component is focused");
         *   }
         * }
         * ```
         */
        this.onFocus = new EventEmitter(); //tslint:disable-line:no-output-rename
        /**
         * Fires each time the `input` element gets blurred.
         *
         * > To wire the event programmatically, use the `onBlur` property.
         *
         * @example
         * ```ts-no-run
         * _@Component({
         * selector: 'my-app',
         * template: `
         *  <kendo-maskedtextbox (blur)="handleBlur()"></kendo-maskedtextbox>
         * `
         * })
         * class AppComponent {
         *   public handleBlur(): void {
         *      console.log("Component is blurred");
         *   }
         * }
         * ```
         */
        this.onBlur = new EventEmitter(); //tslint:disable-line:no-output-rename
        /**
         * Fires each time the value changes.
         */
        this.valueChange = new EventEmitter();
        this.focusClick = false;
        this.defaultRules = {
            "#": /[\d\s\+\-]/,
            "&": /[\S]/,
            "0": /[\d]/,
            "9": /[\d\s]/,
            "?": /[a-zA-Z\s]/,
            "A": /[a-zA-Z0-9]/,
            "C": /./,
            "L": /[a-zA-Z]/,
            "a": /[a-zA-Z0-9\s]/
        };
        this.isPasted = false;
        this.selection = [0, 0];
        /**
         * @hidden
         */
        this.handleFocus = () => {
            this.focused = true;
            if (this.maskOnFocus && this.emptyMask) {
                this.updateInput(this.service.maskRaw(this.value));
                this.ngZone.runOutsideAngular(() => {
                    setTimeout(() => { this.setSelection(0, 0); }, 0);
                });
            }
            if (hasObservers(this.onFocus)) {
                this.ngZone.run(() => {
                    this.onFocus.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.handleClick = () => {
            if (this.focused && !this.focusClick) {
                this.focusClick = true;
                const { selectionStart, selectionEnd } = this.input.nativeElement;
                if (selectionStart === selectionEnd) {
                    this.setFocusSelection();
                }
            }
        };
        /**
         * @hidden
         */
        this.handleBlur = () => {
            this.changeDetector.markForCheck();
            this.focused = false;
            this.focusClick = false;
            if (this.maskOnFocus && this.emptyMask) {
                this.updateInput(this.maskedValue);
            }
            if (hasObservers(this.onBlur) || requiresZoneOnBlur(this.control)) {
                this.ngZone.run(() => {
                    this.onTouched();
                    this.onBlur.emit();
                });
            }
        };
        this.onChange = (_) => { };
        this.onTouched = () => { };
        this.direction = rtl ? 'rtl' : 'ltr';
        this.updateService();
    }
    get hostDisabledClass() {
        return this.disabled;
    }
    /**
     * Exposes the RegExp-based mask validation array ([see example]({% slug masks_maskedtextbox %})).
     */
    get rules() {
        return this._rules || this.defaultRules;
    }
    set rules(value) {
        this._rules = Object.assign({}, this.defaultRules, value);
    }
    /**
     * @hidden
     */
    set tabIndex(tabIndex) {
        this.tabindex = tabIndex;
    }
    get tabIndex() {
        return this.tabindex;
    }
    ngOnInit() {
        if (this.hostElement) {
            this.renderer.removeAttribute(this.hostElement.nativeElement, "tabindex");
        }
        this.control = this.injector.get(NgControl, null);
    }
    /**
     * @hidden
     * Used by the TextBoxContainer to determine if the MaskedTextBox is empty.
     */
    isEmpty() {
        if (this.input) {
            return !Boolean(this.input.nativeElement.value);
        }
    }
    /**
     * @hidden
     */
    handleDragDrop() {
        return false;
    }
    /**
     * Focuses the MaskedTextBox.
     *
     * @example
     * ```ts-no-run
     * _@Component({
     * selector: 'my-app',
     * template: `
     *  <button (click)="maskedinput.focus()">Focus the input</button>
     *  <kendo-maskedtextbox #maskedinput></kendo-maskedtextbox>
     * `
     * })
     * class AppComponent { }
     * ```
     */
    focus() {
        if (!this.input) {
            return;
        }
        this.input.nativeElement.focus();
        this.setFocusSelection();
    }
    /**
     * Blurs the MaskedTextBox.
     */
    blur() {
        if (!this.input) {
            return;
        }
        this.input.nativeElement.blur();
    }
    /**
     * @hidden
     */
    pasteHandler(e) {
        const { selectionStart, selectionEnd } = e.target;
        if (selectionEnd === selectionStart) {
            return;
        }
        this.isPasted = true;
        this.selection = [selectionStart, selectionEnd];
    }
    /**
     * @hidden
     */
    inputHandler(e) {
        const value = e.target.value;
        const [start, end] = this.selection;
        if (!this.mask) {
            this.updateValue(value);
            this.isPasted = false;
            return;
        }
        let result;
        if (this.isPasted) {
            this.isPasted = false;
            const rightPart = this.maskedValue.length - end;
            const to = value.length - rightPart;
            result = this.service.maskInRange(value.slice(start, to), this.maskedValue, start, end);
        }
        else {
            result = this.service.maskInput(value, this.maskedValue, e.target.selectionStart);
        }
        this.updateInput(result.value, result.selection);
        this.updateValue(result.value);
    }
    /**
     * @hidden
     */
    ngOnChanges(changes) {
        if (changes.value) {
            this.value = this.normalizeValue();
        }
        if (!this.mask) {
            this.updateInput(this.value);
            return;
        }
        const next = this.extractChanges(changes);
        this.updateService(next);
        if (isChanged('value', changes)) {
            const maskedValue = this.service.maskRaw(this.value);
            if (maskedValue !== this.maskedValue) {
                this.updateInput(maskedValue);
            }
        }
        else if (anyChanged(['promptPlaceholder', 'includeLiterals'], changes)) {
            resolvedPromise.then(() => {
                this.updateValue(this.maskedValue);
            });
        }
        else {
            this.updateInput(this.service.maskRaw(this.value));
        }
    }
    /**
     * @hidden
     * Writes a new value to the element.
     */
    writeValue(value) {
        this.value = this.normalizeValue(value);
        this.updateInput(this.service.maskRaw(this.value));
    }
    /**
     * @hidden
     * Sets the function that will be called when a `change` event is triggered.
     */
    registerOnChange(fn) {
        this.onChange = fn;
    }
    /**
     * @hidden
     * Sets the function that will be called when a `touch` event is triggered.
     */
    registerOnTouched(fn) {
        this.onTouched = fn;
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
    /**
     * @hidden
     */
    validate(_) {
        if (this.maskValidation === false || !this.mask) {
            return null;
        }
        if (!this.service.validationValue(this.maskedValue)) {
            return null;
        }
        if (this.maskedValue.indexOf(this.prompt) !== -1) {
            return {
                patternError: {
                    mask: this.mask,
                    maskedValue: this.maskedValue,
                    value: this.value
                }
            };
        }
        return null;
    }
    /**
     * @hidden
     */
    updateValue(maskedValue) {
        if (this.mask && !this.service.validationValue(maskedValue)) {
            this.value = '';
        }
        else {
            this.value = this.service.rawValue(maskedValue);
        }
        this.onChange(this.value);
        this.valueChange.emit(this.value);
    }
    updateInput(maskedValue = '', selection) {
        this.maskedValue = maskedValue;
        const value = this.maskOnFocus && !this.focused && this.emptyMask ? '' : maskedValue;
        this.renderer.setProperty(this.input.nativeElement, "value", value);
        if (selection !== undefined) {
            this.setSelection(selection, selection);
        }
    }
    extractChanges(changes) {
        return Object.keys(changes).filter(key => key !== 'rules').reduce((obj, key) => {
            obj[key] = changes[key].currentValue;
            return obj;
        }, {}); // tslint:disable-line:align
    }
    updateService(extra) {
        const config = Object.assign({
            includeLiterals: this.includeLiterals,
            mask: this.mask,
            prompt: this.prompt,
            promptPlaceholder: this.promptPlaceholder,
            rules: this.rules
        }, extra); // tslint:disable-line:align
        this.service.update(config);
    }
    setSelection(start = this.selection[0], end = this.selection[1]) {
        if (this.focused) {
            invokeElementMethod(this.input, 'setSelectionRange', start, end);
        }
    }
    get emptyMask() {
        return this.service.maskRaw() === this.maskedValue;
    }
    setFocusSelection() {
        const selectionStart = this.input.nativeElement.selectionStart;
        const index = this.maskedValue ? this.maskedValue.indexOf(this.prompt) : 0;
        if (index >= 0 && index < selectionStart) {
            this.selection = [index, index];
            this.setSelection();
        }
    }
    get focused() {
        return this.isFocused;
    }
    set focused(value) {
        if (this.isFocused !== value && this.hostElement) {
            const element = this.hostElement.nativeElement;
            if (value) {
                this.renderer.addClass(element, FOCUSED$3);
            }
            else {
                this.renderer.removeClass(element, FOCUSED$3);
            }
            this.isFocused = value;
        }
    }
    normalizeValue(value = this.value) {
        const present = isPresent(value);
        if (present && typeof value !== 'string') {
            if (isDevMode()) {
                throw new Error('The MaskedTextBox component supports only string values.');
            }
            return String(value);
        }
        return present ? value : '';
    }
}
MaskedTextBoxComponent.decorators = [
    { type: Component, args: [{
                exportAs: 'kendoMaskedTextBox',
                providers: [
                    MaskingService,
                    {
                        multi: true,
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => MaskedTextBoxComponent) /* tslint:disable-line */
                    },
                    {
                        multi: true,
                        provide: NG_VALIDATORS,
                        useExisting: forwardRef(() => MaskedTextBoxComponent) /* tslint:disable-line */
                    },
                    {
                        provide: KendoInput,
                        useExisting: forwardRef(() => MaskedTextBoxComponent)
                    }
                ],
                selector: 'kendo-maskedtextbox',
                template: `
        <input type="text"
            #input
            autocomplete="off"
            autocorrect="off"
            autocapitalize="off"
            spellcheck="false"
            class="k-textbox"
            [id]="focusableId"
            [tabindex]="tabIndex"
            [attr.title]="title"
            [disabled]="disabled"
            [readonly]="readonly"
            [kendoEventsOutsideAngular]="{
                focus: handleFocus,
                blur: handleBlur,
                click: handleClick,
                dragstart: handleDragDrop,
                drop: handleDragDrop
            }"
        />
    `
            },] },
];
/** @nocollapse */
MaskedTextBoxComponent.ctorParameters = () => [
    { type: MaskingService },
    { type: Renderer2 },
    { type: ElementRef },
    { type: NgZone },
    { type: Injector },
    { type: ChangeDetectorRef },
    { type: Boolean, decorators: [{ type: Optional }, { type: Inject, args: [RTL,] }] }
];
MaskedTextBoxComponent.propDecorators = {
    focusableId: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    title: [{ type: Input }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    hostClasses: [{ type: HostBinding, args: ['class.k-widget',] }, { type: HostBinding, args: ['class.k-maskedtextbox',] }],
    hostDisabledClass: [{ type: HostBinding, args: ['class.k-state-disabled',] }],
    mask: [{ type: Input }],
    value: [{ type: Input }],
    rules: [{ type: Input }],
    prompt: [{ type: Input }],
    promptPlaceholder: [{ type: Input }],
    includeLiterals: [{ type: Input }],
    maskOnFocus: [{ type: Input }],
    maskValidation: [{ type: Input }],
    tabindex: [{ type: Input }],
    tabIndex: [{ type: Input }],
    onFocus: [{ type: Output, args: ['focus',] }],
    onBlur: [{ type: Output, args: ['blur',] }],
    valueChange: [{ type: Output }],
    input: [{ type: ViewChild, args: ['input', { static: true },] }],
    pasteHandler: [{ type: HostListener, args: ['paste', ['$event'],] }],
    inputHandler: [{ type: HostListener, args: ['input', ['$event'],] }]
};

/* tslint:disable:component-selector */
/**
 * @hidden
 */
class SliderTick {
    constructor(value) {
        this.value = value;
        this.classes = {
            'k-tick': true
        };
    }
}
/**
 * @hidden
 */
class SliderTicksComponent {
    constructor(rtl) {
        this.rtl = rtl;
        this.wrapperClasses = 'k-reset k-slider-items';
        this.ticks = [];
    }
    ngOnChanges(_) {
        this.createTicks();
    }
    createTicks() {
        const count = calculateTicksCount(this.min, this.max, this.step);
        const largeStep = this.largeStep;
        const tickValueProps = {
            max: this.max,
            min: this.min,
            smallStep: this.step
        };
        let result = [];
        for (let i = 0; i < count; i++) {
            result.push(new SliderTick(calculateValueFromTick(i, tickValueProps)));
            if (largeStep && i % largeStep === 0) {
                result[i].large = true;
                result[i].classes['k-tick-large'] = true;
            }
        }
        if (this.rtl || this.vertical) {
            result = result.reverse();
        }
        if (result.length > 0) {
            Object.assign(result[0].classes, this.endTickClasses(true));
            Object.assign(result[result.length - 1].classes, this.endTickClasses(false));
        }
        this.ticks = result;
    }
    endTickClasses(first) {
        return {
            'k-first': (first && !this.vertical) || (!first && this.vertical),
            'k-last': (!first && !this.vertical) || (first && this.vertical)
        };
    }
}
SliderTicksComponent.decorators = [
    { type: Component, args: [{
                selector: '[kendoSliderTicks]',
                template: `
    <li #tickElement *ngFor="let tick of ticks;"
        [ngClass]="tick.classes"
        title="{{ tickTitle(tick.value) }}"
        role="presentation"
     >
         <ng-container [ngSwitch]="tick.large">
            <span class="k-label" *ngSwitchCase="true">
                <ng-container [ngTemplateOutlet]="labelTemplate || defaultLabel" [ngTemplateOutletContext]="tick">
                </ng-container>
            </span>
            <ng-container *ngSwitchCase="false">&nbsp;</ng-container>
         </ng-container>
     </li>

     <ng-template #defaultLabel let-value="value">
        {{ tickTitle(value) }}
     </ng-template>
  `
            },] },
];
/** @nocollapse */
SliderTicksComponent.ctorParameters = () => [
    { type: Boolean, decorators: [{ type: Optional }, { type: Inject, args: [RTL,] }] }
];
SliderTicksComponent.propDecorators = {
    wrapperClasses: [{ type: HostBinding, args: ['class',] }],
    tickTitle: [{ type: Input }],
    vertical: [{ type: Input }],
    step: [{ type: Input }],
    largeStep: [{ type: Input }],
    min: [{ type: Input }],
    max: [{ type: Input }],
    labelTemplate: [{ type: Input }],
    tickElements: [{ type: ViewChildren, args: ['tickElement',] }]
};

/**
 * @hidden
 */
class SlidersCommonModule {
}
SlidersCommonModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    SliderTicksComponent,
                    LabelTemplateDirective
                ],
                exports: [
                    LabelTemplateDirective,
                    SliderTicksComponent,
                    DraggableModule,
                    EventsModule,
                    ResizeSensorModule
                ],
                imports: [CommonModule, DraggableModule, EventsModule, ResizeSensorModule]
            },] },
];

/**
 * @hidden
 */
class SliderMessages extends ComponentMessages {
}
SliderMessages.propDecorators = {
    decrement: [{ type: Input }],
    increment: [{ type: Input }],
    dragHandle: [{ type: Input }]
};

/**
 * @hidden
 */
class LocalizedSliderMessagesDirective extends SliderMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedSliderMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: SliderMessages,
                        useExisting: forwardRef(() => LocalizedSliderMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoSliderLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedSliderMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * Custom component messages override default component messages.
 */
class SliderCustomMessagesComponent extends SliderMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
SliderCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: SliderMessages,
                        useExisting: forwardRef(() => SliderCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-slider-messages',
                template: ``
            },] },
];
/** @nocollapse */
SliderCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the Slider component.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Inputs module
 * import { SliderModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 * import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, BrowserAnimationsModule, SliderModule], // import Slider module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class SliderModule {
}
SliderModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    SliderComponent,
                    SliderCustomMessagesComponent,
                    LocalizedSliderMessagesDirective
                ],
                exports: [
                    SliderComponent,
                    SliderCustomMessagesComponent,
                    LabelTemplateDirective,
                    LocalizedSliderMessagesDirective
                ],
                imports: [CommonModule, SlidersCommonModule]
            },] },
];

/**
 * @hidden
 */
class RangeSliderMessages extends ComponentMessages {
}
RangeSliderMessages.propDecorators = {
    dragHandleStart: [{ type: Input }],
    dragHandleEnd: [{ type: Input }]
};

/**
 * @hidden
 */
class LocalizedRangeSliderMessagesDirective extends RangeSliderMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedRangeSliderMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: RangeSliderMessages,
                        useExisting: forwardRef(() => LocalizedRangeSliderMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoSliderLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedRangeSliderMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * Custom component messages override default component messages.
 */
class RangeSliderCustomMessagesComponent extends RangeSliderMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
RangeSliderCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: RangeSliderMessages,
                        useExisting: forwardRef(() => RangeSliderCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-rangeslider-messages',
                template: ``
            },] },
];
/** @nocollapse */
RangeSliderCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the RangeSlider component.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Inputs module
 * import { RangeSliderModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 * import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, BrowserAnimationsModule, RangeSliderModule], // import RangeSlider module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class RangeSliderModule {
}
RangeSliderModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    RangeSliderComponent,
                    RangeSliderCustomMessagesComponent,
                    LocalizedRangeSliderMessagesDirective
                ],
                exports: [
                    RangeSliderComponent,
                    RangeSliderCustomMessagesComponent,
                    LocalizedRangeSliderMessagesDirective,
                    LabelTemplateDirective
                ],
                imports: [CommonModule, SlidersCommonModule]
            },] },
];

/**
 * @hidden
 */
class Messages extends ComponentMessages {
}
Messages.propDecorators = {
    on: [{ type: Input }],
    off: [{ type: Input }]
};

/**
 * @hidden
 */
class LocalizedSwitchMessagesDirective extends Messages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedSwitchMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: Messages,
                        useExisting: forwardRef(() => LocalizedSwitchMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoSwitchLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedSwitchMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * Custom component messages override default component messages.
 */
class SwitchCustomMessagesComponent extends Messages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
SwitchCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: Messages,
                        useExisting: forwardRef(() => SwitchCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-switch-messages',
                template: ``
            },] },
];
/** @nocollapse */
SwitchCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the Switch component.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Switch module
 * import { SwitchModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, SwitchModule], // import Switch module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class SwitchModule {
}
SwitchModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    SwitchComponent,
                    SwitchCustomMessagesComponent,
                    LocalizedSwitchMessagesDirective
                ],
                exports: [
                    SwitchComponent,
                    SwitchCustomMessagesComponent,
                    LocalizedSwitchMessagesDirective
                ],
                imports: [CommonModule, EventsModule, ResizeSensorModule]
            },] },
];

/**
 * @hidden
 */
class LocalizedNumericTextBoxMessagesDirective extends NumericTextBoxMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedNumericTextBoxMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: NumericTextBoxMessages,
                        useExisting: forwardRef(() => LocalizedNumericTextBoxMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoNumericTextBoxLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedNumericTextBoxMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the NumericTextBox component.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the NumericTextBox module
 * import { NumericTextBoxModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, NumericTextBoxModule], // import NumericTextBox module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class NumericTextBoxModule {
}
NumericTextBoxModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    LocalizedNumericTextBoxMessagesDirective,
                    NumericTextBoxComponent,
                    NumericTextBoxCustomMessagesComponent
                ],
                exports: [
                    NumericTextBoxComponent,
                    NumericTextBoxCustomMessagesComponent
                ],
                imports: [CommonModule, EventsModule]
            },] },
];

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the MaskedTextBox component.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the MaskedTextBox module
 * import { MaskedTextBoxModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, MaskedTextBoxModule], // import MaskedTextBox module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class MaskedTextBoxModule {
}
MaskedTextBoxModule.decorators = [
    { type: NgModule, args: [{
                declarations: [MaskedTextBoxComponent],
                exports: [MaskedTextBoxComponent],
                imports: [CommonModule, EventsModule]
            },] },
];

/**
 * Specifies the adornments in the suffix container ([see examples]({% slug adornments_textbox %}#toc-suffixadornments)).
 *  @example
 * ```ts-no-run
 * _@Component({
 * selector: 'my-app',
 * template: `
 *  <kendo-textbox>
 *    <ng-template kendoTextBoxSuffixTemplate>
 *        <button kendoButton look="clear" icon="image"></button>
 *    </ng-template>
 *  </kendo-textbox>
 * `
 * })
 * class AppComponent {}
 * ```
 */
class TextBoxSuffixTemplateDirective {
    constructor(templateRef) {
        this.templateRef = templateRef;
    }
}
TextBoxSuffixTemplateDirective.decorators = [
    { type: Directive, args: [{
                selector: '[kendoTextBoxSuffixTemplate]'
            },] },
];
/** @nocollapse */
TextBoxSuffixTemplateDirective.ctorParameters = () => [
    { type: TemplateRef, decorators: [{ type: Optional }] }
];

/**
 * Specifies the adornments in the prefix container ([see examples]({% slug adornments_textbox %}#toc-prefixadornments)).
 * @example
 * ```ts-no-run
 * _@Component({
 * selector: 'my-app',
 * template: `
 *  <kendo-textbox>
 *    <ng-template kendoTextBoxPrefixTemplate>
 *        <button kendoButton look="clear" icon="image"></button>
 *    </ng-template>
 *  </kendo-textbox>
 * `
 * })
 * class AppComponent {}
 * ```
 */
class TextBoxPrefixTemplateDirective {
    constructor(templateRef) {
        this.templateRef = templateRef;
    }
}
TextBoxPrefixTemplateDirective.decorators = [
    { type: Directive, args: [{
                selector: '[kendoTextBoxPrefixTemplate]'
            },] },
];
/** @nocollapse */
TextBoxPrefixTemplateDirective.ctorParameters = () => [
    { type: TemplateRef, decorators: [{ type: Optional }] }
];

class TextBoxComponent {
    constructor(localizationService, ngZone, changeDetector, renderer, injector, hostElement) {
        this.localizationService = localizationService;
        this.ngZone = ngZone;
        this.changeDetector = changeDetector;
        this.renderer = renderer;
        this.injector = injector;
        this.hostElement = hostElement;
        /**
         * Sets the `title` attribute of the `input` element of the TextBox.
         */
        this.title = '';
        /**
         * Sets the disabled state of the component.
         *
         * @default false
         */
        this.disabled = false;
        /**
         * Sets the read-only state of the component.
         *
         * @default false
         */
        this.readonly = false;
        /**
         * Specifies the `tabindex` of the TextBox.
         *
         * @default 0
         */
        this.tabindex = 0;
        /**
         * Provides a value for the TextBox.
         */
        this.value = null;
        /**
         * Determines whether the whole value will be selected when the TextBox is clicked. Defaults to `false`.
         *
         * @default false
         */
        this.selectOnFocus = false;
        /**
         * Specifies when the Success icon will be shown ([see example]({% slug validation_textbox %})).
         *
         * The possible values are:
         *
         * `boolean`&mdash;The Success icon is displayed, if the condition given by the developer is met.
         *
         * `initial`&mdash;The Success icon will be displayed when the component state is neither `invalid` nor `touched` or `dirty`.
         *
         * @default false
         */
        this.showSuccessIcon = false;
        /**
         * Specifies when the Error icon will be shown ([see example]({% slug validation_textbox %})).
         *
         * The possible values are:
         *
         * * `initial`&mdash;The Error icon will be displayed when the component state is
         * `invalid` and `touched` or `dirty`.
         * * `boolean`&mdash;The Error icon is displayed, if the condition given by the developer is met.
         *
         * @default false
         */
        this.showErrorIcon = false;
        /**
         * Specifies whether a Clear button will be rendered.
         *
         * @default false
         */
        this.clearButton = false;
        /**
         * Fires each time the value is changed&mdash;
         * when the component is blurred or the value is cleared through the **Clear** button
         * ([see example]({% slug overview_textbox %}#toc-events)).
         * When the value of the component is programmatically changed to `ngModel` or `formControl`
         * through its API or form binding, the `valueChange` event is not triggered because it
         * might cause a mix-up with the built-in `valueChange` mechanisms of the `ngModel` or `formControl` bindings.
         */
        this.valueChange = new EventEmitter();
        /**
         * Fires each time the user focuses the `input` element.
         */
        this.inputFocus = new EventEmitter();
        /**
         * Fires each time the `input` element gets blurred.
         */
        this.inputBlur = new EventEmitter();
        /**
         * Fires each time the user focuses the TextBox component.
         *
         * > To wire the event programmatically, use the `onFocus` property.
         *
         * @example
         * ```ts-no-run
         * _@Component({
         * selector: 'my-app',
         * template: `
         *  <kendo-textbox (focus)="handleFocus()"></kendo-textbox>
         * `
         * })
         * class AppComponent {
         *   public handleFocus(): void {
         *      console.log('Component is isFocused');
         *   }
         * }
         * ```
         */
        this.onFocus = new EventEmitter();
        /**
         * Fires each time the TextBox component gets blurred.
         *
         * > To wire the event programmatically, use the `onBlur` property.
         *
         * @example
         * ```ts-no-run
         * _@Component({
         * selector: 'my-app',
         * template: `
         *  <kendo-textbox (blur)="handleBlur()"></kendo-textbox>
         * `
         * })
         * class AppComponent {
         *   public handleBlur(): void {
         *      console.log('Component is blurred');
         *   }
         * }
         * ```
         */
        this.onBlur = new EventEmitter();
        this.hostClass = true;
        this._isFocused = false;
        this.focusChangedProgrammatically = false;
        /**
         * @hidden
         */
        this.handleInputFocus = () => {
            if (!this.disabled) {
                if (this.selectOnFocus && this.value) {
                    this.ngZone.run(() => {
                        setTimeout(() => { this.selectAll(); });
                    });
                }
                if (hasObservers(this.onFocus)) {
                    if (!this.isFocused) {
                        this.ngZone.run(() => {
                            this.onFocus.emit();
                        });
                    }
                }
                if (hasObservers(this.inputFocus)) {
                    if (!this.focusChangedProgrammatically || (this.focusChangedProgrammatically && this.clearButtonClicked)) {
                        this.ngZone.run(() => {
                            this.inputFocus.emit();
                        });
                    }
                }
                this.ngZone.run(() => {
                    this.isFocused = true;
                });
            }
        };
        /**
         * @hidden
         */
        this.handleInputBlur = () => {
            this.changeDetector.markForCheck();
            if (hasObservers(this.inputBlur) || requiresZoneOnBlur(this.control)) {
                this.ngZone.run(() => {
                    this.ngTouched();
                    this.inputBlur.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.handleInput = (ev) => {
            let incomingValue = ev.target.value;
            this.updateValue(incomingValue);
        };
        this.ngChange = (_) => { };
        this.ngTouched = () => { };
        this.direction = localizationService.rtl ? 'rtl' : 'ltr';
    }
    /**
     * @hidden
     */
    set tabIndex(tabIndex) {
        this.tabindex = tabIndex;
    }
    get tabIndex() {
        return this.tabindex;
    }
    get disabledClass() {
        return this.disabled;
    }
    get isFocused() {
        return this.disabled ? false : this._isFocused;
    }
    set isFocused(isFocused) {
        this._isFocused = isFocused;
    }
    ngOnInit() {
        this.control = this.injector.get(NgControl, null);
        this.checkClearButton();
        this.subscriptions = this.localizationService.changes.subscribe(({ rtl }) => {
            this.direction = rtl ? 'rtl' : 'ltr';
        });
    }
    ngAfterViewInit() {
        const hostElement = this.hostElement.nativeElement;
        let cursorInsideWrapper = false;
        let tabbing = false;
        this.ngZone.runOutsideAngular(() => {
            // focusIn and focusOut are relative to the host element
            this.subscriptions.add(this.renderer.listen(hostElement, 'focusin', () => {
                if (!this.isFocused) {
                    this.ngZone.run(() => {
                        this.onFocus.emit();
                        this.isFocused = true;
                    });
                }
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'focusout', (args) => {
                if (!this.isFocused) {
                    return;
                }
                if (tabbing) {
                    const closestTextbox = closest(args.relatedTarget, (element) => element === this.hostElement.nativeElement);
                    if (!closestTextbox) {
                        this.handleBlur();
                    }
                    tabbing = false;
                }
                else {
                    if (!cursorInsideWrapper && !this.clearButtonClicked) {
                        this.handleBlur();
                    }
                }
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'mouseenter', () => {
                cursorInsideWrapper = true;
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'mouseleave', () => {
                cursorInsideWrapper = false;
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'keydown', (args) => {
                if (args.keyCode === Keys.Tab) {
                    tabbing = true;
                }
                else {
                    tabbing = false;
                }
            }));
        });
    }
    ngOnChanges(changes) {
        if (changes.disabled || changes.readonly) {
            this.checkClearButton();
        }
    }
    ngOnDestroy() {
        this.subscriptions.unsubscribe();
    }
    /**
     * Focuses the TextBox.
     *
     * @example
     * ```ts-no-run
     * _@Component({
     * selector: 'my-app',
     * template: `
     *  <button (click)="input.focus()">Focus the input</button>
     *  <kendo-textbox #input></kendo-textbox>
     * `
     * })
     * class AppComponent { }
     * ```
     */
    focus() {
        if (!this.input) {
            return;
        }
        this.focusChangedProgrammatically = true;
        this.isFocused = true;
        this.input.nativeElement.focus();
        this.focusChangedProgrammatically = false;
    }
    /**
     * Blurs the TextBox.
     */
    blur() {
        this.focusChangedProgrammatically = true;
        const isFocusedElement = this.hostElement.nativeElement.querySelector(':focus');
        if (isFocusedElement) {
            isFocusedElement.blur();
        }
        this.isFocused = false;
        this.focusChangedProgrammatically = false;
    }
    /**
     * @hidden
     */
    clearTitle() {
        return this.localizationService.get('clear');
    }
    /**
     * @hidden
     */
    checkClearButton() {
        this.showClearButton =
            !this.disabled &&
                !this.readonly &&
                this.clearButton &&
                !!this.value;
    }
    /**
     * @hidden
     */
    clearValue(ev) {
        if (ev) {
            ev.preventDefault();
        }
        this.clearButtonClicked = true;
        this.input.nativeElement.value = '';
        this.input.nativeElement.focus();
        this.updateValue(null);
        this.checkClearButton();
        this.clearButtonClicked = false;
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.value = value;
        this.checkClearButton();
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.ngChange = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.ngTouched = fn;
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
    /**
     * @hidden
     */
    showErrorsInitial() {
        if (!this.control) {
            return false;
        }
        const { invalid, dirty, touched } = this.control;
        return invalid && (dirty || touched);
    }
    /**
     * @hidden
     */
    showSuccessInitial() {
        if (!this.control) {
            return false;
        }
        const { valid, dirty, touched } = this.control;
        return valid && (dirty || touched);
    }
    /**
     * @hidden
     */
    get successIconClasses() {
        return this.successIcon
            ? `k-text-success ${this.successIcon}`
            : `k-text-success k-icon k-i-check-outline`;
    }
    /**
     * @hidden
     */
    get errorIconClasses() {
        return this.errorIcon
            ? `k-text-error ${this.errorIcon}`
            : `k-text-error k-icon k-i-warning`;
    }
    /**
     * @hidden
     */
    get clearButtonClasses() {
        return this.clearButtonIcon
            ? this.clearButtonIcon
            : `k-icon k-i-close-circle`;
    }
    /**
     * @hidden
     */
    get hasErrors() {
        return this.showErrorIcon === 'initial'
            ? this.showErrorsInitial()
            : this.showErrorIcon;
    }
    /**
     * @hidden
     */
    get isSuccessful() {
        return this.showSuccessIcon === 'initial'
            ? this.showSuccessInitial()
            : this.showSuccessIcon;
    }
    setSelection(start, end) {
        if (this.isFocused) {
            invokeElementMethod(this.input, 'setSelectionRange', start, end);
        }
    }
    selectAll() {
        if (this.value) {
            this.setSelection(0, this.value.length);
        }
    }
    updateValue(value) {
        if (!areSame(this.value, value)) {
            this.ngZone.run(() => {
                this.value = value;
                this.ngChange(value);
                this.valueChange.emit(value);
                this.checkClearButton();
                this.changeDetector.markForCheck();
            });
        }
    }
    handleBlur() {
        this.ngZone.run(() => {
            if (!this.focusChangedProgrammatically) {
                this.onBlur.emit();
            }
            this.isFocused = false;
        });
    }
}
TextBoxComponent.decorators = [
    { type: Component, args: [{
                exportAs: 'kendoTextBox',
                providers: [
                    LocalizationService,
                    { provide: L10N_PREFIX, useValue: 'kendo.textbox' },
                    {
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => TextBoxComponent),
                        multi: true
                    },
                    { provide: KendoInput, useExisting: forwardRef(() => TextBoxComponent) }
                ],
                selector: 'kendo-textbox',
                template: `
        <ng-container kendoTextBoxLocalizedMessages
            i18n-clear="kendo.textbox.clear|The title for the **Clear** button in the TextBox."
            clear="Clear">
        </ng-container>
        <span class="k-input-prefix">
            <ng-template
                *ngIf="prefixTemplate"
                [ngTemplateOutlet]="prefixTemplate?.templateRef">
            </ng-template>
        </span>
        <input
            class="k-input"
            #input
            [disabled]="disabled"
            [readonly]="readonly"
            [attr.tabindex]="disabled ? undefined : tabindex"
            [value]="value"
            [attr.placeholder]="placeholder"
            [attr.title]="title"
            [kendoEventsOutsideAngular]="{
                focus: handleInputFocus,
                blur: handleInputBlur,
                input: handleInput}"/>
        <span class="k-input-suffix">
            <span *ngIf="hasErrors" [ngClass]="errorIconClasses"></span>
            <span *ngIf="isSuccessful" [ngClass]="successIconClasses"></span>
            <span
                role="button"
                class="k-clear-value"
                *ngIf="showClearButton"
                (click)="clearValue()"
                (mousedown)="$event.preventDefault()"
                [tabindex]="tabIndex"
                [attr.aria-label]="clearTitle()"
                [title]="clearTitle()"
                (keydown.enter)="clearValue($event)"
                (keydown.space)="clearValue($event)"
                >
                <span [ngClass]="clearButtonClasses"></span>
            </span>
            <ng-template
                *ngIf="suffixTemplate"
                [ngTemplateOutlet]="suffixTemplate?.templateRef">
            </ng-template>
        </span>
    `
            },] },
];
/** @nocollapse */
TextBoxComponent.ctorParameters = () => [
    { type: LocalizationService },
    { type: NgZone },
    { type: ChangeDetectorRef },
    { type: Renderer2 },
    { type: Injector },
    { type: ElementRef }
];
TextBoxComponent.propDecorators = {
    title: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    tabindex: [{ type: Input }],
    value: [{ type: Input }],
    selectOnFocus: [{ type: Input }],
    showSuccessIcon: [{ type: Input }],
    showErrorIcon: [{ type: Input }],
    clearButton: [{ type: Input }],
    successIcon: [{ type: Input }],
    errorIcon: [{ type: Input }],
    clearButtonIcon: [{ type: Input }],
    tabIndex: [{ type: Input }],
    placeholder: [{ type: Input }],
    valueChange: [{ type: Output }],
    inputFocus: [{ type: Output }],
    inputBlur: [{ type: Output }],
    onFocus: [{ type: Output, args: ['focus',] }],
    onBlur: [{ type: Output, args: ['blur',] }],
    input: [{ type: ViewChild, args: ['input', { static: true },] }],
    suffixTemplate: [{ type: ContentChild, args: [TextBoxSuffixTemplateDirective,] }],
    prefixTemplate: [{ type: ContentChild, args: [TextBoxPrefixTemplateDirective,] }],
    disabledClass: [{ type: HostBinding, args: ['class.k-state-disabled',] }],
    hostClass: [{ type: HostBinding, args: ['class.k-textbox',] }],
    isFocused: [{ type: HostBinding, args: ['class.k-state-focused',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }]
};

/**
 * Specifies a separator in the content of the TextBox ([see examples]({% slug adornments_textbox %}#toc-separator)).
 * @example
 * ```ts-no-run
 * _@Component({
 * selector: 'my-app',
 * template: `
 *  <kendo-textbox>
 *    <ng-template kendoTextBoxSuffixTemplate>
 *        <button kendoButton look="clear" icon="image"></button>
 *        <kendo-textbox-separator></kendo-textbox-separator>
 *    </ng-template>
 *  </kendo-textbox>
 * `
 * })
 * class AppComponent {}
 * ```
 */
class TextBoxSeparatorComponent {
    constructor() {
        this.hostClass = true;
    }
}
TextBoxSeparatorComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-textbox-separator',
                template: ``
            },] },
];
TextBoxSeparatorComponent.propDecorators = {
    hostClass: [{ type: HostBinding, args: ['class.k-textbox-separator',] }]
};

/**
 * @hidden
 */
class TextBoxMessages extends ComponentMessages {
}
TextBoxMessages.propDecorators = {
    clear: [{ type: Input }]
};

/**
 * Custom component messages override default component messages.
 */
class TextBoxCustomMessagesComponent extends TextBoxMessages {
    constructor(service) {
        super();
        this.service = service;
    }
    get override() {
        return true;
    }
}
TextBoxCustomMessagesComponent.decorators = [
    { type: Component, args: [{
                providers: [
                    {
                        provide: TextBoxMessages,
                        useExisting: forwardRef(() => TextBoxCustomMessagesComponent) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: 'kendo-textbox-messages',
                template: ``
            },] },
];
/** @nocollapse */
TextBoxCustomMessagesComponent.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * @hidden
 */
class LocalizedTextBoxMessagesDirective extends TextBoxMessages {
    constructor(service) {
        super();
        this.service = service;
    }
}
LocalizedTextBoxMessagesDirective.decorators = [
    { type: Directive, args: [{
                providers: [
                    {
                        provide: TextBoxMessages,
                        useExisting: forwardRef(() => LocalizedTextBoxMessagesDirective) // tslint:disable-line:no-forward-ref
                    }
                ],
                selector: '[kendoTextBoxLocalizedMessages]'
            },] },
];
/** @nocollapse */
LocalizedTextBoxMessagesDirective.ctorParameters = () => [
    { type: LocalizationService }
];

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the TextBox directive.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the TextBox module
 * import { TextBoxModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, TextBoxModule], // import TextBox module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class TextBoxModule {
}
TextBoxModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    TextBoxDirective,
                    TextAreaDirective,
                    TextBoxContainerComponent,
                    TextBoxComponent,
                    TextBoxSeparatorComponent,
                    TextBoxSuffixTemplateDirective,
                    TextBoxPrefixTemplateDirective,
                    TextBoxCustomMessagesComponent,
                    LocalizedTextBoxMessagesDirective
                ],
                exports: [
                    TextBoxDirective,
                    TextAreaDirective,
                    TextBoxContainerComponent,
                    TextBoxComponent,
                    TextBoxSeparatorComponent,
                    TextBoxSuffixTemplateDirective,
                    TextBoxPrefixTemplateDirective,
                    EventsModule,
                    TextBoxCustomMessagesComponent,
                    LocalizedTextBoxMessagesDirective
                ],
                imports: [CommonModule, EventsModule]
            },] },
];

/**
 * @hidden
 */
class PreventableEvent {
    constructor() {
        this.prevented = false;
    }
    /**
     * Prevents the default action for a specified event.
     * In this way, the source component suppresses the built-in behavior that follows the event.
     */
    preventDefault() {
        this.prevented = true;
    }
    /**
     * If the event is prevented by any of its subscribers, returns `true`.
     *
     * @returns - `true` if the default action was prevented. Otherwise, returns `false`.
     */
    isDefaultPrevented() {
        return this.prevented;
    }
}

/**
 * Fires each time the left side of the ColorPicker wrapper is clicked.
 * The event is triggered regardless of whether a ColorPicker icon is set or not.
 *
 * Provides information about the current active color and gives the option to prevent the opening of the popup.
 *
 * @example
 *
 * ```ts-no-run
 * _@Component({
 *   selector: 'my-app',
 *   template: `
 *       <kendo-colorpicker
 *           [icon]="'edit-tools'"
 *           [value]="'#900'"
 *           (activeColorClick)="handleActiveColorClick($event)"
 *       >
 *       </kendo-colorpicker>
 *   `
 * })
 * class AppComponent {
 *     public handleActiveColorClick(event: ActiveColorClickEvent): void {
 *         event.preventOpen();
 *
 *         console.log('Open prevented:', event.isOpenPrevented());
 *         console.log('Current color:', event.color);
 *     }
 *  }
 * ```
 */
class ActiveColorClickEvent {
    /**
     * @hidden
     * @param color Represents the current value of the ColorPicker.
     */
    constructor(color) {
        this.color = color;
        this.openPrevented = false;
    }
    /**
     * Prevents the opening of the popup.
     */
    preventOpen() {
        this.openPrevented = true;
    }
    /**
     * Returns `true` if the popup opening is prevented by any of its subscribers.
     *
     * @returns - Returns `true` if the open action was prevented. Otherwise, returns `false`.
     */
    isOpenPrevented() {
        return this.openPrevented;
    }
}

/**
 * @hidden
 *
 * Returns the hex or rgba string representation of the color.
 */
const parseColor$1 = (value, format, safe = true) => {
    const allowedFormats = ['hex', 'rgba', 'name'];
    if (allowedFormats.indexOf(format) === -1) {
        throw new Error(`Unsupported color output format '${format}'. The available options are 'hex', 'rgba' or 'name'.`);
    }
    if (!isPresent(value)) {
        return;
    }
    if (format === 'name') {
        return nameFormat(value, safe);
    }
    const parsedColor = parseColor(value.trim(), safe);
    if (!isPresent(parsedColor)) {
        return;
    }
    return format === 'hex' ? parsedColor.toCss() : parsedColor.toCssRgba();
};
/**
 * @hidden
 *
 * Returns an HSV object representation of the color string.
 */
const getHSV = (value, safe = true) => {
    const parsed = parseColor(value, safe);
    if (!isPresent(parsed)) {
        return {};
    }
    return parsed.toHSV();
};
/**
 * @hidden
 *
 * Returns an RGBA object representation of the color string.
 */
const getRGBA = (value, safe = true) => {
    const parsed = parseColor(value, safe);
    if (!isPresent(parsed)) {
        return {};
    }
    return parsed.toBytes();
};
/**
 * @hidden
 *
 * Returns the RGBA string representation of the color.
 */
const getColorFromHSV = (hsva) => {
    const hue = fitIntoBounds(hsva.h, 0, 359.9);
    const saturation = fitIntoBounds(hsva.s, 0, 1);
    const value = fitIntoBounds(hsva.v, 0, 1);
    const alpha = fitIntoBounds(hsva.a, 0, 1);
    return Color.fromHSV(hue, saturation, value, alpha).toCssRgba();
};
/**
 * @hidden
 *
 * Returns the RGBA string representation of the color based on the `hue`, assuming the `value`, `saturation` and `alpha` have value of `1`.
 */
const getColorFromHue = (hue) => {
    return getColorFromHSV({ h: hue, s: 1, v: 1, a: 1 });
};
/**
 * @hidden
 *
 * Returns the RGBA string representation of the color.
 */
const getColorFromRGBA = (rgba) => {
    const red = fitIntoBounds(rgba.r, 0, 255);
    const green = fitIntoBounds(rgba.g, 0, 255);
    const blue = fitIntoBounds(rgba.b, 0, 255);
    const alpha = fitIntoBounds(rgba.a, 0, 1);
    return Color.fromBytes(red, green, blue, alpha).toCssRgba();
};
/**
 *
 * @hidden
 */
function nameFormat(value, safe) {
    value = value.toLowerCase().trim();
    if (isPresent(namedColors[value])) {
        return value;
    }
    if (parseColor(value, safe)) {
        value = parseColor(value, safe).toHex();
    }
    const key = Object.keys(namedColors).find(key => namedColors[key] === value);
    if (!key && !safe) {
        throw new Error(`The provided color ${value} is not supported for 'format="name"' property.To display ${value} color, the component 'format' property shoud be set to 'hex' or 'rgba' `);
    }
    return key;
}

// tslint:disable:max-line-length
/**
 * @hidden
 */
const PALETTEPRESETS = {
    basic: {
        colors: '000000,7f7f7f,880015,ed1c24,ff7f27,fff200,22b14c,00a2e8,3f48cc,a349a4,ffffff,c3c3c3,b97a57,ffaec9,ffc90e,efe4b0,b5e61d,99d9ea,7092be,c8bfe7',
        columns: 10
    },
    office: {
        colors: 'ffffff, 000000, e6e6e6, 435569, 4371c4, ed7e32, a5a4a5, febf04, 5a9bd5, 71ae48, f2f2f3, 7f7f7f, d1cece, d5dde3, dae1f4, fce5d4, deeded, fff2cc, deeaf6, e1efd9, d7d8d8, 585959, aeabab, adbaca, b4c5e7, f6caac, dbdbdb, ffe498, bcd6ee, c5e0b2, bfbfc0, 3f3f3f, 767070, 8595b1, 8fabdb, f5b183, c9c8c9, fed965, 9bc4e5, a8d08d, a5a5a6, 262625, 393939, 334050, 2e5496, c45a11, 7b7b7a, bf9000, 2f75b5, 548235, 7f7f7f, 0b0c0c, 161616, 222a34, 203764, 843d0b, 525252, 7f6000, 1d4d79, 375623',
        columns: 10
    },
    apex: {
        colors: 'ffffff, 000000, c9c2d1, 69676d, ceb966, 9cb084, 6bb1c9, 6585cf, 7e6bc9, a379bb, f2f2f2, 7f7f7f, f4f2f5, e0e0e2, f5f1e0, ebefe6, e1eff4, e0e6f5, e5e1f4, ece4f1, d8d8d8, 595959, e9e6ec, c2c1c5, ebe3c1, d7dfcd, c3dfe9, c1ceeb, cbc3e9, dac9e3, bfbfbf, 3f3f3f, dedae3, a4a3a8, e1d5a3, c3cfb5, a6d0de, a2b5e2, b1a6de, c7aed6, a5a5a5, 262626, 9688a5, 4e4d51, ae9638, 758c5a, 3d8da9, 365bb0, 533da9, 7d4d99, 7f7f7f, 0c0c0c, 635672, 343336, 746425, 4e5d3c, 295e70, 243c75, 372970, 533366',
        columns: 10
    },
    austin: {
        colors: 'ffffff, 000000, caf278, 3e3d2d, 94c600, 71685a, ff6700, 909465, 956b43, fea022, f2f2f2, 7f7f7f, f4fce4, dddcd0, efffc0, e3e1dc, ffe0cb, e8e9df, ece1d6, feecd2, d8d8d8, 595959, e9f9c9, bbb9a1, dfff82, c8c3ba, ffc299, d2d4c0, dac3ad, fed9a6, bfbfbf, 3f3f3f, dff7ae, ada598, cfff43, ada598, ffa365, bcbfa1, c8a585, fec67a, a5a5a5, 262626, a9ea25, 2e2d21, 6f9400, 544e43, bf4d00, 6c6f4b, 6f5032, d77b00, 7f7f7f, 0c0c0c, 74a50f, 1f1e16, 4a6300, 38342d, 7f3300, 484a32, 4a3521, 8f5200',
        columns: 10
    },
    clarity: {
        colors: 'ffffff, 292934, f3f2dc, d2533c, 93a299, ad8f67, 726056, 4c5a6a, 808da0, 79463d, f2f2f2, e7e7ec, e7e5b9, f6dcd8, e9ecea, eee8e0, e4dedb, d8dde3, e5e8ec, e9d6d3, d8d8d8, c4c4d1, d5d185, edbab1, d3d9d6, ded2c2, c9beb8, b2bcc8, ccd1d9, d3aea7, bfbfbf, 8a8aa3, aca73b, e4978a, bec7c1, cdbba3, af9e94, 8c9bac, b2bac6, bd857c, a5a5a5, 56566e, 56531d, a43925, 6b7c72, 866b48, 554840, 39434f, 5c697b, 5a342d, 7f7f7f, 3b3b4b, 22210b, 6d2619, 47534c, 594730, 39302b, 262d35, 3d4652, 3c231e',
        columns: 10
    },
    slipstream: {
        colors: 'ffffff, 000000, b4dcfa, 212745, 4e67c8, 5eccf3, a7ea52, 5dceaf, ff8021, f14124, f2f2f2, 7f7f7f, 8bc9f7, c7cce4, dbe0f4, def4fc, edfadc, def5ef, ffe5d2, fcd9d3, d8d8d8, 595959, 4facf3, 909aca, b8c2e9, beeafa, dbf6b9, beebdf, ffcca6, f9b3a7, bfbfbf, 3f3f3f, 0d78c9, 5967af, 94a3de, 9ee0f7, caf297, 9de1cf, ffb279, f68d7b, a5a5a5, 262626, 063c64, 181d33, 31479f, 11b2eb, 81d319, 34ac8b, d85c00, c3260c, 7f7f7f, 0c0c0c, 021828, 101322, 202f6a, 0b769c, 568c11, 22725c, 903d00, 821908',
        columns: 10
    },
    metro: {
        colors: 'ffffff, 000000, d6ecff, 4e5b6f, 7fd13b, ea157a, feb80a, 00addc, 738ac8, 1ab39f, f2f2f2, 7f7f7f, a7d6ff, d9dde4, e5f5d7, fad0e4, fef0cd, c5f2ff, e2e7f4, c9f7f1, d8d8d8, 595959, 60b5ff, b3bcca, cbecb0, f6a1c9, fee29c, 8be6ff, c7d0e9, 94efe3, bfbfbf, 3f3f3f, 007dea, 8d9baf, b2e389, f272af, fed46b, 51d9ff, aab8de, 5fe7d5, a5a5a5, 262626, 003e75, 3a4453, 5ea226, af0f5b, c58c00, 0081a5, 425ea9, 138677, 7f7f7f, 0c0c0c, 00192e, 272d37, 3f6c19, 750a3d, 835d00, 00566e, 2c3f71, 0c594f',
        columns: 10
    },
    flow: {
        colors: 'ffffff, 000000, dbf5f9, 04617b, 0f6fc6, 009dd9, 0bd0d9, 10cf9b, 7cca62, a5c249, f2f2f2, 7f7f7f, b2e9f2, b4ecfc, c7e2fa, c4eeff, c9fafc, c9faed, e4f4df, edf2da, d8d8d8, 595959, 76d9e8, 6adafa, 90c6f6, 89deff, 93f5f9, 94f6db, cae9c0, dbe6b6, bfbfbf, 3f3f3f, 21b2c8, 20c8f7, 59a9f2, 4fceff, 5df0f6, 5ff2ca, b0dfa0, c9da91, a5a5a5, 262626, 105964, 02485c, 0b5394, 0075a2, 089ca2, 0b9b74, 54a838, 7e9532, 7f7f7f, 0c0c0c, 062328, 01303d, 073763, 004e6c, 05686c, 07674d, 387025, 546321',
        columns: 10
    },
    hardcover: {
        colors: 'ffffff, 000000, ece9c6, 895d1d, 873624, d6862d, d0be40, 877f6c, 972109, aeb795, f2f2f2, 7f7f7f, e1dca5, f2e0c6, f0d0c9, f6e6d5, f5f2d8, e7e5e1, fbc7bc, eef0e9, d8d8d8, 595959, d0c974, e6c28d, e2a293, eeceaa, ece5b2, cfccc3, f78f7a, dee2d4, bfbfbf, 3f3f3f, a29a36, daa454, d4735e, e6b681, e2d88c, b7b2a5, f35838, ced3bf, a5a5a5, 262626, 514d1b, 664515, 65281a, a2641f, a39428, 655f50, 711806, 879464, 7f7f7f, 0c0c0c, 201e0a, 442e0e, 431b11, 6c4315, 6d621a, 433f35, 4b1004, 5a6243',
        columns: 10
    },
    trek: {
        colors: 'ffffff, 000000, fbeec9, 4e3b30, f0a22e, a5644e, b58b80, c3986d, a19574, c17529, f2f2f2, 7f7f7f, f7e09e, e1d6cf, fcecd5, eddfda, f0e7e5, f3eae1, ece9e3, f5e3d1, d8d8d8, 595959, f3cc5f, c4ad9f, f9d9ab, dcc0b6, e1d0cc, e7d5c4, d9d4c7, ebc7a3, bfbfbf, 3f3f3f, d29f0f, a78470, f6c781, cba092, d2b9b2, dbc1a7, c6bfab, e1ac76, a5a5a5, 262626, 694f07, 3a2c24, c87d0e, 7b4b3a, 926255, a17242, 7b7153, 90571e, 7f7f7f, 0c0c0c, 2a1f03, 271d18, 855309, 523226, 614138, 6b4c2c, 524b37, 603a14',
        columns: 10
    },
    verve: {
        colors: 'ffffff, 000000, d2d2d2, 666666, ff388c, e40059, 9c007f, 68007f, 005bd3, 00349e, f2f2f2, 7f7f7f, bdbdbd, e0e0e0, ffd7e8, ffc6dc, ffb8f1, f1b2ff, c3dcff, b8cfff, d8d8d8, 595959, 9d9d9d, c1c1c1, ffafd1, ff8eba, ff71e4, e365ff, 87baff, 72a0ff, bfbfbf, 3f3f3f, 696969, a3a3a3, ff87ba, ff5597, ff2ad7, d519ff, 4b98ff, 2b71ff, a5a5a5, 262626, 343434, 4c4c4c, e90062, ab0042, 75005f, 4e005f, 00449e, 002676, 7f7f7f, 0c0c0c, 151515, 333333, 9b0041, 72002c, 4e003f, 34003f, 002d69, 00194f',
        columns: 10
    },
    monochrome: {
        colors: '000000, 1a1a1a, 333333, 4d4d4d, 666666, 808080, 999999, b3b3b3, cccccc, e6e6e6, f2f2f2, ffffff',
        columns: 12
    },
    accessible: {
        colors: 'black, grey, darkred, red, darkorange, gold, green, blue, darkblue, purple, white, darkgrey, saddlebrown, pink, orange, yellow, lightgreen, lightskyblue, lightblue, mediumpurple',
        columns: 10
    }
};

const DEFAULT_PRESET = 'office';
const DEFAULT_ACCESSIBLE_PRESET = 'accessible';
let serial = 0;
/**
 * Represents the [Kendo UI ColorPicker component for Angular]({% slug overview_colorpicker %}).
 * Designed to replace the `<input type="color">` HTML5 tag which is not widely supported in browsers.
 */
class ColorPickerComponent {
    constructor(popupService, localizationService) {
        this.popupService = popupService;
        /**
         * @hidden
         */
        this.hostClasses = true;
        /**
         * @hidden
         */
        this.focusableId = `k-colorpicker-${serial++}`;
        /**
         * Sets what view the ColorPicker will render in the popup.
         */
        this.view = 'gradient';
        /**
         * Sets the read-only state of the ColorPicker.
         */
        this.readonly = false;
        /**
         * Sets the disabled state of the ColorPicker.
         */
        this.disabled = false;
        /**
         * Specifies the output format of the ColorPicker.
         * The input value may be in a different format. However, it will be parsed into the output `format`
         * after the component processes it.
         *
         * If the `gradient` view is used with the `opacity` option set to true, this setting will be ignored and `rgba` will be used instead.
         *
         * The supported values are:
         * * `rgba` (default)
         * * `hex`
         * * [`name`](https://www.w3.org/wiki/CSS/Properties/color/keywords)
         */
        this.format = 'rgba';
        /**
         * Fires each time the value is changed.
         */
        this.valueChange = new EventEmitter();
        /**
         * Fires each time the popup is about to open.
         * This event is preventable. If you cancel it, the popup will remain closed.
         */
        this.open = new EventEmitter();
        /**
         * Fires each time the popup is about to close.
         * This event is preventable. If you cancel it, the popup will remain open.
         */
        this.close = new EventEmitter();
        /**
         * Fires each time ColorPicker is focused.
         */
        this.onFocus = new EventEmitter();
        /**
         * Fires each time the ColorPicker is blurred.
         */
        this.onBlur = new EventEmitter();
        /**
         * Fires each time the left side of the ColorPicker wrapper is clicked.
         * The event is triggered regardless of whether a ColorPicker icon is set or not.
         *
         * The [`ActiveColorClickEvent`]({% slug api_inputs_activecolorclickevent %}) event provides the option to prevent the popup opening.
         */
        this.activeColorClick = new EventEmitter();
        this._tabindex = 0;
        this._popupSettings = { animate: true };
        this._paletteSettings = {};
        this._gradientSettings = { opacity: true, clearButton: false };
        this.notifyNgTouched = () => { };
        this.notifyNgChanged = () => { };
        this.dynamicRTLSubscription = localizationService.changes.subscribe(({ rtl }) => {
            this.direction = rtl ? 'rtl' : 'ltr';
        });
    }
    /**
     * Specifies the value of the initially selected color.
     */
    set value(value) {
        this._value = parseColor$1(value, this.format);
    }
    get value() {
        return this._value;
    }
    /**
     * Configures the popup of the ColorPicker.
     */
    set popupSettings(value) {
        this._popupSettings = Object.assign(this._popupSettings, value);
    }
    get popupSettings() {
        return this._popupSettings;
    }
    /**
     * Configures the palette that is displayed in the ColorPicker popup.
     */
    set paletteSettings(value) {
        this._paletteSettings = Object.assign(this._paletteSettings, value);
    }
    get paletteSettings() {
        return this._paletteSettings;
    }
    /**
     * Configures the gradient that is displayed in the ColorPicker popup.
     */
    set gradientSettings(value) {
        this._gradientSettings = Object.assign(this._gradientSettings, value);
    }
    get gradientSettings() {
        return this._gradientSettings;
    }
    /**
     * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the component.
     */
    set tabindex(value) {
        const tabindex = Number(value);
        const defaultValue = 0;
        this._tabindex = !isNaN(tabindex) ? tabindex : defaultValue;
    }
    get tabindex() {
        return !this.disabled ? this._tabindex : undefined;
    }
    /**
     * Indicates whether the ColorPicker popup is open.
     */
    get isOpen() {
        return isPresent(this.popupRef);
    }
    /**
     * @hidden
     */
    get iconStyles() {
        if (this.iconClass) {
            return this.iconClass;
        }
        if (this.icon) {
            return `k-icon k-i-${this.icon}`;
        }
    }
    ngOnInit() {
        const defaultPreset = (this.format !== 'name') ? DEFAULT_PRESET : DEFAULT_ACCESSIBLE_PRESET;
        const settingsPalette = this._paletteSettings.palette;
        const presetColumns = typeof settingsPalette === 'string' && PALETTEPRESETS[settingsPalette] ?
            PALETTEPRESETS[settingsPalette].columns :
            undefined;
        this._paletteSettings = {
            palette: settingsPalette || defaultPreset,
            tileSize: this._paletteSettings.tileSize || 24,
            columns: this._paletteSettings.columns || presetColumns || 10
        };
    }
    ngOnChanges(changes) {
        if (changes.format && changes.format.currentValue === 'name') {
            this.view = 'palette';
        }
        if (this.view === 'gradient' && this.gradientSettings.opacity) {
            this.format = 'rgba';
            this.value = parseColor$1(this.value, this.format);
        }
    }
    ngOnDestroy() {
        this.closePopup();
        if (this.dynamicRTLSubscription) {
            this.dynamicRTLSubscription.unsubscribe();
        }
    }
    /**
     * @hidden
     */
    handleWrapperClick() {
        this.toggleWithEvents(!this.isOpen);
        this.focus();
    }
    /**
     * @hidden
     */
    handleActiveColorClick() {
        this.focus();
        const event = new ActiveColorClickEvent(this.value);
        this.activeColorClick.emit(event);
        if (!event.isOpenPrevented() || this.isOpen) {
            this.handleWrapperClick();
        }
    }
    /**
     * Focuses the wrapper of the ColorPicker.
     */
    focus() {
        if (this.disabled) {
            return;
        }
        this.wrapper.nativeElement.focus();
    }
    /**
     * @hidden
     */
    handleWrapperFocus() {
        if (isPresent(this.palette)) {
            this.palette.nativeElement.focus();
        }
        if (this.isFocused) {
            return;
        }
        this.isFocused = true;
        this.onFocus.emit();
    }
    /**
     * Blurs the ColorPicker.
     */
    blur() {
        this.wrapper.nativeElement.blur();
    }
    /**
     * @hidden
     */
    handleWrapperBlur() {
        if (this.isOpen) {
            return;
        }
        this.isFocused = false;
        this.onBlur.emit();
        this.notifyNgTouched();
    }
    /**
     * Clears the color value of the ColorPicker.
     */
    reset() {
        if (!isPresent(this.value)) {
            return;
        }
        this._value = undefined;
        this.notifyNgChanged(undefined);
    }
    /**
     * Toggles the popup of the ColorPicker.
     * Does not trigger the `open` and `close` events of the component.
     *
     * @param open An optional parameter. Specifies whether the popup will be opened or closed.
     */
    toggle(open) {
        if (this.disabled || this.readonly) {
            return;
        }
        this.closePopup();
        open = isPresent(open) ? open : !this.isOpen;
        if (open) {
            this.openPopup();
        }
    }
    /**
     * @hidden
     */
    handleValueChange(color, closePopup) {
        const parsedColor = parseColor$1(color, this.format);
        const valueChange = parsedColor !== this.value;
        if (closePopup) {
            this.toggleWithEvents(false);
            this.focus();
        }
        if (valueChange) {
            this.value = parsedColor;
            this.valueChange.emit(parsedColor);
            this.notifyNgChanged(parsedColor);
        }
    }
    /**
     * @hidden
     */
    handlePopupBlur(event) {
        const focusInPopupElement = this.popupRef.popupElement.contains(event.relatedTarget);
        const wrapperClicked = event.relatedTarget === this.wrapper.nativeElement;
        if (!this.isFocused || wrapperClicked || focusInPopupElement) {
            return;
        }
        this.isFocused = false;
        this.onBlur.emit();
        this.notifyNgTouched();
        this.toggleWithEvents(false);
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.value = value;
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.notifyNgChanged = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.notifyNgTouched = fn;
    }
    /**
     * @hidden
     */
    setDisabledState(isDisabled) {
        this.disabled = isDisabled;
    }
    /**
     * @hidden
     */
    handleWrapperKeyDown(event) {
        if (event.keyCode === Keys.ArrowDown || event.keyCode === Keys.Enter) {
            event.preventDefault();
            this.toggleWithEvents(true);
        }
    }
    /**
     * @hidden
     */
    handlePopupKeyDown(keyCode) {
        if (keyCode === Keys.Tab || keyCode === Keys.Escape) {
            this.toggleWithEvents(false);
            this.wrapper.nativeElement.focus();
        }
    }
    /**
     * @hidden
     * Used by the TextBoxContainer to determine if the component is empty.
     */
    isEmpty() {
        return false;
    }
    toggleWithEvents(open) {
        const sameState = this.isOpen === open;
        if (this.disabled || this.readonly || sameState) {
            return;
        }
        const eventArgs = new PreventableEvent();
        open ? this.open.emit(eventArgs) : this.close.emit(eventArgs);
        if (!eventArgs.isDefaultPrevented()) {
            this.toggle(open);
        }
    }
    openPopup() {
        const horizontalAlign = this.direction === "rtl" ? "right" : "left";
        const anchorPosition = { horizontal: horizontalAlign, vertical: "bottom" };
        const popupPosition = { horizontal: horizontalAlign, vertical: "top" };
        this.popupRef = this.popupService.open({
            anchor: this.wrapper,
            animate: this.popupSettings.animate,
            appendTo: this.popupSettings.appendTo,
            popupAlign: popupPosition,
            anchorAlign: anchorPosition,
            popupClass: 'k-colorpicker-popup',
            content: this.popupTemplate,
            positionMode: 'absolute'
        });
        this.popupRef.popupAnchorViewportLeave.subscribe(() => {
            this.toggleWithEvents(false);
            if (!this.isOpen) {
                this.wrapper.nativeElement.focus({
                    preventScroll: true
                });
            }
        });
    }
    closePopup() {
        if (!this.isOpen) {
            return;
        }
        this.popupRef.close();
        this.popupRef = null;
        this.palette = null;
    }
}
ColorPickerComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-colorpicker',
                providers: [{
                        multi: true,
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => ColorPickerComponent)
                    }, {
                        provide: KendoInput,
                        useExisting: forwardRef(() => ColorPickerComponent)
                    },
                    LocalizationService,
                    {
                        provide: L10N_PREFIX,
                        useValue: 'kendo.colorpicker'
                    }
                ],
                template: `
        <span
            #wrapper
            [ngClass]="{
                'k-picker-wrap': true,
                'k-state-disabled': this.disabled,
                'k-state-focused': this.isFocused
            }"
            role="listbox"
            [attr.aria-expanded]="isOpen"
            [id]="focusableId"
            [attr.tabindex]="tabindex"
            (focus)="handleWrapperFocus()"
            (blur)="handleWrapperBlur()"
            (mousedown)="$event.preventDefault()"
            (keydown)="handleWrapperKeyDown($event)"
        >
            <span *ngIf="!iconStyles" class="k-selected-color" [style.background-color]="value" (click)="handleActiveColorClick()">
                <span class="k-icon k-i-line" *ngIf="!value"></span>
            </span>
            <span *ngIf="iconStyles" class="k-tool-icon" [ngClass]="iconStyles" (click)="handleActiveColorClick()">
                <span class="k-selected-color" [style.background-color]="value"></span>
            </span>
            <span class="k-select" (click)="handleWrapperClick()">
                <span class="k-icon k-i-arrow-s"></span>
            </span>
        </span>
        <ng-template #popupTemplate>
            <kendo-colorgradient
                *ngIf="view === 'gradient' || view === 'combo'"
                kendoFocusOnDomReady
                [value]="value"
                [format]="format"
                [opacity]="gradientSettings.opacity"
                [clearButton]="gradientSettings.clearButton"
                (focusout)="handlePopupBlur($event)"
                (valueChange)="handleValueChange($event, false)"
                (keydown)="handlePopupKeyDown($event.keyCode)"
            >
            </kendo-colorgradient>
            <kendo-colorpalette
                #palette
                *ngIf="view === 'palette' || view === 'combo'"
                kendoFocusOnDomReady
                [palette]="paletteSettings.palette"
                [columns]="paletteSettings.columns"
                [tileSize]="paletteSettings.tileSize"
                [format]="format"
                [value]="value"
                (blur)="handlePopupBlur($event)"
                (cellSelection)="handleValueChange($event, true)"
                (mousedown)="$event.preventDefault()"
                (keydown)="handlePopupKeyDown($event.keyCode)"
            >
            </kendo-colorpalette>
        </ng-template>
        <ng-container #container></ng-container>
    `
            },] },
];
/** @nocollapse */
ColorPickerComponent.ctorParameters = () => [
    { type: PopupService },
    { type: LocalizationService }
];
ColorPickerComponent.propDecorators = {
    hostClasses: [{ type: HostBinding, args: ['class.k-widget',] }, { type: HostBinding, args: ['class.k-colorpicker',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    focusableId: [{ type: Input }],
    view: [{ type: Input }],
    readonly: [{ type: Input }],
    disabled: [{ type: Input }],
    format: [{ type: Input }],
    value: [{ type: Input }],
    popupSettings: [{ type: Input }],
    paletteSettings: [{ type: Input }],
    gradientSettings: [{ type: Input }],
    icon: [{ type: Input }],
    iconClass: [{ type: Input }],
    tabindex: [{ type: Input }],
    valueChange: [{ type: Output }],
    open: [{ type: Output }],
    close: [{ type: Output }],
    onFocus: [{ type: Output, args: ['focus',] }],
    onBlur: [{ type: Output, args: ['blur',] }],
    activeColorClick: [{ type: Output }],
    container: [{ type: ViewChild, args: ['container', { read: ViewContainerRef },] }],
    wrapper: [{ type: ViewChild, args: ['wrapper',] }],
    popupTemplate: [{ type: ViewChild, args: ['popupTemplate',] }],
    palette: [{ type: ViewChild, args: ['palette', { read: ElementRef },] }]
};

/**
 * @hidden
 */
class ColorPaletteService {
    constructor() {
        this.colorRows = [];
    }
    setColorMatrix(palette, columns) {
        this.colorRows = [];
        if (!(isPresent(palette) && palette.length)) {
            return;
        }
        columns = columns || palette.length;
        for (let start = 0; start < palette.length; start += columns) {
            const row = palette.slice(start, columns + start);
            this.colorRows.push(row);
        }
    }
    getCellCoordsFor(color) {
        if (!isPresent(color)) {
            return;
        }
        for (let row = 0; row < this.colorRows.length; row++) {
            for (let col = 0; col < this.colorRows[row].length; col++) {
                if (this.colorRows[row][col] === color) {
                    return { row, col };
                }
            }
        }
    }
    getColorAt(cellCoords) {
        if (!(isPresent(cellCoords) && isPresent(this.colorRows[cellCoords.row]))) {
            return;
        }
        return this.colorRows[cellCoords.row][cellCoords.col];
    }
    getNextCell(current, horizontalStep, verticalStep) {
        if (!(isPresent(current) && isPresent(current.row) && isPresent(current.col))) {
            return { row: 0, col: 0 };
        }
        const row = this.clampIndex(current.row + verticalStep, this.colorRows.length - 1);
        const col = this.clampIndex(current.col + horizontalStep, this.colorRows[row].length - 1);
        return { row, col };
    }
    clampIndex(index, max) {
        const minArrayIndex = 0;
        if (index < minArrayIndex) {
            return minArrayIndex;
        }
        if (index > max) {
            return max;
        }
        return index;
    }
}
ColorPaletteService.decorators = [
    { type: Injectable },
];

const DEFAULT_TILE_SIZE = 24;
const DEFAULT_COLUMNS_COUNT = 10;
const DEFAULT_PRESET$1 = 'office';
const DEFAULT_ACCESSIBLE_PRESET$1 = 'accessible';
let serial$1 = 0;
/**
 * The ColorPalette component provides a set of predefined palette presets and enables you to implement a custom color palette.
 * The ColorPalette is independently used by `kendo-colorpicker` and can be directly added to the page.
 */
class ColorPaletteComponent {
    constructor(service, localizationService) {
        this.service = service;
        /**
         * @hidden
         */
        this.id = `k-colorpalette-${serial$1++}`;
        /**
         * Specifies the output format of the ColorPaletteComponent.
         * The input value may be in a different format. However, it will be parsed into the output `format`
         * after the component processes it.
         *
         * The supported values are:
         * * (Default) `hex`
         * * `rgba`
         * * `name`
         */
        this.format = 'hex';
        /**
         * Specifies the size of a color cell.
         *
         * The possible values are:
         * * (Default) `tileSize = 24`
         * * `{ width: number, height: number }`
         */
        this.tileSize = { width: DEFAULT_TILE_SIZE, height: DEFAULT_TILE_SIZE };
        /**
         * Fires each time the color selection is changed.
         */
        this.selectionChange = new EventEmitter();
        /**
         * Fires each time the value is changed.
         */
        this.valueChange = new EventEmitter();
        /**
         * Fires each time the user selects a cell with the mouse or presses `Enter`.
         *
         * @hidden
         */
        this.cellSelection = new EventEmitter();
        /**
         * @hidden
         */
        this.hostClasses = true;
        this._tabindex = 0;
        this.notifyNgTouched = () => { };
        this.notifyNgChanged = () => { };
        this.dynamicRTLSubscription = localizationService.changes.subscribe(({ rtl }) => {
            this.direction = rtl ? 'rtl' : 'ltr';
        });
    }
    /**
     * @hidden
     */
    get paletteId() {
        return this.id;
    }
    /**
     * Specifies the value of the initially selected color.
     */
    set value(value) {
        this._value = parseColor$1(value, this.format);
    }
    get value() {
        return this._value;
    }
    /**
     * Specifies the number of columns that will be displayed.
     * Defaults to `10`.
     */
    set columns(value) {
        const minColumnsCount = 1;
        this._columns = value > minColumnsCount ? value : minColumnsCount;
    }
    get columns() {
        return this._columns;
    }
    /**
     * The color palette that will be displayed.
     *
     * The supported values are:
     * * The name of the predefined palette preset (for example, `office`, `basic`, and `apex`).
     * * A string with comma-separated colors.
     * * A string array.
     */
    set palette(value) {
        if (!isPresent(value)) {
            value = DEFAULT_PRESET$1;
        }
        if (typeof value === 'string' && isPresent(PALETTEPRESETS[value])) {
            this.columns = this.columns || PALETTEPRESETS[value].columns;
            value = PALETTEPRESETS[value].colors;
        }
        const colors = (typeof value === 'string') ? value.split(',') : value;
        this._palette = colors.map(color => parseColor$1(color, this.format, false));
    }
    get palette() {
        return this._palette;
    }
    /**
     * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the component.
     */
    set tabindex(value) {
        const tabindex = Number(value);
        const defaultValue = 0;
        this._tabindex = !isNaN(tabindex) ? tabindex : defaultValue;
    }
    get tabindex() {
        return !this.disabled ? this._tabindex : undefined;
    }
    /**
     * @hidden
     */
    get tileLayout() {
        if (typeof this.tileSize !== 'number') {
            return this.tileSize;
        }
        return { width: this.tileSize, height: this.tileSize };
    }
    /**
     * @hidden
     */
    get colorRows() {
        return this.service.colorRows;
    }
    /**
     * @hidden
     */
    get hostTabindex() { return this.tabindex; }
    /**
     * @hidden
     */
    get disabledClass() { return this.disabled; }
    ngOnInit() {
        if (this.colorRows.length === 0) {
            const defaultPreset = (this.format !== 'name') ? DEFAULT_PRESET$1 : DEFAULT_ACCESSIBLE_PRESET$1;
            this.palette = this.palette || defaultPreset;
            this.setRows();
            this.focusedCell = this.service.getCellCoordsFor(this.value);
        }
    }
    ngOnDestroy() {
        if (this.dynamicRTLSubscription) {
            this.dynamicRTLSubscription.unsubscribe();
        }
    }
    ngOnChanges(changes) {
        if (changes.palette || changes.columns) {
            this.setRows();
        }
        if (changes.palette || changes.value || changes.columns) {
            this.focusedCell = this.service.getCellCoordsFor(this.value);
        }
    }
    /**
     * @hidden
     */
    handleKeydown(event) {
        const isRTL = this.direction === 'rtl';
        switch (event.keyCode) {
            case Keys.ArrowDown:
                this.handleCellNavigation(0, 1);
                break;
            case Keys.ArrowUp:
                this.handleCellNavigation(0, -1);
                break;
            case Keys.ArrowRight:
                this.handleCellNavigation(isRTL ? -1 : 1, 0);
                break;
            case Keys.ArrowLeft:
                this.handleCellNavigation(isRTL ? 1 : -1, 0);
                break;
            case Keys.Enter:
                this.handleEnter();
                break;
            default: return;
        }
        event.preventDefault();
    }
    /**
     * @hidden
     */
    handleHostBlur() {
        this.notifyNgTouched();
    }
    /**
     * @hidden
     */
    handleCellSelection(value, focusedCell) {
        if (this.readonly) {
            return;
        }
        this.focusedCell = focusedCell;
        const parsedColor = parseColor$1(value, this.format, false);
        this.cellSelection.emit(parsedColor);
        if (this.value !== parsedColor) {
            this.value = parsedColor;
            this.valueChange.emit(parsedColor);
            this.notifyNgChanged(parsedColor);
        }
        if (this.selection !== parsedColor) {
            this.selection = parsedColor;
            this.selectionChange.emit(parsedColor);
        }
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.value = value;
        this.focusedCell = this.service.getCellCoordsFor(this.value);
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.notifyNgChanged = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.notifyNgTouched = fn;
    }
    /**
     * @hidden
     */
    setDisabledState(isDisabled) {
        this.disabled = isDisabled;
    }
    /**
     * @hidden
     * Used by the TextBoxContainer to determine if the component is empty.
     */
    isEmpty() {
        return false;
    }
    /**
     * Clears the color value of the ColorPalette.
     */
    reset() {
        this.focusedCell = null;
        if (isPresent(this.value)) {
            this._value = undefined;
            this.notifyNgChanged(undefined);
        }
    }
    setRows() {
        if (!isPresent(this.palette)) {
            return;
        }
        this.columns = this.columns || DEFAULT_COLUMNS_COUNT;
        this.service.setColorMatrix(this.palette, this.columns);
    }
    handleCellNavigation(horizontalStep, verticalStep) {
        if (this.readonly) {
            return;
        }
        this.focusedCell = this.service.getNextCell(this.focusedCell, horizontalStep, verticalStep);
        const selection = this.service.getColorAt(this.focusedCell);
        if (this.selection !== selection) {
            this.selection = selection;
            this.selectionChange.emit(selection);
        }
    }
    handleEnter() {
        if (!isPresent(this.focusedCell)) {
            return;
        }
        const selectedColor = this.service.getColorAt(this.focusedCell);
        this.handleCellSelection(selectedColor, this.focusedCell);
    }
}
ColorPaletteComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-colorpalette',
                providers: [
                    {
                        multi: true,
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => ColorPaletteComponent) // tslint:disable-line:no-forward-ref
                    }, {
                        provide: KendoInput,
                        useExisting: forwardRef(() => ColorPaletteComponent)
                    },
                    ColorPaletteService,
                    LocalizationService,
                    {
                        provide: L10N_PREFIX,
                        useValue: 'kendo.colorpalette'
                    }
                ],
                template: `
        <div role="grid">
            <table class="k-palette k-reset" role="presentation">
                <tbody>
                    <tr role="row" *ngFor="let row of colorRows; let rowIndex = index">
                        <td *ngFor="let color of row; let colIndex = index"
                            [class.k-state-selected]="focusedCell?.row === rowIndex && focusedCell?.col === colIndex"
                            class="k-item"
                            [attr.value]="color"
                            (click)="handleCellSelection(color, { row: rowIndex, col: colIndex })"
                            [ngStyle]="{
                                backgroundColor: color,
                                width: tileLayout.width + 'px',
                                height: tileLayout.height + 'px',
                                minWidth: tileLayout.width + 'px'
                            }">
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    `
            },] },
];
/** @nocollapse */
ColorPaletteComponent.ctorParameters = () => [
    { type: ColorPaletteService },
    { type: LocalizationService }
];
ColorPaletteComponent.propDecorators = {
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    paletteId: [{ type: HostBinding, args: ['attr.id',] }],
    id: [{ type: Input }],
    format: [{ type: Input }],
    value: [{ type: Input }],
    columns: [{ type: Input }],
    palette: [{ type: Input }],
    tabindex: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    tileSize: [{ type: Input }],
    selectionChange: [{ type: Output }],
    valueChange: [{ type: Output }],
    cellSelection: [{ type: Output }],
    hostTabindex: [{ type: HostBinding, args: ['attr.tabindex',] }],
    hostClasses: [{ type: HostBinding, args: ['class.k-widget',] }, { type: HostBinding, args: ['class.k-colorpalette',] }],
    disabledClass: [{ type: HostBinding, args: ['class.k-state-disabled',] }],
    handleKeydown: [{ type: HostListener, args: ['keydown', ['$event'],] }],
    handleHostBlur: [{ type: HostListener, args: ['blur',] }]
};

const DEFAULT_OUTPUT_FORMAT = 'rgba';
const DEFAULT_BACKGROUND_COLOR = 'rgba(255, 0, 0, 1)';
let serial$2 = 0;
/**
 * The ColorGradient component enables smooth color transitions and provides options for selecting specific colors over the drag handle.
 * The ColorGradient is independently used by `kendo-colorpicker` and can be directly added to the page.
 */
class ColorGradientComponent {
    constructor(host, ngZone, renderer, cdr, localizationService) {
        this.host = host;
        this.ngZone = ngZone;
        this.renderer = renderer;
        this.cdr = cdr;
        this.hostClasses = true;
        /**
         * @hidden
         */
        this.id = `k-colorgradient-${serial$2++}`;
        /**
         * Defines whether the alpha slider will be displayed.
         */
        this.opacity = true;
        /**
         * Sets the disabled state of the ColorGradient.
         */
        this.disabled = false;
        /**
         * Sets the read-only state of the ColorGradient.
         */
        this.readonly = false;
        /**
         * Specifies whether the ColorGradient should display a 'Clear color' button.
         */
        this.clearButton = false;
        /**
         * Fires each time the user selects a new color.
         */
        this.valueChange = new EventEmitter();
        /**
         * @hidden
         */
        this.backgroundColor = DEFAULT_BACKGROUND_COLOR;
        /**
         * @hidden
         *
         * Represents the currently selected `hue`, `saturation`, `value`, and `alpha` values.
         * The values are initially set in `ngOnInit` or in `ngOnChanges` and are
         * updated on moving the drag handle or the sliders.
         */
        this.hsva = {};
        /**
         * Specifies the output format of the ColorGradientComponent.
         * The input value may be in a different format. However, it will be parsed into the output `format`
         * after the component processes it.
         *
         * The supported values are:
         * * (Default) `rgba`
         * * `hex`
         */
        this.format = DEFAULT_OUTPUT_FORMAT;
        this._tabindex = 0;
        this.listeners = [];
        this.notifyNgChanged = () => { };
        this.notifyNgTouched = () => { };
        this.dynamicRTLSubscription = localizationService.changes.subscribe(({ rtl }) => {
            this.direction = rtl ? 'rtl' : 'ltr';
        });
    }
    get disabledClass() {
        return this.disabled;
    }
    get hostTabindex() {
        return this.tabindex;
    }
    /**
     * @hidden
     */
    get gradientId() {
        return this.id;
    }
    /**
     * Specifies the value of the initially selected color.
     */
    set value(value) {
        this._value = parseColor$1(value, this.format);
    }
    get value() {
        return this._value;
    }
    /**
     * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the component.
     */
    set tabindex(value) {
        const tabindex = Number(value);
        const defaultValue = 0;
        this._tabindex = !isNaN(tabindex) ? tabindex : defaultValue;
    }
    get tabindex() {
        return !this.disabled ? this._tabindex : undefined;
    }
    /**
     * Indicates whether the ColorGradient or any of its content is focused.
     */
    get isFocused() {
        if (!(isDocumentAvailable() && isPresent(this.host))) {
            return false;
        }
        return this.host.nativeElement === document.activeElement || this.host.nativeElement.contains(document.activeElement);
    }
    /**
     * @hidden
     */
    get alphaSliderValue() {
        // setting the initial value to undefined to force the slider to recalculate the height of the slider track on the next cdr run
        if (!(isPresent(this.hsva) && isPresent(this.hsva.a))) {
            return;
        }
        return this.hsva.a * 100;
    }
    get gradientRect() {
        return this.gradientWrapper.nativeElement.getBoundingClientRect();
    }
    ngAfterViewInit() {
        this.updateUI();
        this.cdr.detectChanges();
        this.addEventListeners();
    }
    ngOnChanges(changes) {
        if (isChanged('value', changes) && !this.isFocused) {
            this.updateUI();
        }
    }
    ngOnDestroy() {
        this.listeners.forEach(removeListener => removeListener());
        if (this.dynamicRTLSubscription) {
            this.dynamicRTLSubscription.unsubscribe();
        }
    }
    /**
     * Focuses the component.
     */
    focus() {
        if (this.disabled) {
            return;
        }
        this.host.nativeElement.focus();
    }
    /**
     * @hidden
     */
    reset() {
        this.handleValueChange(undefined);
        this.updateUI();
    }
    /**
     * @hidden
     */
    handleDragPress(args) {
        if (this.disabled || this.readonly || !isPresent(args.originalEvent)) {
            return;
        }
        this.focus();
        args.originalEvent.preventDefault();
    }
    /**
     * @hidden
     */
    onHandleDrag(args) {
        if (this.disabled || this.readonly) {
            return;
        }
        this.renderer.addClass(this.gradientWrapper.nativeElement, 'k-dragging');
        this.changePosition(args);
    }
    /**
     * @hidden
     */
    onHandleRelease() {
        if (this.disabled || this.readonly) {
            return;
        }
        this.renderer.removeClass(this.gradientWrapper.nativeElement, 'k-dragging');
    }
    /**
     * @hidden
     */
    changePosition(position) {
        if (this.disabled || this.readonly) {
            return;
        }
        this.ngZone.run(() => this.moveDragHandle(position));
    }
    /**
     * @hidden
     */
    handleHueSliderChange(hue) {
        this.handleValueChange(getColorFromHSV(this.hsva));
        this.backgroundColor = getColorFromHue(hue);
        this.setAlphaSliderBackground(this.backgroundColor);
    }
    /**
     * @hidden
     */
    handleAlphaSliderChange(alpha) {
        this.hsva.a = alpha / 100;
        this.handleValueChange(getColorFromHSV(this.hsva));
    }
    /**
     * @hidden
     */
    handleInputsValueChange(color) {
        const parsed = parseColor$1(color, this.format);
        if (this.value !== parsed) {
            this.handleValueChange(parsed);
            this.updateUI();
        }
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.value = value;
        if (isPresent(this.gradientWrapper)) {
            this.updateUI();
        }
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.notifyNgChanged = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.notifyNgTouched = fn;
    }
    /**
     * @hidden
     */
    setDisabledState(isDisabled) {
        this.disabled = isDisabled;
    }
    /**
     * @hidden
     * Used by the TextBoxContainer to determine if the component is empty.
     */
    isEmpty() {
        return false;
    }
    moveDragHandle(position) {
        const deltaX = position.clientX - this.gradientRect.left;
        const deltaY = position.clientY - this.gradientRect.top;
        const top = fitIntoBounds(deltaY, 0, this.gradientRect.height);
        const left = fitIntoBounds(deltaX, 0, this.gradientRect.width);
        this.setDragHandleElementPosition(top, left);
        this.hsva.s = left / this.gradientRect.width;
        this.hsva.v = 1 - top / this.gradientRect.height;
        this.handleValueChange(getColorFromHSV(this.hsva));
        this.setAlphaSliderBackground(getColorFromHSV(Object.assign({}, this.hsva, { a: 1 })));
    }
    updateUI() {
        if (!isDocumentAvailable()) {
            return;
        }
        this.hsva = this.value ? getHSV(this.value) : { h: 0, s: 0, v: 1, a: 1 };
        const top = (1 - this.hsva.v) * this.gradientRect.height;
        const left = this.hsva.s * this.gradientRect.width;
        this.setDragHandleElementPosition(top, left);
        this.backgroundColor = getColorFromHue(this.hsva.h);
        this.setAlphaSliderBackground(this.backgroundColor);
    }
    handleValueChange(color) {
        if (this.value === color) {
            return;
        }
        this.value = color;
        this.valueChange.emit(color);
        this.notifyNgChanged(color);
    }
    setDragHandleElementPosition(top, left) {
        const dragHandle = this.dragHandle.nativeElement;
        this.renderer.setStyle(dragHandle, 'top', `${top}px`);
        this.renderer.setStyle(dragHandle, 'left', `${left}px`);
    }
    setAlphaSliderBackground(backgroundColor) {
        if (!isPresent(this.alphaSlider)) {
            return;
        }
        const sliderTrack = this.alphaSlider.track.nativeElement;
        this.renderer.setStyle(sliderTrack, 'background', `linear-gradient(to top, transparent, ${backgroundColor})`);
    }
    addEventListeners() {
        this.ngZone.runOutsideAngular(() => {
            const focusOutListener = this.renderer.listen(this.host.nativeElement, 'focusout', (event) => {
                if (!containsFocus(this.host.nativeElement, event.relatedTarget) && isUntouched(this.host)) {
                    this.ngZone.run(() => this.notifyNgTouched());
                }
            });
            this.listeners.push(focusOutListener);
        });
    }
}
ColorGradientComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-colorgradient',
                providers: [{
                        multi: true,
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => ColorGradientComponent)
                    }, {
                        provide: KendoInput,
                        useExisting: forwardRef(() => ColorGradientComponent)
                    },
                    LocalizationService,
                    {
                        provide: L10N_PREFIX,
                        useValue: 'kendo.colorgradient'
                    }
                ],
                template: `
        <div class="k-hbox k-hsv-wrap">
            <div class="k-hsv-rectangle" [style.background-color]="backgroundColor">
                <div
                    #gradientWrapper
                    kendoDraggable
                    class="k-hsv-gradient"
                    (click)="changePosition($event)"
                    (kendoPress)="handleDragPress($event)"
                    (kendoDrag)="onHandleDrag($event)"
                    (kendoRelease)="onHandleRelease()">
                    <div
                        #dragHandle
                        class="k-draghandle"
                    >
                    </div>
                </div>
            </div>
            <div class="k-hbox k-sliders-wrap {{ clearButton ? 'k-sliders-wrap-clearable' : '' }}">
                <span class="k-clear-color k-button k-bare k-button-icon" *ngIf="clearButton" (click)="reset()">
                    <span class="k-icon k-i-reset-color"></span>
                </span>
                <kendo-slider
                    class="k-hue-slider"
                    [disabled]="disabled"
                    [readonly]="readonly"
                    [showButtons]="false"
                    [tickPlacement]="'none'"
                    [vertical]="true"
                    [min]="0"
                    [max]="360"
                    [smallStep]="5"
                    [largeStep]="10"
                    [(value)]="hsva.h"
                    (valueChange)="handleHueSliderChange($event)"
                >
                </kendo-slider>
                <kendo-slider
                    *ngIf="opacity && format === 'rgba'"
                    #alphaSlider
                    class="k-alpha-slider"
                    [disabled]="disabled"
                    [readonly]="readonly"
                    [showButtons]="false"
                    [tickPlacement]="'none'"
                    [vertical]="true"
                    [min]="0"
                    [max]="100"
                    [smallStep]="1"
                    [largeStep]="10"
                    [value]="alphaSliderValue"
                    (valueChange)="handleAlphaSliderChange($event)"
                >
                </kendo-slider>
            </div>
        </div>
        <kendo-colorinput
            [opacity]="opacity"
            [value]="value"
            [disabled]="disabled"
            [readonly]="readonly"
            (valueChange)="handleInputsValueChange($event)"
        >
        </kendo-colorinput>
`
            },] },
];
/** @nocollapse */
ColorGradientComponent.ctorParameters = () => [
    { type: ElementRef },
    { type: NgZone },
    { type: Renderer2 },
    { type: ChangeDetectorRef },
    { type: LocalizationService }
];
ColorGradientComponent.propDecorators = {
    hostClasses: [{ type: HostBinding, args: ['class.k-widget',] }, { type: HostBinding, args: ['class.k-flatcolorpicker',] }],
    disabledClass: [{ type: HostBinding, args: ['class.k-state-disabled',] }],
    hostTabindex: [{ type: HostBinding, args: ['attr.tabindex',] }],
    gradientId: [{ type: HostBinding, args: ['attr.id',] }],
    id: [{ type: Input }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    opacity: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    clearButton: [{ type: Input }],
    value: [{ type: Input }],
    tabindex: [{ type: Input }],
    valueChange: [{ type: Output }],
    format: [{ type: Input }],
    alphaSlider: [{ type: ViewChild, args: ['alphaSlider',] }],
    gradientWrapper: [{ type: ViewChild, args: ['gradientWrapper',] }],
    dragHandle: [{ type: ViewChild, args: ['dragHandle',] }]
};

/**
 * @hidden
 */
class ColorInputComponent {
    constructor(host) {
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
    /**
     * Indicates whether any of the inputs are focused.
     */
    get isFocused() {
        if (!(isDocumentAvailable() && isPresent(this.host))) {
            return false;
        }
        const activeElement = document.activeElement;
        return this.host.nativeElement.contains(activeElement);
    }
    /**
     * Indicates whether any of the rgba inputs have value.
     */
    get rgbaInputValid() {
        return Object.keys(this.rgba).every(key => isPresent(this.rgba[key]));
    }
    ngOnChanges(changes) {
        if (isPresent(changes.value) && !this.isFocused) {
            this.hex = parseColor$1(this.value, 'hex');
            this.rgba = getRGBA(this.value);
            this.rgba.a = parseColor$1(this.value, 'rgba') ? this.rgba.a : 1;
        }
    }
    handleRgbaValueChange() {
        const color = getColorFromRGBA(this.rgba);
        if (!this.rgbaInputValid || color === this.value) {
            return;
        }
        this.value = color;
        this.rgba = getRGBA(this.value);
        this.hex = parseColor$1(color, 'hex');
        this.valueChange.emit(color);
    }
    handleHexValueChange(hex) {
        this.hex = hex;
        const color = parseColor$1(hex, 'rgba');
        if (!isPresent(color) || color === this.value) {
            return;
        }
        this.value = color;
        this.rgba = getRGBA(color);
        this.valueChange.emit(color);
    }
    handleRgbaInputBlur() {
        if (!this.rgbaInputValid) {
            this.rgba = getRGBA(this.value);
        }
    }
    handleHexInputBlur() {
        this.hex = parseColor$1(this.value, 'hex');
    }
}
ColorInputComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-colorinput',
                template: `
        <div class="k-hbox k-gradient-values">
            <input
                #hexInput
                class="k-textbox k-hex-value"
                [disabled]="disabled"
                [readonly]="readonly"
                [value]="hex || ''"
                placeholder="no color"
                (blur)="handleHexInputBlur()"
                (input)="handleHexValueChange(hexInput.value)"
            />
            <kendo-numerictextbox
                [disabled]="disabled"
                [readonly]="readonly"
                [min]="0"
                [max]="255"
                placeholder="R"
                [(value)]="rgba.r"
                [autoCorrect]="true"
                [spinners]="false"
                [format]="'n'"
                [decimals]="0"
                (blur)="handleRgbaInputBlur()"
                (valueChange)="handleRgbaValueChange()"
            >
            </kendo-numerictextbox>
            <kendo-numerictextbox
                [disabled]="disabled"
                [readonly]="readonly"
                [min]="0"
                [max]="255"
                placeholder="G"
                [(value)]="rgba.g"
                [autoCorrect]="true"
                [spinners]="false"
                [format]="'n'"
                [decimals]="0"
                (blur)="handleRgbaInputBlur()"
                (valueChange)="handleRgbaValueChange()"
            >
            </kendo-numerictextbox>
            <kendo-numerictextbox
                [disabled]="disabled"
                [readonly]="readonly"
                [min]="0"
                [max]="255"
                placeholder="B"
                [(value)]="rgba.b"
                [autoCorrect]="true"
                [spinners]="false"
                [format]="'n'"
                [decimals]="0"
                (blur)="handleRgbaInputBlur()"
                (valueChange)="handleRgbaValueChange()"
            >
            </kendo-numerictextbox>
            <kendo-numerictextbox
                *ngIf="opacity"
                [disabled]="disabled"
                [readonly]="readonly"
                [min]="0"
                [max]="1"
                placeholder="A"
                [(value)]="rgba.a"
                [autoCorrect]="true"
                [spinners]="false"
                [step]="0.01"
                [format]="'n2'"
                [decimals]="2"
                (blur)="handleRgbaInputBlur()"
                (valueChange)="handleRgbaValueChange()"
            >
            </kendo-numerictextbox>
        </div>
        <div class="k-hbox k-gradient-values">
            <div class="k-hex-value">hex</div>
            <div>r</div>
            <div>g</div>
            <div>b</div>
            <div *ngIf="opacity">a</div>
        </div>
    `
            },] },
];
/** @nocollapse */
ColorInputComponent.ctorParameters = () => [
    { type: ElementRef }
];
ColorInputComponent.propDecorators = {
    value: [{ type: Input }],
    opacity: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    valueChange: [{ type: Output }],
    colorInputClass: [{ type: HostBinding, args: ['class.k-colorinputs',] }]
};

/**
 * @hidden
 */
class FocusOnDomReadyDirective {
    constructor(host, ngZone) {
        this.host = host;
        this.ngZone = ngZone;
    }
    ngAfterContentInit() {
        this.focusOnNextTick();
    }
    focusOnNextTick() {
        this.ngZone.runOutsideAngular(() => setTimeout(() => this.host.nativeElement.focus()));
    }
}
FocusOnDomReadyDirective.decorators = [
    { type: Directive, args: [{
                selector: '[kendoFocusOnDomReady]'
            },] },
];
/** @nocollapse */
FocusOnDomReadyDirective.ctorParameters = () => [
    { type: ElementRef },
    { type: NgZone }
];

const PUBLIC_DIRECTIVES = [
    ColorPickerComponent,
    ColorPaletteComponent,
    ColorGradientComponent
];
const INTERNAL_DIRECTIVES = [
    ColorInputComponent,
    FocusOnDomReadyDirective
];
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the ColorPicker.
 */
class ColorPickerModule {
}
ColorPickerModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    PUBLIC_DIRECTIVES,
                    INTERNAL_DIRECTIVES
                ],
                exports: [PUBLIC_DIRECTIVES],
                imports: [
                    SliderModule,
                    NumericTextBoxModule,
                    CommonModule,
                    PopupModule,
                    DraggableModule
                ]
            },] },
];

/**
 * Represents the directive that renders the [Kendo UI CheckBox]({% slug overview_checkbox %}) input component.
 * The directive is placed on input type="checkbox" elements.
 *
 * @example
 * ```ts-no-run
 * <input type="checkbox" kendoCheckBox />
 * ```
 */
class CheckBoxDirective {
    constructor() {
        this.kendoClass = true;
    }
}
CheckBoxDirective.decorators = [
    { type: Directive, args: [{
                selector: 'input[kendoCheckBox]'
            },] },
];
CheckBoxDirective.propDecorators = {
    kendoClass: [{ type: HostBinding, args: ['class.k-checkbox',] }]
};

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the CheckBox directive.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the CheckBox module
 * import { CheckBoxModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, CheckBoxModule], // import CheckBox module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class CheckBoxModule {
}
CheckBoxModule.decorators = [
    { type: NgModule, args: [{
                declarations: [CheckBoxDirective],
                exports: [CheckBoxDirective],
                imports: [CommonModule]
            },] },
];

/**
 * Represents the directive that renders the [Kendo UI RadioButton]({% slug overview_checkbox %}) input component.
 * The directive is placed on input type="radio" elements.
 *
 * @example
 * ```ts-no-run
 * <input type="radio" kendoRadioButton />
 * ```
 */
class RadioButtonDirective {
    constructor() {
        this.kendoClass = true;
    }
}
RadioButtonDirective.decorators = [
    { type: Directive, args: [{
                selector: 'input[kendoRadioButton]'
            },] },
];
RadioButtonDirective.propDecorators = {
    kendoClass: [{ type: HostBinding, args: ['class.k-radio',] }]
};

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the RadioButton directive.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the RadioButton module
 * import { RadioButtonModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, RadioButtonModule], // import RadioButton module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class RadioButtonModule {
}
RadioButtonModule.decorators = [
    { type: NgModule, args: [{
                declarations: [RadioButtonDirective],
                exports: [RadioButtonDirective],
                imports: [CommonModule]
            },] },
];

let serial$3 = 0;
/**
 * Represents an error message that will be shown underneath
 * a Kendo control or native HTML form-bound component after a validation.
 */
class ErrorComponent {
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
        this.id = `kendo-error-${serial$3++}`;
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

let serial$4 = 0;
/**
 * Represents a hint message that will be shown underneath a form-bound component.
 */
class HintComponent {
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
        this.id = `kendo-hint-${serial$4++}`;
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

/**
 * Specifies a container for form-bound controls (Kendo controls or native HTML controls).
 * Applies styling and behavior rules.
 */
class FormFieldComponent {
    constructor(renderer, localizationService) {
        this.renderer = renderer;
        this.localizationService = localizationService;
        this.hostClass = true;
        /**
         *
         * Specifies when the Hint messages will be shown.
         *
         * The possible values are:
         *
         * * (Default) `initial`&mdash;Allows displaying hints when the form-bound component state is
         * `valid` or `untouched` and `pristine`.
         * * `always`&mdash;Allows full control over the visibility of the hints.
         *
         */
        this.showHints = 'initial';
        /**
         * Specifies the layout orientation of the form field.
         *
         * * The possible values are:
         *
         * * (Default) `vertical`
         * * `horizontal`
         */
        this.orientation = 'vertical';
        /**
         * Specifies when the Error messages will be shown.
         *
         * The possible values are:
         *
         * * (Default) `initial`&mdash;Allows displaying errors when the form-bound component state is
         * `invalid` and `touched` or `dirty`.
         * * `always`&mdash;Allows full control over the visibility of the errors.
         *
         */
        this.showErrors = 'initial';
        this.subscriptions = new Subscription();
        this.rtl = false;
        this.subscriptions.add(this.localizationService.changes.subscribe(({ rtl }) => {
            this.rtl = rtl;
            this.direction = this.rtl ? 'rtl' : 'ltr';
        }));
    }
    get errorClass() {
        if (!this.control) {
            return false;
        }
        return this.control.invalid && (this.control.touched || this.control.dirty);
    }
    get disabledClass() {
        if (!this.control) {
            return false;
        }
        // radiobutton group
        if (this.isRadioControl(this.control)) {
            return false;
        }
        return this.disabledControl() ||
            this.disabledElement() ||
            this.disabledKendoInput();
    }
    set formControls(formControls) {
        this.validateFormControl(formControls);
        this.control = formControls.first;
    }
    /**
     * @hidden
     */
    get horizontal() {
        return this.orientation === 'horizontal';
    }
    /**
     * @hidden
     */
    get hasHints() {
        return this.showHints === 'always' ? true : this.showHintsInitial();
    }
    /**
     * @hidden
     */
    get hasErrors() {
        return this.showErrors === 'always' ? true : this.showErrorsInitial();
    }
    ngAfterViewInit() {
        this.setDescription();
    }
    ngOnDestroy() {
        this.subscriptions.unsubscribe();
    }
    disabledKendoInput() {
        return this.kendoInput && this.kendoInput.disabled;
    }
    disabledControl() {
        return this.control.disabled;
    }
    disabledElement() {
        const elements = this.controlElementRefs.toArray();
        return elements.every(e => e.nativeElement.hasAttribute('disabled'));
    }
    validateFormControl(formControls) {
        if (isDevMode() && formControls.length !== 1 && !this.isControlGroup(formControls)) {
            throw new Error('The `kendo-formfield` component should contain ' +
                'only one control of type NgControl with a formControlName(https://angular.io/api/forms/FormControlName)' +
                'or an ngModel(https://angular.io/api/forms/NgModel) binding.');
        }
    }
    isControlGroup(formControls) {
        if (!formControls.length) {
            return false;
        }
        const name = formControls.first.name;
        return formControls.toArray().every(c => c.name === name && (this.isRadioControl(c)));
    }
    isRadioControl(control) {
        return control.valueAccessor instanceof RadioControlValueAccessor;
    }
    updateDescription() {
        const controls = this.findControlElements();
        if (!controls) {
            return;
        }
        controls.forEach((control) => {
            const ariaIds = this.generateDescriptionIds(control);
            this.renderer.setAttribute(control, 'aria-describedby', ariaIds);
        });
    }
    findControlElements() {
        if (!this.controlElementRefs) {
            return;
        }
        // if the control is KendoInput and has focusableId - dropdowns, dateinputs
        if (this.kendoInput && this.kendoInput.focusableId && isDocumentAvailable()) {
            return [document.getElementById(this.kendoInput.focusableId)];
        }
        return this.controlElementRefs.map(el => el.nativeElement);
    }
    generateDescriptionIds(control) {
        const ids = new Set();
        if (control.hasAttribute('aria-describedby')) {
            const attributes = control.getAttribute('aria-describedby').split(' ');
            attributes.forEach((attr) => {
                if (attr.includes('kendo-hint-') || attr.includes('kendo-error-')) {
                    return;
                }
                ids.add(attr);
            });
        }
        this.hintChildren.forEach((hint) => {
            ids.add(hint.id);
        });
        this.errorChildren.forEach((error) => {
            ids.add(error.id);
        });
        return Array.from(ids).join(' ');
    }
    showHintsInitial() {
        if (!this.control) {
            return true;
        }
        const { valid, untouched, pristine } = this.control;
        return valid || (untouched && pristine);
    }
    showErrorsInitial() {
        if (!this.control) {
            return false;
        }
        const { invalid, dirty, touched } = this.control;
        return invalid && (dirty || touched);
    }
    setDescription() {
        this.updateDescription();
        this.subscriptions.add(this.errorChildren.changes.subscribe(() => this.updateDescription()));
        this.subscriptions.add(this.hintChildren.changes.subscribe(() => this.updateDescription()));
    }
}
FormFieldComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-formfield',
                template: `
        <ng-content select="label, kendo-label"></ng-content>
        <div [class.k-form-field-wrap]="horizontal">
            <ng-content></ng-content>
            <ng-content select="kendo-formhint" *ngIf="hasHints"></ng-content>
            <ng-content select="kendo-formerror" *ngIf="hasErrors"></ng-content>
        </div>
    `,
                providers: [
                    LocalizationService,
                    {
                        provide: L10N_PREFIX,
                        useValue: 'kendo.formfield'
                    }
                ]
            },] },
];
/** @nocollapse */
FormFieldComponent.ctorParameters = () => [
    { type: Renderer2 },
    { type: LocalizationService }
];
FormFieldComponent.propDecorators = {
    hostClass: [{ type: HostBinding, args: ['class.k-form-field',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    errorClass: [{ type: HostBinding, args: ['class.k-form-field-error',] }],
    disabledClass: [{ type: HostBinding, args: ['class.k-form-field-disabled',] }],
    formControls: [{ type: ContentChildren, args: [NgControl, { descendants: true, static: true },] }],
    controlElementRefs: [{ type: ContentChildren, args: [NgControl, { read: ElementRef, descendants: true, static: true },] }],
    kendoInput: [{ type: ContentChild, args: [KendoInput, { static: true },] }],
    errorChildren: [{ type: ContentChildren, args: [ErrorComponent, { descendants: true, static: true },] }],
    hintChildren: [{ type: ContentChildren, args: [HintComponent, { descendants: true, static: true },] }],
    showHints: [{ type: Input }],
    orientation: [{ type: Input }],
    showErrors: [{ type: Input }]
};

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the FormField, Error and Hint components.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the FormField module
 * import { FormFieldModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, FormFieldModule], // import FormField module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class FormFieldModule {
}
FormFieldModule.decorators = [
    { type: NgModule, args: [{
                declarations: [HintComponent, ErrorComponent, FormFieldComponent],
                exports: [HintComponent, ErrorComponent, FormFieldComponent],
                imports: [CommonModule]
            },] },
];

/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the Inputs components.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Inputs module
 * import { InputsModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 * import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, BrowserAnimationsModule, InputsModule], // import Inputs module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
class InputsModule {
}
InputsModule.decorators = [
    { type: NgModule, args: [{
                exports: [TextBoxModule, SliderModule, RangeSliderModule, SwitchModule, NumericTextBoxModule, MaskedTextBoxModule, ColorPickerModule, CheckBoxModule, RadioButtonModule, FormFieldModule],
                imports: [CommonModule]
            },] },
];

/**
 * Generated bundle index. Do not edit.
 */

export { CheckBoxModule, ColorInputComponent, FocusOnDomReadyDirective, ColorPaletteService, MaskingService, NumericTextBoxMessages, RadioButtonModule, RangeSliderCustomMessagesComponent, RangeSliderMessages, SliderCustomMessagesComponent, SliderMessages, SliderBase, SlidersCommonModule, SwitchCustomMessagesComponent, Messages, TextBoxCustomMessagesComponent, LocalizedTextBoxMessagesDirective, TextBoxMessages, TextBoxPrefixTemplateDirective, TextBoxSeparatorComponent, TextBoxSuffixTemplateDirective, TextBoxComponent, SliderComponent, RangeSliderComponent, LabelTemplateDirective, SwitchComponent, TextBoxContainerComponent, TextBoxDirective, TextAreaDirective, NumericTextBoxComponent, NumericTextBoxCustomMessagesComponent, MaskedTextBoxComponent, InputsModule, SliderTicksComponent, SliderModule, RangeSliderModule, SwitchModule, NumericTextBoxModule, MaskedTextBoxModule, TextBoxModule, ColorPickerComponent, ColorPaletteComponent, ColorGradientComponent, ColorPickerModule, ActiveColorClickEvent, CheckBoxDirective, RadioButtonDirective, HintComponent, ErrorComponent, FormFieldComponent, FormFieldModule, LocalizedNumericTextBoxMessagesDirective, LocalizedSliderMessagesDirective, LocalizedRangeSliderMessagesDirective, LocalizedSwitchMessagesDirective };
