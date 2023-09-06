/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Renderer2, Component, ElementRef, Input, ViewChild, forwardRef, NgZone, Injector, ChangeDetectorRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { take } from 'rxjs/operators';
import { trimValue, isSameRange, trimValueRange, validateValue } from '../sliders-common/sliders-util';
import { RangeSliderModel } from './rangeslider-model';
import { L10N_PREFIX, LocalizationService } from '@progress/kendo-angular-l10n';
import { eventValue, isStartHandle } from '../sliders-common/sliders-util';
import { invokeElementMethod } from '../common/dom-utils';
import { guid, isDocumentAvailable, Keys, KendoInput, anyChanged, hasObservers } from '@progress/kendo-angular-common';
import { requiresZoneOnBlur } from '../common/utils';
import { SliderBase } from '../sliders-common/slider-base';
const PRESSED = 'k-pressed';
/**
 * Represents the [Kendo UI RangeSlider component for Angular]({% slug overview_rangeslider %}).
 */
export class RangeSliderComponent extends SliderBase {
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
                this.renderer.addClass(sliderSelection, PRESSED);
                this.renderer.addClass(draghandle, PRESSED);
            }
            else {
                this.renderer.removeClass(sliderSelection, PRESSED);
                this.renderer.removeClass(draghandle, PRESSED);
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
