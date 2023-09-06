/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var forms_1 = require("@angular/forms");
var rxjs_1 = require("rxjs");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
var kendo_angular_l10n_1 = require("@progress/kendo-angular-l10n");
var error_component_1 = require("./error.component");
var hint_component_1 = require("./hint.component");
/**
 * Specifies a container for form-bound controls (Kendo controls or native HTML controls).
 * Applies styling and behavior rules.
 */
var FormFieldComponent = /** @class */ (function () {
    function FormFieldComponent(renderer, localizationService) {
        var _this = this;
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
        this.subscriptions = new rxjs_1.Subscription();
        this.rtl = false;
        this.subscriptions.add(this.localizationService.changes.subscribe(function (_a) {
            var rtl = _a.rtl;
            _this.rtl = rtl;
            _this.direction = _this.rtl ? 'rtl' : 'ltr';
        }));
    }
    Object.defineProperty(FormFieldComponent.prototype, "errorClass", {
        get: function () {
            if (!this.control) {
                return false;
            }
            return this.control.invalid && (this.control.touched || this.control.dirty);
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormFieldComponent.prototype, "disabledClass", {
        get: function () {
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
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormFieldComponent.prototype, "formControls", {
        set: function (formControls) {
            this.validateFormControl(formControls);
            this.control = formControls.first;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormFieldComponent.prototype, "horizontal", {
        /**
         * @hidden
         */
        get: function () {
            return this.orientation === 'horizontal';
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormFieldComponent.prototype, "hasHints", {
        /**
         * @hidden
         */
        get: function () {
            return this.showHints === 'always' ? true : this.showHintsInitial();
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormFieldComponent.prototype, "hasErrors", {
        /**
         * @hidden
         */
        get: function () {
            return this.showErrors === 'always' ? true : this.showErrorsInitial();
        },
        enumerable: true,
        configurable: true
    });
    FormFieldComponent.prototype.ngAfterViewInit = function () {
        this.setDescription();
    };
    FormFieldComponent.prototype.ngOnDestroy = function () {
        this.subscriptions.unsubscribe();
    };
    FormFieldComponent.prototype.disabledKendoInput = function () {
        return this.kendoInput && this.kendoInput.disabled;
    };
    FormFieldComponent.prototype.disabledControl = function () {
        return this.control.disabled;
    };
    FormFieldComponent.prototype.disabledElement = function () {
        var elements = this.controlElementRefs.toArray();
        return elements.every(function (e) { return e.nativeElement.hasAttribute('disabled'); });
    };
    FormFieldComponent.prototype.validateFormControl = function (formControls) {
        if (core_1.isDevMode() && formControls.length !== 1 && !this.isControlGroup(formControls)) {
            throw new Error('The `kendo-formfield` component should contain ' +
                'only one control of type NgControl with a formControlName(https://angular.io/api/forms/FormControlName)' +
                'or an ngModel(https://angular.io/api/forms/NgModel) binding.');
        }
    };
    FormFieldComponent.prototype.isControlGroup = function (formControls) {
        var _this = this;
        if (!formControls.length) {
            return false;
        }
        var name = formControls.first.name;
        return formControls.toArray().every(function (c) { return c.name === name && (_this.isRadioControl(c)); });
    };
    FormFieldComponent.prototype.isRadioControl = function (control) {
        return control.valueAccessor instanceof forms_1.RadioControlValueAccessor;
    };
    FormFieldComponent.prototype.updateDescription = function () {
        var _this = this;
        var controls = this.findControlElements();
        if (!controls) {
            return;
        }
        controls.forEach(function (control) {
            var ariaIds = _this.generateDescriptionIds(control);
            _this.renderer.setAttribute(control, 'aria-describedby', ariaIds);
        });
    };
    FormFieldComponent.prototype.findControlElements = function () {
        if (!this.controlElementRefs) {
            return;
        }
        // if the control is KendoInput and has focusableId - dropdowns, dateinputs
        if (this.kendoInput && this.kendoInput.focusableId && kendo_angular_common_1.isDocumentAvailable()) {
            return [document.getElementById(this.kendoInput.focusableId)];
        }
        return this.controlElementRefs.map(function (el) { return el.nativeElement; });
    };
    FormFieldComponent.prototype.generateDescriptionIds = function (control) {
        var ids = new Set();
        if (control.hasAttribute('aria-describedby')) {
            var attributes = control.getAttribute('aria-describedby').split(' ');
            attributes.forEach(function (attr) {
                if (attr.includes('kendo-hint-') || attr.includes('kendo-error-')) {
                    return;
                }
                ids.add(attr);
            });
        }
        this.hintChildren.forEach(function (hint) {
            ids.add(hint.id);
        });
        this.errorChildren.forEach(function (error) {
            ids.add(error.id);
        });
        return Array.from(ids).join(' ');
    };
    FormFieldComponent.prototype.showHintsInitial = function () {
        if (!this.control) {
            return true;
        }
        var _a = this.control, valid = _a.valid, untouched = _a.untouched, pristine = _a.pristine;
        return valid || (untouched && pristine);
    };
    FormFieldComponent.prototype.showErrorsInitial = function () {
        if (!this.control) {
            return false;
        }
        var _a = this.control, invalid = _a.invalid, dirty = _a.dirty, touched = _a.touched;
        return invalid && (dirty || touched);
    };
    FormFieldComponent.prototype.setDescription = function () {
        var _this = this;
        this.updateDescription();
        this.subscriptions.add(this.errorChildren.changes.subscribe(function () { return _this.updateDescription(); }));
        this.subscriptions.add(this.hintChildren.changes.subscribe(function () { return _this.updateDescription(); }));
    };
    FormFieldComponent.decorators = [
        { type: core_1.Component, args: [{
                    selector: 'kendo-formfield',
                    template: "\n        <ng-content select=\"label, kendo-label\"></ng-content>\n        <div [class.k-form-field-wrap]=\"horizontal\">\n            <ng-content></ng-content>\n            <ng-content select=\"kendo-formhint\" *ngIf=\"hasHints\"></ng-content>\n            <ng-content select=\"kendo-formerror\" *ngIf=\"hasErrors\"></ng-content>\n        </div>\n    ",
                    providers: [
                        kendo_angular_l10n_1.LocalizationService,
                        {
                            provide: kendo_angular_l10n_1.L10N_PREFIX,
                            useValue: 'kendo.formfield'
                        }
                    ]
                },] },
    ];
    /** @nocollapse */
    FormFieldComponent.ctorParameters = function () { return [
        { type: core_1.Renderer2 },
        { type: kendo_angular_l10n_1.LocalizationService }
    ]; };
    FormFieldComponent.propDecorators = {
        hostClass: [{ type: core_1.HostBinding, args: ['class.k-form-field',] }],
        direction: [{ type: core_1.HostBinding, args: ['attr.dir',] }],
        errorClass: [{ type: core_1.HostBinding, args: ['class.k-form-field-error',] }],
        disabledClass: [{ type: core_1.HostBinding, args: ['class.k-form-field-disabled',] }],
        formControls: [{ type: core_1.ContentChildren, args: [forms_1.NgControl, { descendants: true, static: true },] }],
        controlElementRefs: [{ type: core_1.ContentChildren, args: [forms_1.NgControl, { read: core_1.ElementRef, descendants: true, static: true },] }],
        kendoInput: [{ type: core_1.ContentChild, args: [kendo_angular_common_1.KendoInput, { static: true },] }],
        errorChildren: [{ type: core_1.ContentChildren, args: [error_component_1.ErrorComponent, { descendants: true, static: true },] }],
        hintChildren: [{ type: core_1.ContentChildren, args: [hint_component_1.HintComponent, { descendants: true, static: true },] }],
        showHints: [{ type: core_1.Input }],
        orientation: [{ type: core_1.Input }],
        showErrors: [{ type: core_1.Input }]
    };
    return FormFieldComponent;
}());
exports.FormFieldComponent = FormFieldComponent;
