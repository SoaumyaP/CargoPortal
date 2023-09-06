/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var core_1 = require("@angular/core");
var forms_1 = require("@angular/forms");
var rxjs_1 = require("rxjs");
var operators_1 = require("rxjs/operators");
var sliders_util_1 = require("../sliders-common/sliders-util");
var slider_model_1 = require("./slider-model");
var kendo_angular_l10n_1 = require("@progress/kendo-angular-l10n");
var utils_1 = require("../common/utils");
var sliders_util_2 = require("../sliders-common/sliders-util");
var dom_utils_1 = require("../common/dom-utils");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
var slider_base_1 = require("../sliders-common/slider-base");
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
        _this.focusableId = "k-" + kendo_angular_common_1.guid();
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
            if (kendo_angular_common_1.hasObservers(_this.onFocus)) {
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
            if (kendo_angular_common_1.hasObservers(_this.onBlur) || utils_1.requiresZoneOnBlur(_this.control)) {
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
            if (!_this.isDisabled && !(sliders_util_2.isButton(target) || sliders_util_2.isButton(target.parentNode))) {
                var value = sliders_util_2.eventValue(args, _this.track.nativeElement, _this.getProps());
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
            _this.changeValue(sliders_util_1.trimValue(max, min, value));
            e.preventDefault();
        };
        _this.ngChange = function (_) { };
        _this.ngTouched = function () { };
        _this.decreaseValue = function () {
            _this.changeValue(sliders_util_2.decreaseValueToStep(_this.value, _this.getProps()));
        };
        _this.increaseValue = function () {
            _this.changeValue(sliders_util_2.increaseValueToStep(_this.value, _this.getProps()));
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
        dom_utils_1.invokeElementMethod(this.wrapper, 'focus');
    };
    /**
     * Blurs the Slider.
     */
    SliderComponent.prototype.blur = function () {
        dom_utils_1.invokeElementMethod(this.wrapper, 'blur');
    };
    SliderComponent.prototype.ngOnChanges = function (changes) {
        var _this = this;
        if (kendo_angular_common_1.anyChanged(['value', 'fixedTickWidth', 'tickPlacement'], changes, true)) {
            this.ngZone.onStable.asObservable().pipe(operators_1.take(1)).subscribe(function () {
                _this.sizeComponent(false);
            });
        }
    };
    SliderComponent.prototype.ngAfterViewInit = function () {
        var _this = this;
        if (!kendo_angular_common_1.isDocumentAvailable()) {
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
        this.changeValue(sliders_util_2.eventValue(args, this.track.nativeElement, this.getProps()));
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
        if (!utils_1.areSame(this.value, value)) {
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
        if (!kendo_angular_common_1.isDocumentAvailable()) {
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
        var model = new slider_model_1.SliderModel(props, wrapper, track, this.renderer);
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
            var mousedown = rxjs_1.fromEvent(element, 'mousedown');
            var mouseup = rxjs_1.fromEvent(element, 'mouseup');
            var mouseout = rxjs_1.fromEvent(element, 'mouseout');
            var subscription = mousedown.pipe(operators_1.filter(function (e) { return e.button === 0 && !_this.isDisabled; }), operators_1.concatMap(function () {
                return rxjs_1.interval(150).pipe(operators_1.startWith(-1), operators_1.takeUntil(rxjs_1.merge(mouseup, mouseout)));
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
            value: sliders_util_1.trimValue(this.max, this.min, this.value),
            vertical: this.vertical
        };
    };
    SliderComponent.decorators = [
        { type: core_1.Component, args: [{
                    exportAs: 'kendoSlider',
                    providers: [
                        kendo_angular_l10n_1.LocalizationService,
                        { provide: kendo_angular_l10n_1.L10N_PREFIX, useValue: 'kendo.slider' },
                        { multi: true, provide: forms_1.NG_VALUE_ACCESSOR, useExisting: core_1.forwardRef(function () { return SliderComponent; }) },
                        { provide: kendo_angular_common_1.KendoInput, useExisting: core_1.forwardRef(function () { return SliderComponent; }) }
                    ],
                    selector: 'kendo-slider',
                    template: "\n        <ng-container kendoSliderLocalizedMessages\n            i18n-increment=\"kendo.slider.increment|The title of the **Increase** button of the Slider.\"\n            increment=\"increment\"\n            i18n-decrement=\"kendo.slider.decrement|The title of the **Decrease** button of the Slider.\"\n            decrement=\"decrement\"\n            i18n-dragHandle=\"kendo.slider.dragHandle|The title of the drag handle of the Slider.\"\n            dragHandle=\"Drag\"\n        >\n        <div class=\"k-slider-wrap\" #wrap\n            [id]=\"focusableId\"\n            [class.k-slider-buttons]=\"showButtons\"\n            [class.k-slider-topleft]=\"tickPlacement === 'before'\"\n            [class.k-slider-bottomright]=\"tickPlacement === 'after'\"\n            [attr.tabindex]=\"disabled ? undefined : tabIndex\"\n            [kendoEventsOutsideAngular]=\"{ focus: handleFocus, blur: handleBlur, click: onWrapClick, keydown: onKeyDown }\"\n            >\n            <a\n                #decreaseButton\n                *ngIf=\"showButtons\"\n                class=\"k-button k-button-decrease\"\n                [title]=\"decrementMessage\"\n                [attr.aria-label]=\"decrementMessage\"\n            >\n                <span class=\"k-icon\"\n                    [class.k-i-arrow-w]=\"!vertical\"\n                    [class.k-i-arrow-s]=\"vertical\"\n                >\n                </span>\n            </a>\n            <a\n                *ngIf=\"showButtons\"\n                #increaseButton\n                class=\"k-button k-button-increase\"\n                [title]=\"incrementMessage\"\n                [attr.aria-label]=\"incrementMessage\"\n            >\n                <span class=\"k-icon\"\n                    [class.k-i-arrow-e]=\"!vertical\"\n                    [class.k-i-arrow-n]=\"vertical\"\n                >\n                </span>\n            </a>\n            <ul kendoSliderTicks\n                #ticks\n                *ngIf=\"tickPlacement !== 'none'\"\n                [tickTitle]=\"title\"\n                [vertical]=\"vertical\"\n                [step]=\"smallStep\"\n                [largeStep]=\"largeStep\"\n                [min]=\"min\"\n                [max]=\"max\"\n                [labelTemplate]=\"labelTemplate?.templateRef\"\n            >\n            </ul>\n        <div #track class=\"k-slider-track\">\n            <div #sliderSelection class=\"k-slider-selection\">\n            </div>\n                <a #draghandle\n                    role=\"slider\"\n                    [attr.aria-valuemin]=\"min\"\n                    [attr.aria-valuemax]=\"max\"\n                    [attr.aria-valuenow]=\"value\"\n                    [attr.aria-disabled]=\"disabled ? true : undefined\"\n                    [attr.aria-readonly]=\"readonly ? true : undefined\"\n                    [attr.aria-orientation]=\"vertical ? 'vertical' : 'horizontal'\"\n                    [style.touch-action]=\"isDisabled ? '' : 'none'\"\n                    class=\"k-draghandle\"\n                    [title]=\"dragHandleMessage\"\n                    kendoDraggable\n                    (kendoPress)=\"ifEnabled(handleDragPress, $event)\"\n                    (kendoDrag)=\"ifEnabled(onHandleDrag, $event)\"\n                    (kendoRelease)=\"ifEnabled(onHandleRelease, $event)\"\n                ></a>\n            </div>\n            <kendo-resize-sensor (resize)=\"sizeComponent(false)\"></kendo-resize-sensor>\n        </div>\n  "
                },] },
    ];
    /** @nocollapse */
    SliderComponent.ctorParameters = function () { return [
        { type: kendo_angular_l10n_1.LocalizationService },
        { type: core_1.Injector },
        { type: core_1.Renderer2 },
        { type: core_1.NgZone },
        { type: core_1.ChangeDetectorRef },
        { type: core_1.ElementRef }
    ]; };
    SliderComponent.propDecorators = {
        focusableId: [{ type: core_1.Input }],
        dragHandleTitle: [{ type: core_1.Input }],
        incrementTitle: [{ type: core_1.Input }],
        animate: [{ type: core_1.Input }],
        decrementTitle: [{ type: core_1.Input }],
        showButtons: [{ type: core_1.Input }],
        value: [{ type: core_1.Input }],
        tabIndex: [{ type: core_1.Input }],
        draghandle: [{ type: core_1.ViewChild, args: ['draghandle', { static: true },] }],
        decreaseButton: [{ type: core_1.ViewChild, args: ['decreaseButton',] }],
        increaseButton: [{ type: core_1.ViewChild, args: ['increaseButton',] }]
    };
    return SliderComponent;
}(slider_base_1.SliderBase));
exports.SliderComponent = SliderComponent;
