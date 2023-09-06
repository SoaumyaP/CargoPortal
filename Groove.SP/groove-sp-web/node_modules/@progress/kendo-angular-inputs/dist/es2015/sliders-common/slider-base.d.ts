/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { EventEmitter, Injector, Renderer2, NgZone, ChangeDetectorRef, ElementRef } from '@angular/core';
import { Subscription } from 'rxjs';
import { NgControl } from '@angular/forms';
import { LabelTemplateDirective } from './label-template.directive';
/**
 * @hidden
 */
export declare abstract class SliderBase {
    protected localizationService: LocalizationService;
    protected injector: Injector;
    protected renderer: Renderer2;
    protected ngZone: NgZone;
    protected changeDetector: ChangeDetectorRef;
    protected hostElement: ElementRef;
    /**
     * Defines the title of the ticks ([see example]({% slug ticks_slider %}#toc-titles)). The default title
     * for each tick is its Slider value. If you use a callback function, the function accepts an argument
     * that holds the value of the component and returns a string with the new title.
     */
    title: (value: number) => string;
    /**
     * Denotes the location of the tick marks in the Slider ([see example]({% slug ticks_slider %}#toc-placement)).
     *
     * The available options are:
     * * `before`&mdash;The tick marks are located to the top side of the horizontal track or to the left side of a vertical track.
     * * `after`&mdash;The tick marks are located to the bottom side of the horizontal track or to the right side of the vertical track.
     * * `both`&mdash; (Default) The tick marks are located on both sides of the track.
     * * `none`&mdash;The tick marks are not visible. The actual elements are not added to the DOM tree.
     */
    tickPlacement: string;
    /**
     * If `vertical` is set to `true`, the orientation of the Slider changes from horizontal to vertical
     * ([see example]({% slug orientation_slider %})).
     */
    vertical: boolean;
    /**
     * The minimum value of the Slider ([see example]({% slug predefinedsteps_slider %}#toc-small-steps)).
     * The attribute accepts both integers and floating-point numbers.
     */
    min: number;
    /**
     * The maximum value of the Slider ([see example]({% slug predefinedsteps_slider %}#toc-small-steps)).
     * The attribute accepts both integers and floating-point numbers.
     */
    max: number;
    /**
     * The step value of the Slider ([see example]({% slug predefinedsteps_slider %}#toc-small-steps)).
     * Accepts positive values only. Can be an integer or a floating-point number.
     */
    smallStep: number;
    /**
     * Specifies that every n<sup>th</sup> tick will be large and will have a label
     * ([see example]({% slug predefinedsteps_slider %}#toc-large-steps)).
     * Accepts positive integer values only.
     */
    largeStep: number;
    /**
     * Sets the width between each two ticks along the track ([see example]({% slug ticks_slider %}#toc-width)). The value
     * has to be set in pixels. If no `fixedTickWidth` is provided, the Slider automatically adjusts the tick width to
     * accommodate the elements within the size of the component.
     */
    fixedTickWidth: number;
    /**
     * Determines whether the Slider is disabled ([see example]({% slug disabledstate_slider %})).
     */
    disabled: boolean;
    /**
     * Determines whether the Slider is in its read-only state ([see example]({% slug readonly_slider %})).
     */
    readonly: boolean;
    /**
     * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the Slider.
     */
    tabindex: number;
    /**
     * Fires each time the user focuses the component.
     */
    onFocus: EventEmitter<any>;
    /**
     * Fires each time the component is blurred.
     */
    onBlur: EventEmitter<any>;
    /**
     * Fires each time the user selects a new value.
     */
    valueChange: EventEmitter<any>;
    direction: string;
    readonly horizontalClass: boolean;
    readonly verticalClass: boolean;
    sliderClass: boolean;
    widgetClass: boolean;
    readonly disabledClass: boolean;
    wrapper: ElementRef;
    track: ElementRef;
    sliderSelection: ElementRef;
    ticksContainer: ElementRef;
    ticks: any;
    labelTemplate: LabelTemplateDirective;
    protected subscriptions: Subscription;
    protected isFocused: boolean;
    protected isDragged: boolean;
    protected control: NgControl;
    constructor(localizationService: LocalizationService, injector: Injector, renderer: Renderer2, ngZone: NgZone, changeDetector: ChangeDetectorRef, hostElement: ElementRef);
    /**
     * @hidden
     * Called when the status of the component changes to or from `disabled`.
     * Depending on the value, it enables or disables the appropriate DOM element.
     *
     * @param isDisabled
     */
    setDisabledState(isDisabled: boolean): void;
    ngOnInit(): void;
    /**
     * @hidden
     */
    abstract sizeComponent(animate?: boolean): void;
    /**
     * @hidden
     */
    readonly isDisabled: boolean;
    /**
     * @hidden
     */
    ifEnabled: Function;
    /**
     * @hidden
     * Used by the FloatingLabel to determine if the component is empty.
     */
    isEmpty(): boolean;
    protected readonly reverse: boolean;
    protected readonly keyBinding: Object;
    protected resetStyles(elements: HTMLElement[]): void;
}
