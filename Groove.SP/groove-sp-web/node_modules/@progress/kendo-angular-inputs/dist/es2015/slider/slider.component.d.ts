/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { AfterViewInit, Renderer2, ElementRef, OnChanges, OnDestroy, NgZone, Injector, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { SliderBase } from '../sliders-common/slider-base';
/**
 * Represents the [Kendo UI Slider component for Angular]({% slug overview_slider %}).
 */
export declare class SliderComponent extends SliderBase implements AfterViewInit, ControlValueAccessor, OnChanges, OnDestroy {
    protected localization: LocalizationService;
    protected injector: Injector;
    protected renderer: Renderer2;
    protected ngZone: NgZone;
    protected changeDetector: ChangeDetectorRef;
    protected hostElement: ElementRef;
    /**
     * @hidden
     */
    focusableId: string;
    /**
     * Changes the `title` attribute of the drag handle so that it can be localized.
     */
    dragHandleTitle: string;
    /**
     * Sets the title of the **Increase** button of the Slider ([see example]({% slug sidebuttons_slider %}#toc-titles)).
     */
    incrementTitle: string;
    /**
     * Determines if the animation will be played on value change.
     * Regardless of this setting, no animation will be played during the initial rendering.
     */
    animate: boolean;
    /**
     * Sets the title of the **Decrease** button of the Slider ([see example]({% slug sidebuttons_slider %}#toc-titles)).
     */
    decrementTitle: string;
    /**
     * Renders the arrow side buttons of the Slider ([see example]({% slug sidebuttons_slider %}#toc-hidden-state)).
     * When `showButtons` is set to `false`, the buttons are not displayed.
     */
    showButtons: boolean;
    /**
     * The current value of the Slider when it is initially displayed.
     * The component can use either NgModel or the `value` binding but not both of them at the same time.
     */
    value: number;
    /**
     * @hidden
     */
    tabIndex: number;
    draghandle: ElementRef;
    decreaseButton: ElementRef;
    increaseButton: ElementRef;
    constructor(localization: LocalizationService, injector: Injector, renderer: Renderer2, ngZone: NgZone, changeDetector: ChangeDetectorRef, hostElement: ElementRef);
    /**
     * @hidden
     */
    handleFocus: () => void;
    /**
     * @hidden
     */
    handleBlur: () => void;
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
    focus(): void;
    /**
     * Blurs the Slider.
     */
    blur(): void;
    ngOnChanges(changes: SimpleChanges): void;
    ngAfterViewInit(): void;
    ngOnDestroy(): void;
    /**
     * @hidden
     */
    readonly incrementMessage: string;
    /**
     * @hidden
     */
    readonly decrementMessage: string;
    /**
     * @hidden
     */
    readonly dragHandleMessage: string;
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
    onHandleRelease(): void;
    /**
     * @hidden
     */
    writeValue(value: number): void;
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
    changeValue(value: number): void;
    /**
     * @hidden
     */
    sizeComponent(animate: boolean): void;
    protected focused: boolean;
    private dragging;
    private setValueChangeInterval;
    private ngChange;
    private ngTouched;
    private decreaseValue;
    private increaseValue;
    private getProps;
}
