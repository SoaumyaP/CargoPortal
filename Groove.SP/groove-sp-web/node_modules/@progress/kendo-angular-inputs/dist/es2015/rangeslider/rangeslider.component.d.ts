/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { AfterViewInit, Renderer2, ElementRef, OnChanges, OnDestroy, NgZone, Injector, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { SliderBase } from '../sliders-common/slider-base';
import { RangeSliderValue } from './rangeslider-value.type';
/**
 * Represents the [Kendo UI RangeSlider component for Angular]({% slug overview_rangeslider %}).
 */
export declare class RangeSliderComponent extends SliderBase implements AfterViewInit, ControlValueAccessor, OnChanges, OnDestroy {
    protected localization: LocalizationService;
    protected injector: Injector;
    protected renderer: Renderer2;
    protected ngZone: NgZone;
    protected changeDetector: ChangeDetectorRef;
    protected hostElement: ElementRef;
    /**
     * Sets the range value of the RangeSlider.
     * The component can use either NgModel or the `value` binding but not both of them at the same time.
     */
    value: RangeSliderValue;
    draghandleStart: ElementRef;
    draghandleEnd: ElementRef;
    /**
     * @hidden
     */
    startHandleId: string;
    /**
     * @hidden
     */
    endHandleId: string;
    /**
     * @hidden
     */
    focusableId: string;
    private draggedHandle;
    private handleZIndex;
    private lastHandlePosition;
    private activeHandle;
    private focusChangedProgrammatically;
    constructor(localization: LocalizationService, injector: Injector, renderer: Renderer2, ngZone: NgZone, changeDetector: ChangeDetectorRef, hostElement: ElementRef);
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
    focus(): void;
    /**
     * Blurs the RangeSlider.
     */
    blur(): void;
    ngOnInit(): void;
    ngOnChanges(changes: SimpleChanges): void;
    ngAfterViewInit(): void;
    ngOnDestroy(): void;
    /**
     * @hidden
     */
    textFor(key: string): string;
    /**
     * @hidden
     */
    readonly valueText: string;
    /**
     * @hidden
     */
    onWrapClick: (args: any) => void;
    /**
     * @hidden
     */
    handleDragPress(args: any): void;
    /**
     * @hidden
     */
    onHandleDrag(args: any): void;
    /**
     * @hidden
     */
    onKeyDown: (e: any) => void;
    /**
     * @hidden
     */
    onHandleRelease(args: any): void;
    /**
     * @hidden
     */
    writeValue(value: [number, number]): void;
    /**
     * @hidden
     */
    registerOnChange(fn: () => any): void;
    /**
     * @hidden
     */
    registerOnTouched(fn: () => any): void;
    /**
     * @hidden
     */
    changeValue(value: [number, number]): void;
    /**
     * @hidden
     */
    sizeComponent(): void;
    /**
     * @hidden
     */
    readonly isDisabled: boolean;
    /**
     * @hidden
     * Used by the FloatingLabel to determine if the component is empty.
     */
    isEmpty(): boolean;
    protected focused: boolean;
    private dragging;
    private ngChange;
    private ngTouched;
    private getProps;
    private attachElementEventHandlers;
    private handleBlur;
}
