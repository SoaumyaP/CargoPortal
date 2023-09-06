/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var dom_utils_1 = require("./../common/dom-utils");
var forms_1 = require("@angular/forms");
var core_1 = require("@angular/core");
var dom_utils_2 = require("../common/dom-utils");
var utils_1 = require("../common/utils");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
var textbox_suffix_directive_1 = require("./textbox-suffix.directive");
var textbox_prefix_directive_1 = require("./textbox-prefix.directive");
var kendo_angular_l10n_1 = require("@progress/kendo-angular-l10n");
var TextBoxComponent = /** @class */ (function () {
    function TextBoxComponent(localizationService, ngZone, changeDetector, renderer, injector, hostElement) {
        var _this = this;
        this.localizationService = localizationService;
        this.ngZone = ngZone;
        this.changeDetector = changeDetector;
        this.renderer = renderer;
        this.injector = injector;
        this.hostElement = hostElement;
        /**
         * Sets the `title` attribute of the `input` element of the TextBox.
         */
        this.title = '';
        /**
         * Sets the disabled state of the component.
         *
         * @default false
         */
        this.disabled = false;
        /**
         * Sets the read-only state of the component.
         *
         * @default false
         */
        this.readonly = false;
        /**
         * Specifies the `tabindex` of the TextBox.
         *
         * @default 0
         */
        this.tabindex = 0;
        /**
         * Provides a value for the TextBox.
         */
        this.value = null;
        /**
         * Determines whether the whole value will be selected when the TextBox is clicked. Defaults to `false`.
         *
         * @default false
         */
        this.selectOnFocus = false;
        /**
         * Specifies when the Success icon will be shown ([see example]({% slug validation_textbox %})).
         *
         * The possible values are:
         *
         * `boolean`&mdash;The Success icon is displayed, if the condition given by the developer is met.
         *
         * `initial`&mdash;The Success icon will be displayed when the component state is neither `invalid` nor `touched` or `dirty`.
         *
         * @default false
         */
        this.showSuccessIcon = false;
        /**
         * Specifies when the Error icon will be shown ([see example]({% slug validation_textbox %})).
         *
         * The possible values are:
         *
         * * `initial`&mdash;The Error icon will be displayed when the component state is
         * `invalid` and `touched` or `dirty`.
         * * `boolean`&mdash;The Error icon is displayed, if the condition given by the developer is met.
         *
         * @default false
         */
        this.showErrorIcon = false;
        /**
         * Specifies whether a Clear button will be rendered.
         *
         * @default false
         */
        this.clearButton = false;
        /**
         * Fires each time the value is changed&mdash;
         * when the component is blurred or the value is cleared through the **Clear** button
         * ([see example]({% slug overview_textbox %}#toc-events)).
         * When the value of the component is programmatically changed to `ngModel` or `formControl`
         * through its API or form binding, the `valueChange` event is not triggered because it
         * might cause a mix-up with the built-in `valueChange` mechanisms of the `ngModel` or `formControl` bindings.
         */
        this.valueChange = new core_1.EventEmitter();
        /**
         * Fires each time the user focuses the `input` element.
         */
        this.inputFocus = new core_1.EventEmitter();
        /**
         * Fires each time the `input` element gets blurred.
         */
        this.inputBlur = new core_1.EventEmitter();
        /**
         * Fires each time the user focuses the TextBox component.
         *
         * > To wire the event programmatically, use the `onFocus` property.
         *
         * @example
         * ```ts-no-run
         * _@Component({
         * selector: 'my-app',
         * template: `
         *  <kendo-textbox (focus)="handleFocus()"></kendo-textbox>
         * `
         * })
         * class AppComponent {
         *   public handleFocus(): void {
         *      console.log('Component is isFocused');
         *   }
         * }
         * ```
         */
        this.onFocus = new core_1.EventEmitter();
        /**
         * Fires each time the TextBox component gets blurred.
         *
         * > To wire the event programmatically, use the `onBlur` property.
         *
         * @example
         * ```ts-no-run
         * _@Component({
         * selector: 'my-app',
         * template: `
         *  <kendo-textbox (blur)="handleBlur()"></kendo-textbox>
         * `
         * })
         * class AppComponent {
         *   public handleBlur(): void {
         *      console.log('Component is blurred');
         *   }
         * }
         * ```
         */
        this.onBlur = new core_1.EventEmitter();
        this.hostClass = true;
        this._isFocused = false;
        this.focusChangedProgrammatically = false;
        /**
         * @hidden
         */
        this.handleInputFocus = function () {
            if (!_this.disabled) {
                if (_this.selectOnFocus && _this.value) {
                    _this.ngZone.run(function () {
                        setTimeout(function () { _this.selectAll(); });
                    });
                }
                if (kendo_angular_common_1.hasObservers(_this.onFocus)) {
                    if (!_this.isFocused) {
                        _this.ngZone.run(function () {
                            _this.onFocus.emit();
                        });
                    }
                }
                if (kendo_angular_common_1.hasObservers(_this.inputFocus)) {
                    if (!_this.focusChangedProgrammatically || (_this.focusChangedProgrammatically && _this.clearButtonClicked)) {
                        _this.ngZone.run(function () {
                            _this.inputFocus.emit();
                        });
                    }
                }
                _this.ngZone.run(function () {
                    _this.isFocused = true;
                });
            }
        };
        /**
         * @hidden
         */
        this.handleInputBlur = function () {
            _this.changeDetector.markForCheck();
            if (kendo_angular_common_1.hasObservers(_this.inputBlur) || utils_1.requiresZoneOnBlur(_this.control)) {
                _this.ngZone.run(function () {
                    _this.ngTouched();
                    _this.inputBlur.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.handleInput = function (ev) {
            var incomingValue = ev.target.value;
            _this.updateValue(incomingValue);
        };
        this.ngChange = function (_) { };
        this.ngTouched = function () { };
        this.direction = localizationService.rtl ? 'rtl' : 'ltr';
    }
    Object.defineProperty(TextBoxComponent.prototype, "tabIndex", {
        get: function () {
            return this.tabindex;
        },
        /**
         * @hidden
         */
        set: function (tabIndex) {
            this.tabindex = tabIndex;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxComponent.prototype, "disabledClass", {
        get: function () {
            return this.disabled;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxComponent.prototype, "isFocused", {
        get: function () {
            return this.disabled ? false : this._isFocused;
        },
        set: function (isFocused) {
            this._isFocused = isFocused;
        },
        enumerable: true,
        configurable: true
    });
    TextBoxComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.control = this.injector.get(forms_1.NgControl, null);
        this.checkClearButton();
        this.subscriptions = this.localizationService.changes.subscribe(function (_a) {
            var rtl = _a.rtl;
            _this.direction = rtl ? 'rtl' : 'ltr';
        });
    };
    TextBoxComponent.prototype.ngAfterViewInit = function () {
        var _this = this;
        var hostElement = this.hostElement.nativeElement;
        var cursorInsideWrapper = false;
        var tabbing = false;
        this.ngZone.runOutsideAngular(function () {
            // focusIn and focusOut are relative to the host element
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'focusin', function () {
                if (!_this.isFocused) {
                    _this.ngZone.run(function () {
                        _this.onFocus.emit();
                        _this.isFocused = true;
                    });
                }
            }));
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'focusout', function (args) {
                if (!_this.isFocused) {
                    return;
                }
                if (tabbing) {
                    var closestTextbox = dom_utils_1.closest(args.relatedTarget, function (element) { return element === _this.hostElement.nativeElement; });
                    if (!closestTextbox) {
                        _this.handleBlur();
                    }
                    tabbing = false;
                }
                else {
                    if (!cursorInsideWrapper && !_this.clearButtonClicked) {
                        _this.handleBlur();
                    }
                }
            }));
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'mouseenter', function () {
                cursorInsideWrapper = true;
            }));
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'mouseleave', function () {
                cursorInsideWrapper = false;
            }));
            _this.subscriptions.add(_this.renderer.listen(hostElement, 'keydown', function (args) {
                if (args.keyCode === kendo_angular_common_1.Keys.Tab) {
                    tabbing = true;
                }
                else {
                    tabbing = false;
                }
            }));
        });
    };
    TextBoxComponent.prototype.ngOnChanges = function (changes) {
        if (changes.disabled || changes.readonly) {
            this.checkClearButton();
        }
    };
    TextBoxComponent.prototype.ngOnDestroy = function () {
        this.subscriptions.unsubscribe();
    };
    /**
     * Focuses the TextBox.
     *
     * @example
     * ```ts-no-run
     * _@Component({
     * selector: 'my-app',
     * template: `
     *  <button (click)="input.focus()">Focus the input</button>
     *  <kendo-textbox #input></kendo-textbox>
     * `
     * })
     * class AppComponent { }
     * ```
     */
    TextBoxComponent.prototype.focus = function () {
        if (!this.input) {
            return;
        }
        this.focusChangedProgrammatically = true;
        this.isFocused = true;
        this.input.nativeElement.focus();
        this.focusChangedProgrammatically = false;
    };
    /**
     * Blurs the TextBox.
     */
    TextBoxComponent.prototype.blur = function () {
        this.focusChangedProgrammatically = true;
        var isFocusedElement = this.hostElement.nativeElement.querySelector(':focus');
        if (isFocusedElement) {
            isFocusedElement.blur();
        }
        this.isFocused = false;
        this.focusChangedProgrammatically = false;
    };
    /**
     * @hidden
     */
    TextBoxComponent.prototype.clearTitle = function () {
        return this.localizationService.get('clear');
    };
    /**
     * @hidden
     */
    TextBoxComponent.prototype.checkClearButton = function () {
        this.showClearButton =
            !this.disabled &&
                !this.readonly &&
                this.clearButton &&
                !!this.value;
    };
    /**
     * @hidden
     */
    TextBoxComponent.prototype.clearValue = function (ev) {
        if (ev) {
            ev.preventDefault();
        }
        this.clearButtonClicked = true;
        this.input.nativeElement.value = '';
        this.input.nativeElement.focus();
        this.updateValue(null);
        this.checkClearButton();
        this.clearButtonClicked = false;
    };
    /**
     * @hidden
     */
    TextBoxComponent.prototype.writeValue = function (value) {
        this.value = value;
        this.checkClearButton();
    };
    /**
     * @hidden
     */
    TextBoxComponent.prototype.registerOnChange = function (fn) {
        this.ngChange = fn;
    };
    /**
     * @hidden
     */
    TextBoxComponent.prototype.registerOnTouched = function (fn) {
        this.ngTouched = fn;
    };
    /**
     * @hidden
     * Called when the status of the component changes to or from `disabled`.
     * Depending on the value, it enables or disables the appropriate DOM element.
     *
     * @param isDisabled
     */
    TextBoxComponent.prototype.setDisabledState = function (isDisabled) {
        this.disabled = isDisabled;
    };
    /**
     * @hidden
     */
    TextBoxComponent.prototype.showErrorsInitial = function () {
        if (!this.control) {
            return false;
        }
        var _a = this.control, invalid = _a.invalid, dirty = _a.dirty, touched = _a.touched;
        return invalid && (dirty || touched);
    };
    /**
     * @hidden
     */
    TextBoxComponent.prototype.showSuccessInitial = function () {
        if (!this.control) {
            return false;
        }
        var _a = this.control, valid = _a.valid, dirty = _a.dirty, touched = _a.touched;
        return valid && (dirty || touched);
    };
    Object.defineProperty(TextBoxComponent.prototype, "successIconClasses", {
        /**
         * @hidden
         */
        get: function () {
            return this.successIcon
                ? "k-text-success " + this.successIcon
                : "k-text-success k-icon k-i-check-outline";
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxComponent.prototype, "errorIconClasses", {
        /**
         * @hidden
         */
        get: function () {
            return this.errorIcon
                ? "k-text-error " + this.errorIcon
                : "k-text-error k-icon k-i-warning";
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxComponent.prototype, "clearButtonClasses", {
        /**
         * @hidden
         */
        get: function () {
            return this.clearButtonIcon
                ? this.clearButtonIcon
                : "k-icon k-i-close-circle";
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxComponent.prototype, "hasErrors", {
        /**
         * @hidden
         */
        get: function () {
            return this.showErrorIcon === 'initial'
                ? this.showErrorsInitial()
                : this.showErrorIcon;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TextBoxComponent.prototype, "isSuccessful", {
        /**
         * @hidden
         */
        get: function () {
            return this.showSuccessIcon === 'initial'
                ? this.showSuccessInitial()
                : this.showSuccessIcon;
        },
        enumerable: true,
        configurable: true
    });
    TextBoxComponent.prototype.setSelection = function (start, end) {
        if (this.isFocused) {
            dom_utils_2.invokeElementMethod(this.input, 'setSelectionRange', start, end);
        }
    };
    TextBoxComponent.prototype.selectAll = function () {
        if (this.value) {
            this.setSelection(0, this.value.length);
        }
    };
    TextBoxComponent.prototype.updateValue = function (value) {
        var _this = this;
        if (!utils_1.areSame(this.value, value)) {
            this.ngZone.run(function () {
                _this.value = value;
                _this.ngChange(value);
                _this.valueChange.emit(value);
                _this.checkClearButton();
                _this.changeDetector.markForCheck();
            });
        }
    };
    TextBoxComponent.prototype.handleBlur = function () {
        var _this = this;
        this.ngZone.run(function () {
            if (!_this.focusChangedProgrammatically) {
                _this.onBlur.emit();
            }
            _this.isFocused = false;
        });
    };
    TextBoxComponent.decorators = [
        { type: core_1.Component, args: [{
                    exportAs: 'kendoTextBox',
                    providers: [
                        kendo_angular_l10n_1.LocalizationService,
                        { provide: kendo_angular_l10n_1.L10N_PREFIX, useValue: 'kendo.textbox' },
                        {
                            provide: forms_1.NG_VALUE_ACCESSOR,
                            useExisting: core_1.forwardRef(function () { return TextBoxComponent; }),
                            multi: true
                        },
                        { provide: kendo_angular_common_1.KendoInput, useExisting: core_1.forwardRef(function () { return TextBoxComponent; }) }
                    ],
                    selector: 'kendo-textbox',
                    template: "\n        <ng-container kendoTextBoxLocalizedMessages\n            i18n-clear=\"kendo.textbox.clear|The title for the **Clear** button in the TextBox.\"\n            clear=\"Clear\">\n        </ng-container>\n        <span class=\"k-input-prefix\">\n            <ng-template\n                *ngIf=\"prefixTemplate\"\n                [ngTemplateOutlet]=\"prefixTemplate?.templateRef\">\n            </ng-template>\n        </span>\n        <input\n            class=\"k-input\"\n            #input\n            [disabled]=\"disabled\"\n            [readonly]=\"readonly\"\n            [attr.tabindex]=\"disabled ? undefined : tabindex\"\n            [value]=\"value\"\n            [attr.placeholder]=\"placeholder\"\n            [attr.title]=\"title\"\n            [kendoEventsOutsideAngular]=\"{\n                focus: handleInputFocus,\n                blur: handleInputBlur,\n                input: handleInput}\"/>\n        <span class=\"k-input-suffix\">\n            <span *ngIf=\"hasErrors\" [ngClass]=\"errorIconClasses\"></span>\n            <span *ngIf=\"isSuccessful\" [ngClass]=\"successIconClasses\"></span>\n            <span\n                role=\"button\"\n                class=\"k-clear-value\"\n                *ngIf=\"showClearButton\"\n                (click)=\"clearValue()\"\n                (mousedown)=\"$event.preventDefault()\"\n                [tabindex]=\"tabIndex\"\n                [attr.aria-label]=\"clearTitle()\"\n                [title]=\"clearTitle()\"\n                (keydown.enter)=\"clearValue($event)\"\n                (keydown.space)=\"clearValue($event)\"\n                >\n                <span [ngClass]=\"clearButtonClasses\"></span>\n            </span>\n            <ng-template\n                *ngIf=\"suffixTemplate\"\n                [ngTemplateOutlet]=\"suffixTemplate?.templateRef\">\n            </ng-template>\n        </span>\n    "
                },] },
    ];
    /** @nocollapse */
    TextBoxComponent.ctorParameters = function () { return [
        { type: kendo_angular_l10n_1.LocalizationService },
        { type: core_1.NgZone },
        { type: core_1.ChangeDetectorRef },
        { type: core_1.Renderer2 },
        { type: core_1.Injector },
        { type: core_1.ElementRef }
    ]; };
    TextBoxComponent.propDecorators = {
        title: [{ type: core_1.Input }],
        disabled: [{ type: core_1.Input }],
        readonly: [{ type: core_1.Input }],
        tabindex: [{ type: core_1.Input }],
        value: [{ type: core_1.Input }],
        selectOnFocus: [{ type: core_1.Input }],
        showSuccessIcon: [{ type: core_1.Input }],
        showErrorIcon: [{ type: core_1.Input }],
        clearButton: [{ type: core_1.Input }],
        successIcon: [{ type: core_1.Input }],
        errorIcon: [{ type: core_1.Input }],
        clearButtonIcon: [{ type: core_1.Input }],
        tabIndex: [{ type: core_1.Input }],
        placeholder: [{ type: core_1.Input }],
        valueChange: [{ type: core_1.Output }],
        inputFocus: [{ type: core_1.Output }],
        inputBlur: [{ type: core_1.Output }],
        onFocus: [{ type: core_1.Output, args: ['focus',] }],
        onBlur: [{ type: core_1.Output, args: ['blur',] }],
        input: [{ type: core_1.ViewChild, args: ['input', { static: true },] }],
        suffixTemplate: [{ type: core_1.ContentChild, args: [textbox_suffix_directive_1.TextBoxSuffixTemplateDirective,] }],
        prefixTemplate: [{ type: core_1.ContentChild, args: [textbox_prefix_directive_1.TextBoxPrefixTemplateDirective,] }],
        disabledClass: [{ type: core_1.HostBinding, args: ['class.k-state-disabled',] }],
        hostClass: [{ type: core_1.HostBinding, args: ['class.k-textbox',] }],
        isFocused: [{ type: core_1.HostBinding, args: ['class.k-state-focused',] }],
        direction: [{ type: core_1.HostBinding, args: ['attr.dir',] }]
    };
    return TextBoxComponent;
}());
exports.TextBoxComponent = TextBoxComponent;
