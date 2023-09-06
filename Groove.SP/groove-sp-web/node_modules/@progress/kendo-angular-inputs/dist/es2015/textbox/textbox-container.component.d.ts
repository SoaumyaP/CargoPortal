/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { AfterContentInit, ElementRef, OnDestroy, Renderer2, ChangeDetectorRef } from '@angular/core';
import { TextAreaDirective } from './textarea.directive';
import { NgControl } from '@angular/forms';
/**
 * @hidden
 */
export declare class TextBoxContainerComponent implements AfterContentInit, OnDestroy {
    private elementRef;
    private renderer;
    private changeDetectorRef;
    readonly hostClasses: boolean;
    readonly textareaElementClass: boolean;
    readonly focusedClass: boolean;
    readonly invalidClass: boolean;
    /**
     * @hidden
     */
    direction: string;
    /**
     * Sets the `id` attribute of the input inside the container.
     */
    id: string;
    /**
     * Adds a floating label which describes the input.
     */
    floatingLabel: string;
    kendoInput: any;
    textarea: TextAreaDirective;
    formControl: NgControl;
    /**
     * @hidden
     */
    focused: boolean;
    /**
     * @hidden
     */
    empty: boolean;
    /**
     * @hidden
     */
    invalid: boolean;
    private _subscriptions;
    private autoFillStarted;
    constructor(elementRef: ElementRef, renderer: Renderer2, changeDetectorRef: ChangeDetectorRef, rtl: boolean);
    /**
     * @hidden
     */
    ngAfterContentInit(): void;
    /**
     * @hidden
     */
    ngOnDestroy(): void;
    private subscribe;
    private updateState;
}
