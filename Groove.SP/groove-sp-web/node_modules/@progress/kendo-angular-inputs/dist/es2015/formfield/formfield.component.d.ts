/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { AfterViewInit, ElementRef, Renderer2, QueryList } from '@angular/core';
import { NgControl } from '@angular/forms';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { ShowOptions } from './models/show-options';
import { ErrorComponent } from './error.component';
import { HintComponent } from './hint.component';
import { Orientation } from './models/orientation';
/**
 * Specifies a container for form-bound controls (Kendo controls or native HTML controls).
 * Applies styling and behavior rules.
 */
export declare class FormFieldComponent implements AfterViewInit {
    private renderer;
    private localizationService;
    hostClass: boolean;
    /**
     * @hidden
     */
    direction: string;
    readonly errorClass: boolean;
    readonly disabledClass: boolean;
    formControls: QueryList<NgControl>;
    controlElementRefs: QueryList<ElementRef>;
    kendoInput: any;
    errorChildren: QueryList<ErrorComponent>;
    hintChildren: QueryList<HintComponent>;
    /**
     *
     * Specifies when the Hint messages will be shown.
     *
     * The possible values are:
     *
     * * (Default) `initial`&mdash;Allows displaying hints when the form-bound component state is
     * `valid` or `untouched` and `pristine`.
     * * `always`&mdash;Allows full control over the visibility of the hints.
     *
     */
    showHints: ShowOptions;
    /**
     * Specifies the layout orientation of the form field.
     *
     * * The possible values are:
     *
     * * (Default) `vertical`
     * * `horizontal`
     */
    orientation: Orientation;
    /**
     * Specifies when the Error messages will be shown.
     *
     * The possible values are:
     *
     * * (Default) `initial`&mdash;Allows displaying errors when the form-bound component state is
     * `invalid` and `touched` or `dirty`.
     * * `always`&mdash;Allows full control over the visibility of the errors.
     *
     */
    showErrors: ShowOptions;
    /**
     * @hidden
     */
    readonly horizontal: boolean;
    /**
     * @hidden
     */
    readonly hasHints: boolean;
    /**
     * @hidden
     */
    readonly hasErrors: boolean;
    private control;
    private subscriptions;
    private rtl;
    constructor(renderer: Renderer2, localizationService: LocalizationService);
    ngAfterViewInit(): void;
    ngOnDestroy(): void;
    private disabledKendoInput;
    private disabledControl;
    private disabledElement;
    private validateFormControl;
    private isControlGroup;
    private isRadioControl;
    private updateDescription;
    private findControlElements;
    private generateDescriptionIds;
    private showHintsInitial;
    private showErrorsInitial;
    private setDescription;
}
