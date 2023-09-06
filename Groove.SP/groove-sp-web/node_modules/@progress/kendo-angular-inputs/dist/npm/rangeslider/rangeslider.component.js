/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var core_1 = require("@angular/core");
var forms_1 = require("@angular/forms");
var operators_1 = require("rxjs/operators");
var sliders_util_1 = require("../sliders-common/sliders-util");
var rangeslider_model_1 = require("./rangeslider-model");
var kendo_angular_l10n_1 = require("@progress/kendo-angular-l10n");
var sliders_util_2 = require("../sliders-common/sliders-util");
var dom_utils_1 = require("../common/dom-utils");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
var utils_1 = require("../common/utils");
var slider_base_1 = require("../sliders-common/slider-base");
var PRESSED = 'k-pressed';
/**
 * Represents the [Kendo UI RangeSlider component for Angular]({% slug overview_rangeslider %}).
 */
var RangeSliderComponent = /** @class */ (function (_super) {
    tslib_1.__extends(RangeSliderComponent, _super);
    function RangeSliderComponent(localization, injector, renderer, ngZone, changeDetector, hostElement) {
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
        _this.startHandleId = "k-start-handle-" + kendo_angular_common_1.guid();
        /**
         * @hidden
         */
        _this.endHandleId = "k-end-handle-" + kendo_angular_common_1.guid();
        /**
         * @hidden
         */
        _this.focusableId = _this.startHandleId;
        _this.handleZIndex = 0;
        _this.activeHandle = 'startHandle';
        _this.focusChangedProgrammatically = false;
        /**
         * @hidden
         */
        _this.onWrapClick = function (args) {
            if (!_this.isDisabled) {
                _this.value = _this.value || [_this.min, _this.min];
                var trackValue = sliders_util_2.eventValue(args, _this.track.nativeElement, _this.getProps());
                var newRangeValue = void 0;
                var _a = newRangeValue = _this.value, startValue = _a[0], endValue = _a[1];
                if (trackValue <= startValue) {
                    newRangeValue = [trackValue, endValue];
                    _this.activeHandle = 'startHandle';
                }
                else if (startValue < trackValue && trackValue < endValue) {
                    if (trackValue < (startValue + endValue) / 2) {
                        newRangeValue = [trackValue, endValue];
                        _this.activeHandle = 'startHandle';
                    }
                    else {
                        newRangeValue = [startValue, trackValue];
                        _this.activeHandle = 'endHandle';
                    }
                }
                else if (trackValue >= endValue) {
                    newRangeValue = [startValue, trackValue];
                    _this.activeHandle = 'endHandle';
                }
                var activeHandle = _this.activeHandle === 'startHandle' ? _this.draghandleStart : _this.draghandleEnd;
                dom_utils_1.invokeElementMethod(activeHandle, 'focus');
                _this.changeValue(newRangeValue);
            }
        };
        /**
         * @hidden
         */
        _this.onKeyDown = function (e) {
            _this.value = _this.value || [_this.min, _this.min];
            var options = _this.getProps();
            var max = options.max, min = options.min;
            var handler = _this.keyBinding[e.keyCode];
            if (_this.isDisabled || !handler) {
                return;
            }
            _this.renderer.setStyle(e.target, 'zIndex', ++_this.handleZIndex);
            var startHandleIsActive = sliders_util_2.isStartHandle(e.target);
            var value = handler(tslib_1.__assign({}, options, { value: startHandleIsActive ? _this.value[0] : _this.value[1] }));
            if (startHandleIsActive) {
                if (value > _this.value[1]) {
                    _this.value[1] = value;
                }
            }
            else {
                if (value < _this.value[0]) {
                    _this.value[0] = value;
                }
            }
            var trimmedValue = sliders_util_1.trimValue(max, min, value);
            var newValue = startHandleIsActive ? [trimmedValue, _this.value[1]]
                : [_this.value[0], trimmedValue];
            _this.changeValue(newValue);
            e.preventDefault();
        };
        _this.ngChange = function (_) { };
        _this.ngTouched = function () { };
        _this.handleBlur = function () {
            _this.changeDetector.markForCheck();
            _this.focused = false;
            if (kendo_angular_common_1.hasObservers(_this.onBlur) || utils_1.requiresZoneOnBlur(_this.control)) {
                _this.ngZone.run(function () {
                    _this.ngTouched();
                    if (!_this.focusChangedProgrammatically) {
                        _this.onBlur.emit();
                    }
                });
            }
        };
        return _this;
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
    RangeSliderComponent.prototype.focus = function () {
        this.focusChangedProgrammatically = true;
        dom_utils_1.invokeElementMethod(this.draghandleStart, 'focus');
        this.focusChangedProgrammatically = false;
    };
    /**
     * Blurs the RangeSlider.
     */
    RangeSliderComponent.prototype.blur = function () {
        this.focusChangedProgrammatically = true;
        var activeHandle = this.activeHandle === 'startHandle' ? this.draghandleStart : this.draghandleEnd;
        dom_utils_1.invokeElementMethod(activeHandle, 'blur');
        this.handleBlur();
        this.focusChangedProgrammatically = false;
    };
    RangeSliderComponent.prototype.ngOnInit = function () {
        if (!this.value) {
            this.value = [this.min, this.max];
        }
        _super.prototype.ngOnInit.call(this);
    };
    RangeSliderComponent.prototype.ngOnChanges = function (changes) {
        var _this = this;
        if (kendo_angular_common_1.anyChanged(['value', 'fixedTickWidth', 'tickPlacement'], changes, true)) {
            if (changes.value && changes.value.currentValue) {
                sliders_util_1.validateValue(changes.value.currentValue);
            }
            this.ngZone.onStable.asObservable().pipe(operators_1.take(1)).subscribe(function () {
                _this.sizeComponent();
            });
        }
    };
    RangeSliderComponent.prototype.ngAfterViewInit = function () {
        var _this = this;
        if (!kendo_angular_common_1.isDocumentAvailable()) {
            return;
        }
        this.sizeComponent();
        if (this.ticks) {
            this.ticks.tickElements
                .changes
                .subscribe(function () { return _this.sizeComponent(); });
        }
        this.attachElementEventHandlers();
    };
    RangeSliderComponent.prototype.ngOnDestroy = function () {
        this.subscriptions.unsubscribe();
    };
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.textFor = function (key) {
        return this.localization.get(key);
    };
    Object.defineProperty(RangeSliderComponent.prototype, "valueText", {
        /**
         * @hidden
         */
        get: function () {
            return this.value ? this.value[0] + " - " + this.value[1] : '';
        },
        enumerable: true,
        configurable: true
    });
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.handleDragPress = function (args) {
        if (args.originalEvent) {
            args.originalEvent.preventDefault();
        }
        var target = args.originalEvent.target;
        this.draggedHandle = target;
        this.renderer.setStyle(target, 'zIndex', ++this.handleZIndex);
    };
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.onHandleDrag = function (args) {
        var _this = this;
        this.value = this.value || [this.min, this.min];
        var target = args.originalEvent.target;
        var lastCoords = this.draggedHandle.getBoundingClientRect();
        this.lastHandlePosition = { x: lastCoords.left, y: lastCoords.top };
        this.dragging = { value: true, target: target };
        var left = args.pageX < this.lastHandlePosition.x;
        var right = args.pageX > this.lastHandlePosition.x;
        var up = args.pageY > this.lastHandlePosition.y;
        var moveStartHandle = function () { return _this.changeValue([sliders_util_2.eventValue(args, _this.track.nativeElement, _this.getProps()), _this.value[1]]); };
        var moveEndHandle = function () { return _this.changeValue([_this.value[0], sliders_util_2.eventValue(args, _this.track.nativeElement, _this.getProps())]); };
        var moveBothHandles = function () { return _this.changeValue([sliders_util_2.eventValue(args, _this.track.nativeElement, _this.getProps()), sliders_util_2.eventValue(args, _this.track.nativeElement, _this.getProps())]); };
        var activeStartHandle = sliders_util_2.isStartHandle(this.draggedHandle);
        var vertical = this.vertical;
        var horizontal = !vertical;
        var forward = (vertical && up) || (this.reverse ? horizontal && right : horizontal && left);
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
    };
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.onHandleRelease = function (args) {
        this.dragging = { value: false, target: args.originalEvent.target }; //needed for animation
        this.draggedHandle = undefined;
    };
    //ngModel binding
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.writeValue = function (value) {
        sliders_util_1.validateValue(value);
        this.value = value;
        this.sizeComponent();
    };
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.registerOnChange = function (fn) {
        this.ngChange = fn;
    };
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.registerOnTouched = function (fn) {
        this.ngTouched = fn;
    };
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.changeValue = function (value) {
        var _this = this;
        if (!this.value || !sliders_util_1.isSameRange(this.value, value)) {
            this.ngZone.run(function () {
                _this.value = value;
                _this.ngChange(value);
                if (_this.value) {
                    _this.valueChange.emit(value);
                }
                _this.sizeComponent();
            });
        }
    };
    /**
     * @hidden
     */
    RangeSliderComponent.prototype.sizeComponent = function () {
        if (!kendo_angular_common_1.isDocumentAvailable()) {
            return;
        }
        var wrapper = this.wrapper.nativeElement;
        var track = this.track.nativeElement;
        var selectionEl = this.sliderSelection.nativeElement;
        var dragHandleStartEl = this.draghandleStart.nativeElement;
        var dragHandleEndEl = this.draghandleEnd.nativeElement;
        var ticks = this.ticks ? this.ticksContainer.nativeElement : null;
        this.resetStyles([track, selectionEl, dragHandleStartEl, dragHandleEndEl, ticks, this.hostElement.nativeElement]);
        var props = this.getProps();
        var model = new rangeslider_model_1.RangeSliderModel(props, wrapper, track, this.renderer);
        model.resizeTrack();
        if (this.ticks) { //for case when tickPlacement: none
            model.resizeTicks(this.ticksContainer.nativeElement, this.ticks.tickElements.map(function (element) { return element.nativeElement; }));
        }
        model.positionHandle(dragHandleStartEl);
        model.positionHandle(dragHandleEndEl);
        model.positionSelection(dragHandleStartEl, selectionEl);
        if (this.fixedTickWidth) {
            model.resizeWrapper();
        }
    };
    Object.defineProperty(RangeSliderComponent.prototype, "isDisabled", {
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
    RangeSliderComponent.prototype.isEmpty = function () {
        return false;
    };
    Object.defineProperty(RangeSliderComponent.prototype, "focused", {
        set: function (value) {
            if (this.isFocused !== value && this.hostElement) {
                this.isFocused = value;
            }
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RangeSliderComponent.prototype, "dragging", {
        set: function (data) {
            if (this.isDragged !== data.value && this.sliderSelection && this.draghandleStart && this.draghandleEnd) {
                var sliderSelection = this.sliderSelection.nativeElement;
                var draghandle = data.target;
                if (data.value) {
                    this.renderer.addClass(sliderSelection, PRESSED);
                    this.renderer.addClass(draghandle, PRESSED);
                }
                else {
                    this.renderer.removeClass(sliderSelection, PRESSED);
                    this.renderer.removeClass(draghandle, PRESSED);
                }
                this.isDragged = data.value;
            }
        },
        enumerable: true,
        configurable: true
    });
    RangeSliderComponent.prototype.getProps = function () {
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
            value: sliders_util_1.trimValueRange(this.max, this.min, this.value),
            vertical: this.vertical,
            buttons: false
        };
    };
    RangeSliderComponent.prototype.attachElementEventHandlers = function () {
        var _this = this;
        var hostElement = this.hostElement.nativeElement;
        var tabbing = false;
        var cursorInsideWrapper = false;
        this.ngZone.runOutsideAngular(function () {
            // focusIn and focusOut are relative to the host element
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'focusin', function () {
                if (!_this.isFocused) {
                    _this.ngZone.run(function () {
                        if (!_this.focusChangedProgrammatically) {
                            _this.onFocus.emit();
                        }
                        _this.focused = true;
                    });
                }
            }));
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'focusout', function (args) {
                if (!_this.isFocused) {
                    return;
                }
                if (tabbing) {
                    if (args.relatedTarget !== _this.draghandleStart.nativeElement && args.relatedTarget !== _this.draghandleEnd.nativeElement) {
                        _this.handleBlur();
                    }
                    tabbing = false;
                }
                else {
                    if (!cursorInsideWrapper) {
                        _this.handleBlur();
                    }
                }
            }));
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'mouseenter', function () {
                cursorInsideWrapper = true;
            }));
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'mouseleave', function () {
                cursorInsideWrapper = false;
            }));
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'keydown', function (args) {
                if (args.keyCode === kendo_angular_common_1.Keys.Tab) {
                    tabbing = true;
                }
                else {
                    tabbing = false;
                }
            }));
        });
    };
    RangeSliderComponent.decorators = [
        { type: core_1.Component, args: [{
                    exportAs: 'kendoRangeSlider',
                    providers: [
                        kendo_angular_l10n_1.LocalizationService,
                        { provide: kendo_angular_l10n_1.L10N_PREFIX, useValue: 'kendo.rangeslider' },
                        { multi: true, provide: forms_1.NG_VALUE_ACCESSOR, useExisting: core_1.forwardRef(function () { return RangeSliderComponent; }) },
                        { provide: kendo_angular_common_1.KendoInput, useExisting: core_1.forwardRef(function () { return RangeSliderComponent; }) }
                    ],
                    selector: 'kendo-rangeslider',
                    template: "\n        <ng-container kendoSliderLocalizedMessages\n            i18n-dragHandleStart=\"kendo.rangeslider.dragHandleStart|The title of the **Start** drag handle of the Slider.\"\n            dragHandleStart=\"Drag\"\n            i18n-dragHandleEnd=\"kendo.rangeslider.dragHandleEnd|The title of the **End** drag handle of the Slider.\"\n            dragHandleEnd=\"Drag\"\n        >\n\n        <div class=\"k-slider-wrap\" #wrap\n            [class.k-slider-topleft]=\"tickPlacement === 'before'\"\n            [class.k-slider-bottomright]=\"tickPlacement === 'after'\"\n            [kendoEventsOutsideAngular]=\"{ click: onWrapClick, keydown: onKeyDown }\"\n            >\n            <ul kendoSliderTicks\n                #ticks\n                *ngIf=\"tickPlacement !== 'none'\"\n                [tickTitle]=\"title\"\n                [vertical]=\"vertical\"\n                [step]=\"smallStep\"\n                [largeStep]=\"largeStep\"\n                [min]=\"min\"\n                [max]=\"max\"\n                [labelTemplate]=\"labelTemplate?.templateRef\"\n            >\n            </ul>\n            <div #track class=\"k-slider-track\">\n                <div #sliderSelection class=\"k-slider-selection\">\n                </div>\n                <a #draghandleStart\n                    role=\"slider\"\n                    [id]=\"startHandleId\"\n                    [attr.tabindex]=\"disabled ? undefined : tabindex\"\n                    [attr.aria-valuemin]=\"min\"\n                    [attr.aria-valuemax]=\"max\"\n                    [attr.aria-valuenow]=\"value ? value[0] : null\"\n                    [attr.aria-valuetext]=\"valueText\"\n                    [attr.aria-disabled]=\"disabled ? true : undefined\"\n                    [attr.aria-readonly]=\"readonly ? true : undefined\"\n                    [attr.aria-orientation]=\"vertical ? 'vertical' : 'horizontal'\"\n                    [style.touch-action]=\"isDisabled ? '' : 'none'\"\n                    class=\"k-draghandle\"\n                    [title]=\"textFor('dragHandleStart')\"\n                    kendoDraggable\n                    (kendoPress)=\"ifEnabled(handleDragPress ,$event)\"\n                    (kendoDrag)=\"ifEnabled(onHandleDrag ,$event)\"\n                    (kendoRelease)=\"ifEnabled(onHandleRelease, $event)\"\n                ></a>\n                <a #draghandleEnd\n                    role=\"slider\"\n                    [id]=\"endHandleId\"\n                    [attr.tabindex]=\"disabled ? undefined : tabindex\"\n                    [attr.aria-valuemin]=\"min\"\n                    [attr.aria-valuemax]=\"max\"\n                    [attr.aria-valuenow]=\"value ? value[1] : null\"\n                    [attr.aria-valuetext]=\"valueText\"\n                    [attr.aria-disabled]=\"disabled ? true : undefined\"\n                    [attr.aria-readonly]=\"readonly ? true : undefined\"\n                    [attr.aria-orientation]=\"vertical ? 'vertical' : 'horizontal'\"\n                    [style.touch-action]=\"isDisabled ? '' : 'none'\"\n                    class=\"k-draghandle\"\n                    [title]=\"textFor('dragHandleEnd')\"\n                    kendoDraggable\n                    (kendoPress)=\"ifEnabled(handleDragPress ,$event)\"\n                    (kendoDrag)=\"ifEnabled(onHandleDrag ,$event)\"\n                    (kendoRelease)=\"ifEnabled(onHandleRelease, $event)\"\n                ></a>\n            </div>\n\n            <kendo-resize-sensor (resize)=\"sizeComponent()\"></kendo-resize-sensor>\n        </div>\n  "
                },] },
    ];
    /** @nocollapse */
    RangeSliderComponent.ctorParameters = function () { return [
        { type: kendo_angular_l10n_1.LocalizationService },
        { type: core_1.Injector },
        { type: core_1.Renderer2 },
        { type: core_1.NgZone },
        { type: core_1.ChangeDetectorRef },
        { type: core_1.ElementRef }
    ]; };
    RangeSliderComponent.propDecorators = {
        value: [{ type: core_1.Input }],
        draghandleStart: [{ type: core_1.ViewChild, args: ['draghandleStart', { static: true },] }],
        draghandleEnd: [{ type: core_1.ViewChild, args: ['draghandleEnd', { static: true },] }]
    };
    return RangeSliderComponent;
}(slider_base_1.SliderBase));
exports.RangeSliderComponent = RangeSliderComponent;
