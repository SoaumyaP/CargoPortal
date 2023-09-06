/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, ContentChild, ContentChildren, ElementRef, HostBinding, Input, isDevMode, Renderer2, QueryList } from '@angular/core';
import { NgControl, RadioControlValueAccessor } from '@angular/forms';
import { Subscription } from 'rxjs';
import { KendoInput, isDocumentAvailable } from '@progress/kendo-angular-common';
import { LocalizationService, L10N_PREFIX } from '@progress/kendo-angular-l10n';
import { ErrorComponent } from './error.component';
import { HintComponent } from './hint.component';
/**
 * Specifies a container for form-bound controls (Kendo controls or native HTML controls).
 * Applies styling and behavior rules.
 */
export class FormFieldComponent {
    constructor(renderer, localizationService) {
        this.renderer = renderer;
        this.localizationService = localizationService;
        this.hostClass = true;
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
        this.showHints = 'initial';
        /**
         * Specifies the layout orientation of the form field.
         *
         * * The possible values are:
         *
         * * (Default) `vertical`
         * * `horizontal`
         */
        this.orientation = 'vertical';
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
        this.showErrors = 'initial';
        this.subscriptions = new Subscription();
        this.rtl = false;
        this.subscriptions.add(this.localizationService.changes.subscribe(({ rtl }) => {
            this.rtl = rtl;
            this.direction = this.rtl ? 'rtl' : 'ltr';
        }));
    }
    get errorClass() {
        if (!this.control) {
            return false;
        }
        return this.control.invalid && (this.control.touched || this.control.dirty);
    }
    get disabledClass() {
        if (!this.control) {
            return false;
        }
        // radiobutton group
        if (this.isRadioControl(this.control)) {
            return false;
        }
        return this.disabledControl() ||
            this.disabledElement() ||
            this.disabledKendoInput();
    }
    set formControls(formControls) {
        this.validateFormControl(formControls);
        this.control = formControls.first;
    }
    /**
     * @hidden
     */
    get horizontal() {
        return this.orientation === 'horizontal';
    }
    /**
     * @hidden
     */
    get hasHints() {
        return this.showHints === 'always' ? true : this.showHintsInitial();
    }
    /**
     * @hidden
     */
    get hasErrors() {
        return this.showErrors === 'always' ? true : this.showErrorsInitial();
    }
    ngAfterViewInit() {
        this.setDescription();
    }
    ngOnDestroy() {
        this.subscriptions.unsubscribe();
    }
    disabledKendoInput() {
        return this.kendoInput && this.kendoInput.disabled;
    }
    disabledControl() {
        return this.control.disabled;
    }
    disabledElement() {
        const elements = this.controlElementRefs.toArray();
        return elements.every(e => e.nativeElement.hasAttribute('disabled'));
    }
    validateFormControl(formControls) {
        if (isDevMode() && formControls.length !== 1 && !this.isControlGroup(formControls)) {
            throw new Error('The `kendo-formfield` component should contain ' +
                'only one control of type NgControl with a formControlName(https://angular.io/api/forms/FormControlName)' +
                'or an ngModel(https://angular.io/api/forms/NgModel) binding.');
        }
    }
    isControlGroup(formControls) {
        if (!formControls.length) {
            return false;
        }
        const name = formControls.first.name;
        return formControls.toArray().every(c => c.name === name && (this.isRadioControl(c)));
    }
    isRadioControl(control) {
        return control.valueAccessor instanceof RadioControlValueAccessor;
    }
    updateDescription() {
        const controls = this.findControlElements();
        if (!controls) {
            return;
        }
        controls.forEach((control) => {
            const ariaIds = this.generateDescriptionIds(control);
            this.renderer.setAttribute(control, 'aria-describedby', ariaIds);
        });
    }
    findControlElements() {
        if (!this.controlElementRefs) {
            return;
        }
        // if the control is KendoInput and has focusableId - dropdowns, dateinputs
        if (this.kendoInput && this.kendoInput.focusableId && isDocumentAvailable()) {
            return [document.getElementById(this.kendoInput.focusableId)];
        }
        return this.controlElementRefs.map(el => el.nativeElement);
    }
    generateDescriptionIds(control) {
        const ids = new Set();
        if (control.hasAttribute('aria-describedby')) {
            const attributes = control.getAttribute('aria-describedby').split(' ');
            attributes.forEach((attr) => {
                if (attr.includes('kendo-hint-') || attr.includes('kendo-error-')) {
                    return;
                }
                ids.add(attr);
            });
        }
        this.hintChildren.forEach((hint) => {
            ids.add(hint.id);
        });
        this.errorChildren.forEach((error) => {
            ids.add(error.id);
        });
        return Array.from(ids).join(' ');
    }
    showHintsInitial() {
        if (!this.control) {
            return true;
        }
        const { valid, untouched, pristine } = this.control;
        return valid || (untouched && pristine);
    }
    showErrorsInitial() {
        if (!this.control) {
            return false;
        }
        const { invalid, dirty, touched } = this.control;
        return invalid && (dirty || touched);
    }
    setDescription() {
        this.updateDescription();
        this.subscriptions.add(this.errorChildren.changes.subscribe(() => this.updateDescription()));
        this.subscriptions.add(this.hintChildren.changes.subscribe(() => this.updateDescription()));
    }
}
FormFieldComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-formfield',
                template: `
        <ng-content select="label, kendo-label"></ng-content>
        <div [class.k-form-field-wrap]="horizontal">
            <ng-content></ng-content>
            <ng-content select="kendo-formhint" *ngIf="hasHints"></ng-content>
            <ng-content select="kendo-formerror" *ngIf="hasErrors"></ng-content>
        </div>
    `,
                providers: [
                    LocalizationService,
                    {
                        provide: L10N_PREFIX,
                        useValue: 'kendo.formfield'
                    }
                ]
            },] },
];
/** @nocollapse */
FormFieldComponent.ctorParameters = () => [
    { type: Renderer2 },
    { type: LocalizationService }
];
FormFieldComponent.propDecorators = {
    hostClass: [{ type: HostBinding, args: ['class.k-form-field',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }],
    errorClass: [{ type: HostBinding, args: ['class.k-form-field-error',] }],
    disabledClass: [{ type: HostBinding, args: ['class.k-form-field-disabled',] }],
    formControls: [{ type: ContentChildren, args: [NgControl, { descendants: true, static: true },] }],
    controlElementRefs: [{ type: ContentChildren, args: [NgControl, { read: ElementRef, descendants: true, static: true },] }],
    kendoInput: [{ type: ContentChild, args: [KendoInput, { static: true },] }],
    errorChildren: [{ type: ContentChildren, args: [ErrorComponent, { descendants: true, static: true },] }],
    hintChildren: [{ type: ContentChildren, args: [HintComponent, { descendants: true, static: true },] }],
    showHints: [{ type: Input }],
    orientation: [{ type: Input }],
    showErrors: [{ type: Input }]
};
