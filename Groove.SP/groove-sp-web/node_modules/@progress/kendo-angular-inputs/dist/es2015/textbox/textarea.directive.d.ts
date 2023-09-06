/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ElementRef, EventEmitter, OnDestroy, Renderer2, OnChanges, OnInit, NgZone, ChangeDetectorRef, Injector } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
/**
 * Represents the [Kendo UI TextArea directive for the Inputs components for Angular]({% slug overview_textarea %}).
 * Provides floating labels to `textarea` elements.
 *
 * @example
 * ```ts-no-run
 * <textarea kendoTextArea></textarea>
 * ```
 */
export declare class TextAreaDirective implements OnInit, OnDestroy, OnChanges, ControlValueAccessor {
    private renderer;
    private element;
    private zone;
    private changeDetector;
    private injector;
    elementClass: boolean;
    autofillClass: boolean;
    direction: string;
    /**
     * Fires each time the textarea value is changed.
     */
    valueChange: EventEmitter<any>;
    /**
     * Specifies if the `textarea` element will resize its height automatically
     * ([see example]({% slug overview_textarea %}#toc-auto-resizing)).
     *
     * @default false
     */
    autoSize: boolean;
    /**
     * Specifies the textarea value.
     */
    value: string;
    /**
     * @hidden
     */
    onFocus: EventEmitter<any>;
    /**
     * @hidden
     */
    onBlur: EventEmitter<any>;
    /**
     * @hidden
     */
    onValueChange: EventEmitter<any>;
    /**
     * @hidden
     */
    autoFillStart: EventEmitter<any>;
    /**
     * @hidden
     */
    autoFillEnd: EventEmitter<any>;
    id: string;
    private listeners;
    private inputSubscription;
    private initialHeight;
    private control;
    private resizeSubscription;
    constructor(renderer: Renderer2, element: ElementRef, zone: NgZone, changeDetector: ChangeDetectorRef, injector: Injector, rtl: boolean);
    /**
     * @hidden
     */
    writeValue(value: any): void;
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
    setDisabledState(isDisabled: boolean): void;
    ngOnInit(): void;
    ngOnChanges(changes: any): void;
    ngOnDestroy(): void;
    private ngChange;
    private ngTouched;
    private elementValue;
    private setElementProperty;
    private resize;
    private handleInput;
    private handleFocus;
    private handleBlur;
}
