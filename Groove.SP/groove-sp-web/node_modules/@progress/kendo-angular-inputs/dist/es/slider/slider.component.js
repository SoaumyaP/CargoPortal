/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { Renderer2, Component, ElementRef, Input, ViewChild, forwardRef, NgZone, Injector, ChangeDetectorRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { fromEvent, interval, merge } from 'rxjs';
import { filter, concatMap, startWith, takeUntil, take } from 'rxjs/operators';
import { trimValue } from '../sliders-common/sliders-util';
import { SliderModel } from './slider-model';
import { L10N_PREFIX, LocalizationService } from '@progress/kendo-angular-l10n';
import { areSame, requiresZoneOnBlur } from '../common/utils';
import { isButton, eventValue, decreaseValueToStep, increaseValueToStep } from '../sliders-common/sliders-util';
import { invokeElementMethod } from '../common/dom-utils';
import { guid, isDocumentAvailable, hasObservers, KendoInput, anyChanged } from '@progress/kendo-angular-common';
import { SliderBase } from '../sliders-common/slider-base';
var FOCUSED = 'k-state-focused';
var PRESSED = 'k-pressed';
/**
 * Represents the [Kendo UI Slider component for Angular]({% slug overview_slider %}).
 */
var SliderComponent = /** @class */ (function (_super) {
    tslib_1.__extends(SliderComponent, _super);
    function SliderComponent(localization, injector, renderer, ngZone, changeDetector, hostElement) {
        var _this = _super.call(this, localization, injector, renderer, ngZone, changeDetector, hostElement) || this;
        _this.localization = localization;
        _this.injector = injector;
        _this.renderer = renderer;
        _this.ngZone = ngZone;
        _this.changeDetector = changeDetector;
        _this.hostElement = hostElement;
        /**
         * @hidden
         */
        _this.focusableId = "k-" + guid();
        /**
         * Determines if the animation will be played on value change.
         * Regardless of this setting, no animation will be played during the initial rendering.
         */
        _this.animate = true;
        /**
         * Renders the arrow side buttons of the Slider ([see example]({% slug sidebuttons_slider %}#toc-hidden-state)).
         * When `showButtons` is set to `false`, the buttons are not displayed.
         */
        _this.showButtons = true;
        /**
         * The current value of the Slider when it is initially displayed.
         * The component can use either NgModel or the `value` binding but not both of them at the same time.
         */
        _this.value = _this.min;
        /**
         * @hidden
         */
        _this.handleFocus = function () {
            _this.focused = true;
            if (hasObservers(_this.onFocus)) {
                _this.ngZone.run(function () {
                    _this.onFocus.emit();
                });
            }
        };
        /**
         * @hidden
         */
        _this.handleBlur = function () {
            _this.changeDetector.markForCheck();
            _this.focused = false;
            if (hasObservers(_this.onBlur) || requiresZoneOnBlur(_this.control)) {
                _this.ngZone.run(function () {
                    _this.ngTouched();
                    _this.onBlur.emit();
                });
            }
        };
        /**
         * @hidden
         */
        _this.onWrapClick = function (args) {
            var target = args.target;
            if (!_this.isDisabled && !(isButton(target) || isButton(target.parentNode))) {
                var value = eventValue(args, _this.track.nativeElement, _this.getProps());
                _this.changeValue(value);
            }
        };
        /**
         * @hidden
         */
        _this.onKeyDown = function (e) {
            var options = _this.getProps();
            var max = options.max, min = options.min;
            var handler = _this.keyBinding[e.keyCode];
            if (_this.isDisabled || !handler) {
                return;
            }
            var value = handler(options);
            _this.changeValue(trimValue(max, min, value));
            e.preventDefault();
        };
        _this.ngChange = function (_) { };
        _this.ngTouched = function () { };
        _this.decreaseValue = function () {
            _this.changeValue(decreaseValueToStep(_this.value, _this.getProps()));
        };
        _this.increaseValue = function () {
            _this.changeValue(increaseValueToStep(_this.value, _this.getProps()));
        };
        return _this;
    }
    Object.defineProperty(SliderComponent.prototype, "tabIndex", {
        get: function () {
            return this.tabindex;
        },
        /**
         * @hidden
         */
        set: function (tabIndex) {
            this.tabindex = tabIndex;
        },
        enumerable: true,
        configurable: true
    });
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
    SliderComponent.prototype.focus = function () {
        invokeElementMethod(this.wrapper, 'focus');
    };
    /**
     * Blurs the Slider.
     */
    SliderComponent.prototype.blur = function () {
        invokeElementMethod(this.wrapper, 'blur');
    };
    SliderComponent.prototype.ngOnChanges = function (changes) {
        var _this = this;
        if (anyChanged(['value', 'fixedTickWidth', 'tickPlacement'], changes, true)) {
            this.ngZone.onStable.asObservable().pipe(take(1)).subscribe(function () {
                _this.sizeComponent(false);
            });
        }
    };
    SliderComponent.prototype.ngAfterViewInit = function () {
        var _this = this;
        if (!isDocumentAvailable()) {
            return;
        }
        if (this.showButtons) {
            this.setValueChangeInterval(this.increaseButton.nativeElement, function () { return _this.increaseValue(); });
            this.setValueChangeInterval(this.decreaseButton.nativeElement, function () { return _this.decreaseValue(); });
        }
        this.sizeComponent(false);
        if (this.ticks) {
            this.ticks.tickElements
                .changes
                .subscribe(function () { return _this.sizeComponent(false); });
        }
    };
    SliderComponent.prototype.ngOnDestroy = function () {
        this.subscriptions.unsubscribe();
    };
    Object.defineProperty(SliderComponent.prototype, "incrementMessage", {
        /**
         * @hidden
         */
        get: function () {
            return this.incrementTitle || this.localizationService.get('increment');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(SliderComponent.prototype, "decrementMessage", {
        /**
         * @hidden
         */
        get: function () {
            return this.decrementTitle || this.localizationService.get('decrement');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(SliderComponent.prototype, "dragHandleMessage", {
        /**
         * @hidden
         */
        get: function () {
            return this.dragHandleTitle || this.localizationService.get('dragHandle');
        },
        enumerable: true,
        configurable: true
    });
    /**
     * @hidden
     */
    SliderComponent.prototype.handleDragPress = function (args) {
        if (args.originalEvent) {
            args.originalEvent.preventDefault();
        }
        this.focus();
        this.renderer.removeClass(this.hostElement.nativeElement, 'k-slider-transitions');
    };
    /**
     * @hidden
     */
    SliderComponent.prototype.onHandleDrag = function (args) {
        this.dragging = true;
        this.changeValue(eventValue(args, this.track.nativeElement, this.getProps()));
    };
    /**
     * @hidden
     */
    SliderComponent.prototype.onHandleRelease = function () {
        this.dragging = false; //needed for animation
        this.renderer.addClass(this.hostElement.nativeElement, 'k-slider-transitions');
    };
    //ngModel binding
    /**
     * @hidden
     */
    SliderComponent.prototype.writeValue = function (value) {
        this.value = value;
        this.sizeComponent(this.animate);
    };
    /**
     * @hidden
     */
    SliderComponent.prototype.registerOnChange = function (fn) {
        this.ngChange = fn;
    };
    /**
     * @hidden
     */
    SliderComponent.prototype.registerOnTouched = function (fn) {
        this.ngTouched = fn;
    };
    /**
     * @hidden
     */
    SliderComponent.prototype.changeValue = function (value) {
        var _this = this;
        if (!areSame(this.value, value)) {
            this.ngZone.run(function () {
                _this.value = value;
                _this.ngChange(value);
                _this.valueChange.emit(value);
                _this.sizeComponent(_this.animate);
            });
        }
    };
    /**
     * @hidden
     */
    SliderComponent.prototype.sizeComponent = function (animate) {
        if (!isDocumentAvailable()) {
            return;
        }
        var wrapper = this.wrapper.nativeElement;
        var track = this.track.nativeElement;
        var selectionEl = this.sliderSelection.nativeElement;
        var dragHandleEl = this.draghandle.nativeElement;
        var ticks = this.ticks ? this.ticksContainer.nativeElement : null;
        if (!animate) {
            this.renderer.removeClass(this.hostElement.nativeElement, 'k-slider-transitions');
        }
        this.resetStyles([track, selectionEl, dragHandleEl, ticks, this.hostElement.nativeElement]);
        var props = this.getProps();
        var model = new SliderModel(props, wrapper, track, this.renderer);
        model.resizeTrack();
        if (this.ticks) { //for case when tickPlacement: none
            model.resizeTicks(this.ticksContainer.nativeElement, this.ticks.tickElements.map(function (element) { return element.nativeElement; }));
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
    };
    Object.defineProperty(SliderComponent.prototype, "focused", {
        set: function (value) {
            if (this.isFocused !== value && this.hostElement) {
                var element = this.hostElement.nativeElement;
                if (value) {
                    this.renderer.addClass(element, FOCUSED);
                }
                else {
                    this.renderer.removeClass(element, FOCUSED);
                }
                this.isFocused = value;
            }
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(SliderComponent.prototype, "dragging", {
        set: function (value) {
            if (this.isDragged !== value && this.sliderSelection && this.draghandle) {
                var sliderSelection = this.sliderSelection.nativeElement;
                var draghandle = this.draghandle.nativeElement;
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
        },
        enumerable: true,
        configurable: true
    });
    SliderComponent.prototype.setValueChangeInterval = function (element, callback) {
        var _this = this;
        this.ngZone.runOutsideAngular(function () {
            var mousedown = fromEvent(element, 'mousedown');
            var mouseup = fromEvent(element, 'mouseup');
            var mouseout = fromEvent(element, 'mouseout');
            var subscription = mousedown.pipe(filter(function (e) { return e.button === 0 && !_this.isDisabled; }), concatMap(function () {
                return interval(150).pipe(startWith(-1), takeUntil(merge(mouseup, mouseout)));
            })).subscribe(function () {
                _this.focus();
                callback();
            });
            _this.subscriptions.add(subscription);
        });
    };
    SliderComponent.prototype.getProps = function () {
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
    };
    SliderComponent.decorators = [
        { type: Component, args: [{
                    exportAs: 'kendoSlider',
                    providers: [
                        LocalizationService,
                        { provide: L10N_PREFIX, useValue: 'kendo.slider' },
                        { multi: true, provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(function () { return SliderComponent; }) },
                        { provide: KendoInput, useExisting: forwardRef(function () { return SliderComponent; }) }
                    ],
                    selector: 'kendo-slider',
                    template: "\n        <ng-container kendoSliderLocalizedMessages\n            i18n-increment=\"kendo.slider.increment|The title of the **Increase** button of the Slider.\"\n            increment=\"increment\"\n            i18n-decrement=\"kendo.slider.decrement|The title of the **Decrease** button of the Slider.\"\n            decrement=\"decrement\"\n            i18n-dragHandle=\"kendo.slider.dragHandle|The title of the drag handle of the Slider.\"\n            dragHandle=\"Drag\"\n        >\n        <div class=\"k-slider-wrap\" #wrap\n            [id]=\"focusableId\"\n            [class.k-slider-buttons]=\"showButtons\"\n            [class.k-slider-topleft]=\"tickPlacement === 'before'\"\n            [class.k-slider-bottomright]=\"tickPlacement === 'after'\"\n            [attr.tabindex]=\"disabled ? undefined : tabIndex\"\n            [kendoEventsOutsideAngular]=\"{ focus: handleFocus, blur: handleBlur, click: onWrapClick, keydown: onKeyDown }\"\n            >\n            <a\n                #decreaseButton\n                *ngIf=\"showButtons\"\n                class=\"k-button k-button-decrease\"\n                [title]=\"decrementMessage\"\n                [attr.aria-label]=\"decrementMessage\"\n            >\n                <span class=\"k-icon\"\n                    [class.k-i-arrow-w]=\"!vertical\"\n                    [class.k-i-arrow-s]=\"vertical\"\n                >\n                </span>\n            </a>\n            <a\n                *ngIf=\"showButtons\"\n                #increaseButton\n                class=\"k-button k-button-increase\"\n                [title]=\"incrementMessage\"\n                [attr.aria-label]=\"incrementMessage\"\n            >\n                <span class=\"k-icon\"\n                    [class.k-i-arrow-e]=\"!vertical\"\n                    [class.k-i-arrow-n]=\"vertical\"\n                >\n                </span>\n            </a>\n            <ul kendoSliderTicks\n                #ticks\n                *ngIf=\"tickPlacement !== 'none'\"\n                [tickTitle]=\"title\"\n                [vertical]=\"vertical\"\n                [step]=\"smallStep\"\n                [largeStep]=\"largeStep\"\n                [min]=\"min\"\n                [max]=\"max\"\n                [labelTemplate]=\"labelTemplate?.templateRef\"\n            >\n            </ul>\n        <div #track class=\"k-slider-track\">\n            <div #sliderSelection class=\"k-slider-selection\">\n            </div>\n                <a #draghandle\n                    role=\"slider\"\n                    [attr.aria-valuemin]=\"min\"\n                    [attr.aria-valuemax]=\"max\"\n                    [attr.aria-valuenow]=\"value\"\n                    [attr.aria-disabled]=\"disabled ? true : undefined\"\n                    [attr.aria-readonly]=\"readonly ? true : undefined\"\n                    [attr.aria-orientation]=\"vertical ? 'vertical' : 'horizontal'\"\n                    [style.touch-action]=\"isDisabled ? '' : 'none'\"\n                    class=\"k-draghandle\"\n                    [title]=\"dragHandleMessage\"\n                    kendoDraggable\n                    (kendoPress)=\"ifEnabled(handleDragPress, $event)\"\n                    (kendoDrag)=\"ifEnabled(onHandleDrag, $event)\"\n                    (kendoRelease)=\"ifEnabled(onHandleRelease, $event)\"\n                ></a>\n            </div>\n            <kendo-resize-sensor (resize)=\"sizeComponent(false)\"></kendo-resize-sensor>\n        </div>\n  "
                },] },
    ];
    /** @nocollapse */
    SliderComponent.ctorParameters = function () { return [
        { type: LocalizationService },
        { type: Injector },
        { type: Renderer2 },
        { type: NgZone },
        { type: ChangeDetectorRef },
        { type: ElementRef }
    ]; };
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
    return SliderComponent;
}(SliderBase));
export { SliderComponent };
