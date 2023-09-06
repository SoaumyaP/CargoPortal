/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
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
const FOCUSED = 'k-state-focused';
const PRESSED = 'k-pressed';
/**
 * Represents the [Kendo UI Slider component for Angular]({% slug overview_slider %}).
 */
export class SliderComponent extends SliderBase {
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
