/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { closest } from './../common/dom-utils';
import { NG_VALUE_ACCESSOR, NgControl } from '@angular/forms';
import { Component, ElementRef, forwardRef, Input, Output, EventEmitter, HostBinding, ViewChild, NgZone, ChangeDetectorRef, Injector, ContentChild, Renderer2 } from '@angular/core';
import { invokeElementMethod } from '../common/dom-utils';
import { areSame, requiresZoneOnBlur } from '../common/utils';
import { hasObservers, KendoInput, Keys } from '@progress/kendo-angular-common';
import { TextBoxSuffixTemplateDirective } from './textbox-suffix.directive';
import { TextBoxPrefixTemplateDirective } from './textbox-prefix.directive';
import { LocalizationService, L10N_PREFIX } from '@progress/kendo-angular-l10n';
export class TextBoxComponent {
    constructor(localizationService, ngZone, changeDetector, renderer, injector, hostElement) {
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
        this.valueChange = new EventEmitter();
        /**
         * Fires each time the user focuses the `input` element.
         */
        this.inputFocus = new EventEmitter();
        /**
         * Fires each time the `input` element gets blurred.
         */
        this.inputBlur = new EventEmitter();
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
        this.onFocus = new EventEmitter();
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
        this.onBlur = new EventEmitter();
        this.hostClass = true;
        this._isFocused = false;
        this.focusChangedProgrammatically = false;
        /**
         * @hidden
         */
        this.handleInputFocus = () => {
            if (!this.disabled) {
                if (this.selectOnFocus && this.value) {
                    this.ngZone.run(() => {
                        setTimeout(() => { this.selectAll(); });
                    });
                }
                if (hasObservers(this.onFocus)) {
                    if (!this.isFocused) {
                        this.ngZone.run(() => {
                            this.onFocus.emit();
                        });
                    }
                }
                if (hasObservers(this.inputFocus)) {
                    if (!this.focusChangedProgrammatically || (this.focusChangedProgrammatically && this.clearButtonClicked)) {
                        this.ngZone.run(() => {
                            this.inputFocus.emit();
                        });
                    }
                }
                this.ngZone.run(() => {
                    this.isFocused = true;
                });
            }
        };
        /**
         * @hidden
         */
        this.handleInputBlur = () => {
            this.changeDetector.markForCheck();
            if (hasObservers(this.inputBlur) || requiresZoneOnBlur(this.control)) {
                this.ngZone.run(() => {
                    this.ngTouched();
                    this.inputBlur.emit();
                });
            }
        };
        /**
         * @hidden
         */
        this.handleInput = (ev) => {
            let incomingValue = ev.target.value;
            this.updateValue(incomingValue);
        };
        this.ngChange = (_) => { };
        this.ngTouched = () => { };
        this.direction = localizationService.rtl ? 'rtl' : 'ltr';
    }
    /**
     * @hidden
     */
    set tabIndex(tabIndex) {
        this.tabindex = tabIndex;
    }
    get tabIndex() {
        return this.tabindex;
    }
    get disabledClass() {
        return this.disabled;
    }
    get isFocused() {
        return this.disabled ? false : this._isFocused;
    }
    set isFocused(isFocused) {
        this._isFocused = isFocused;
    }
    ngOnInit() {
        this.control = this.injector.get(NgControl, null);
        this.checkClearButton();
        this.subscriptions = this.localizationService.changes.subscribe(({ rtl }) => {
            this.direction = rtl ? 'rtl' : 'ltr';
        });
    }
    ngAfterViewInit() {
        const hostElement = this.hostElement.nativeElement;
        let cursorInsideWrapper = false;
        let tabbing = false;
        this.ngZone.runOutsideAngular(() => {
            // focusIn and focusOut are relative to the host element
            this.subscriptions.add(this.renderer.listen(hostElement, 'focusin', () => {
                if (!this.isFocused) {
                    this.ngZone.run(() => {
                        this.onFocus.emit();
                        this.isFocused = true;
                    });
                }
            }));
            this.subscriptions.add(this.renderer.listen(hostElement, 'focusout', (args) => {
                if (!this.isFocused) {
                    return;
                }
                if (tabbing) {
                    const closestTextbox = closest(args.relatedTarget, (element) => element === this.hostElement.nativeElement);
                    if (!closestTextbox) {
                        this.handleBlur();
                    }
                    tabbing = false;
                }
                else {
                    if (!cursorInsideWrapper && !this.clearButtonClicked) {
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
    ngOnChanges(changes) {
        if (changes.disabled || changes.readonly) {
            this.checkClearButton();
        }
    }
    ngOnDestroy() {
        this.subscriptions.unsubscribe();
    }
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
    focus() {
        if (!this.input) {
            return;
        }
        this.focusChangedProgrammatically = true;
        this.isFocused = true;
        this.input.nativeElement.focus();
        this.focusChangedProgrammatically = false;
    }
    /**
     * Blurs the TextBox.
     */
    blur() {
        this.focusChangedProgrammatically = true;
        const isFocusedElement = this.hostElement.nativeElement.querySelector(':focus');
        if (isFocusedElement) {
            isFocusedElement.blur();
        }
        this.isFocused = false;
        this.focusChangedProgrammatically = false;
    }
    /**
     * @hidden
     */
    clearTitle() {
        return this.localizationService.get('clear');
    }
    /**
     * @hidden
     */
    checkClearButton() {
        this.showClearButton =
            !this.disabled &&
                !this.readonly &&
                this.clearButton &&
                !!this.value;
    }
    /**
     * @hidden
     */
    clearValue(ev) {
        if (ev) {
            ev.preventDefault();
        }
        this.clearButtonClicked = true;
        this.input.nativeElement.value = '';
        this.input.nativeElement.focus();
        this.updateValue(null);
        this.checkClearButton();
        this.clearButtonClicked = false;
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.value = value;
        this.checkClearButton();
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
     * Called when the status of the component changes to or from `disabled`.
     * Depending on the value, it enables or disables the appropriate DOM element.
     *
     * @param isDisabled
     */
    setDisabledState(isDisabled) {
        this.disabled = isDisabled;
    }
    /**
     * @hidden
     */
    showErrorsInitial() {
        if (!this.control) {
            return false;
        }
        const { invalid, dirty, touched } = this.control;
        return invalid && (dirty || touched);
    }
    /**
     * @hidden
     */
    showSuccessInitial() {
        if (!this.control) {
            return false;
        }
        const { valid, dirty, touched } = this.control;
        return valid && (dirty || touched);
    }
    /**
     * @hidden
     */
    get successIconClasses() {
        return this.successIcon
            ? `k-text-success ${this.successIcon}`
            : `k-text-success k-icon k-i-check-outline`;
    }
    /**
     * @hidden
     */
    get errorIconClasses() {
        return this.errorIcon
            ? `k-text-error ${this.errorIcon}`
            : `k-text-error k-icon k-i-warning`;
    }
    /**
     * @hidden
     */
    get clearButtonClasses() {
        return this.clearButtonIcon
            ? this.clearButtonIcon
            : `k-icon k-i-close-circle`;
    }
    /**
     * @hidden
     */
    get hasErrors() {
        return this.showErrorIcon === 'initial'
            ? this.showErrorsInitial()
            : this.showErrorIcon;
    }
    /**
     * @hidden
     */
    get isSuccessful() {
        return this.showSuccessIcon === 'initial'
            ? this.showSuccessInitial()
            : this.showSuccessIcon;
    }
    setSelection(start, end) {
        if (this.isFocused) {
            invokeElementMethod(this.input, 'setSelectionRange', start, end);
        }
    }
    selectAll() {
        if (this.value) {
            this.setSelection(0, this.value.length);
        }
    }
    updateValue(value) {
        if (!areSame(this.value, value)) {
            this.ngZone.run(() => {
                this.value = value;
                this.ngChange(value);
                this.valueChange.emit(value);
                this.checkClearButton();
                this.changeDetector.markForCheck();
            });
        }
    }
    handleBlur() {
        this.ngZone.run(() => {
            if (!this.focusChangedProgrammatically) {
                this.onBlur.emit();
            }
            this.isFocused = false;
        });
    }
}
TextBoxComponent.decorators = [
    { type: Component, args: [{
                exportAs: 'kendoTextBox',
                providers: [
                    LocalizationService,
                    { provide: L10N_PREFIX, useValue: 'kendo.textbox' },
                    {
                        provide: NG_VALUE_ACCESSOR,
                        useExisting: forwardRef(() => TextBoxComponent),
                        multi: true
                    },
                    { provide: KendoInput, useExisting: forwardRef(() => TextBoxComponent) }
                ],
                selector: 'kendo-textbox',
                template: `
        <ng-container kendoTextBoxLocalizedMessages
            i18n-clear="kendo.textbox.clear|The title for the **Clear** button in the TextBox."
            clear="Clear">
        </ng-container>
        <span class="k-input-prefix">
            <ng-template
                *ngIf="prefixTemplate"
                [ngTemplateOutlet]="prefixTemplate?.templateRef">
            </ng-template>
        </span>
        <input
            class="k-input"
            #input
            [disabled]="disabled"
            [readonly]="readonly"
            [attr.tabindex]="disabled ? undefined : tabindex"
            [value]="value"
            [attr.placeholder]="placeholder"
            [attr.title]="title"
            [kendoEventsOutsideAngular]="{
                focus: handleInputFocus,
                blur: handleInputBlur,
                input: handleInput}"/>
        <span class="k-input-suffix">
            <span *ngIf="hasErrors" [ngClass]="errorIconClasses"></span>
            <span *ngIf="isSuccessful" [ngClass]="successIconClasses"></span>
            <span
                role="button"
                class="k-clear-value"
                *ngIf="showClearButton"
                (click)="clearValue()"
                (mousedown)="$event.preventDefault()"
                [tabindex]="tabIndex"
                [attr.aria-label]="clearTitle()"
                [title]="clearTitle()"
                (keydown.enter)="clearValue($event)"
                (keydown.space)="clearValue($event)"
                >
                <span [ngClass]="clearButtonClasses"></span>
            </span>
            <ng-template
                *ngIf="suffixTemplate"
                [ngTemplateOutlet]="suffixTemplate?.templateRef">
            </ng-template>
        </span>
    `
            },] },
];
/** @nocollapse */
TextBoxComponent.ctorParameters = () => [
    { type: LocalizationService },
    { type: NgZone },
    { type: ChangeDetectorRef },
    { type: Renderer2 },
    { type: Injector },
    { type: ElementRef }
];
TextBoxComponent.propDecorators = {
    title: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    tabindex: [{ type: Input }],
    value: [{ type: Input }],
    selectOnFocus: [{ type: Input }],
    showSuccessIcon: [{ type: Input }],
    showErrorIcon: [{ type: Input }],
    clearButton: [{ type: Input }],
    successIcon: [{ type: Input }],
    errorIcon: [{ type: Input }],
    clearButtonIcon: [{ type: Input }],
    tabIndex: [{ type: Input }],
    placeholder: [{ type: Input }],
    valueChange: [{ type: Output }],
    inputFocus: [{ type: Output }],
    inputBlur: [{ type: Output }],
    onFocus: [{ type: Output, args: ['focus',] }],
    onBlur: [{ type: Output, args: ['blur',] }],
    input: [{ type: ViewChild, args: ['input', { static: true },] }],
    suffixTemplate: [{ type: ContentChild, args: [TextBoxSuffixTemplateDirective,] }],
    prefixTemplate: [{ type: ContentChild, args: [TextBoxPrefixTemplateDirective,] }],
    disabledClass: [{ type: HostBinding, args: ['class.k-state-disabled',] }],
    hostClass: [{ type: HostBinding, args: ['class.k-textbox',] }],
    isFocused: [{ type: HostBinding, args: ['class.k-state-focused',] }],
    direction: [{ type: HostBinding, args: ['attr.dir',] }]
};
