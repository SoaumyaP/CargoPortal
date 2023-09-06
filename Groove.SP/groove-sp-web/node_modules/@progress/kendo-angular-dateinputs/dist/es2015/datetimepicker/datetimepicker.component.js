/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, HostBinding, ViewChild, ElementRef, Input, isDevMode, TemplateRef, ChangeDetectorRef, Output, EventEmitter, NgZone, ViewContainerRef, forwardRef, ContentChild, Inject, ChangeDetectionStrategy, Renderer2 } from '@angular/core';
import { NG_VALUE_ACCESSOR, NG_VALIDATORS } from '@angular/forms';
import { Subscription, fromEvent } from 'rxjs';
import { tap } from 'rxjs/operators';
import { cloneDate, isEqual, getDate } from '@progress/kendo-date-math';
import { PopupService } from '@progress/kendo-angular-popup';
import { IntlService } from '@progress/kendo-angular-intl';
import { hasObservers, KendoInput, Keys, guid } from '@progress/kendo-angular-common';
import { LocalizationService, L10N_PREFIX } from '@progress/kendo-angular-l10n';
import { PickerService } from '../common/picker.service';
import { DisabledDatesService } from '../calendar/services/disabled-dates.service';
import { isPresent } from '../common/utils';
import { mergeDateAndTime, noop, lastMillisecondOfDate, isInRange, isValidRange, isWindowAvailable } from '../util';
import { PreventableEvent } from '../preventable-event';
import { minValidator } from '../validators/min.validator';
import { maxValidator } from '../validators/max.validator';
import { disabledDatesValidator } from '../validators/disabled-date.validator';
import { TIME_PART } from '../timepicker/models/time-part.default';
import { MIN_DATE, MAX_DATE, MIN_TIME, MAX_TIME } from '../defaults';
import { TOUCH_ENABLED } from '../touch-enabled';
import { CellTemplateDirective } from '../calendar/templates/cell-template.directive';
import { MonthCellTemplateDirective } from '../calendar/templates/month-cell-template.directive';
import { YearCellTemplateDirective } from '../calendar/templates/year-cell-template.directive';
import { DecadeCellTemplateDirective } from '../calendar/templates/decade-cell-template.directive';
import { CenturyCellTemplateDirective } from '../calendar/templates/century-cell-template.directive';
import { WeekNumberCellTemplateDirective } from '../calendar/templates/weeknumber-cell-template.directive';
import { HeaderTitleTemplateDirective } from '../calendar/templates/header-title-template.directive';
import { incompleteDateValidator } from '../validators/incomplete-date.validator';
const timeFormatRegExp = new RegExp(`${TIME_PART.hour}|${TIME_PART.minute}|${TIME_PART.second}|${TIME_PART.dayperiod}|literal`);
const VALUE_DOC_LINK = 'https://www.telerik.com/kendo-angular-ui/components/dateinputs/datetimepicker/integration-with-json/';
const MIN_MAX_DOC_LINK = 'https://www.telerik.com/kendo-angular-ui/components/dateinputs/datetimepicker/date-time-limits/';
const DEFAULT_ACTIVE_TAB = 'date';
const DEFAULT_DATEINPUT_FORMAT = 'g';
const DEFAULT_TIMESELECTOR_FORMAT = 't';
/**
 * Represents the [Kendo UI DateTimePicker component for Angular]({% slug overview_datetimepicker %}).
 */
export class DateTimePickerComponent {
    constructor(popupService, intl, cdr, pickerService, ngZone, host, touchEnabled, localization, disabledDatesService, renderer) {
        this.popupService = popupService;
        this.intl = intl;
        this.cdr = cdr;
        this.pickerService = pickerService;
        this.ngZone = ngZone;
        this.host = host;
        this.touchEnabled = touchEnabled;
        this.localization = localization;
        this.disabledDatesService = disabledDatesService;
        this.renderer = renderer;
        /**
         * @hidden
         */
        this.hostClasses = true;
        /**
         * @hidden
         */
        this.focusableId = `k-${guid()}`;
        /**
         * Sets the title of the input element of the DateTimePicker.
         */
        this.title = '';
        /**
         * Sets or gets the `disabled` property of the DateTimePicker and determines whether the component is active
         * ([see example]({% slug disabled_datetimepicker %})).
         */
        this.disabled = false;
        /**
         * Sets the read-only state of the DateTimePicker
         * ([see example]({% slug readonly_datetimepicker %}#toc-read-only-datetimepicker)).
         */
        this.readonly = false;
        /**
         * Sets the read-only state of the DateTimePicker input field
         * ([see example]({% slug readonly_datetimepicker %}#toc-read-only-input)).
         *
         * > Note that if you set the [`readonly`]({% slug api_dateinputs_datetimepickercomponent %}#toc-readonly) property value to `true`,
         * the input will be rendered in a read-only state regardless of the `readOnlyInput` value.
         */
        this.readOnlyInput = false;
        /**
         * Determines whether to display the **Cancel** button in the popup
         * ([see example]({% slug datetimepicker_popup_options %}#toc-toggling-the-cancel-button)).
         */
        this.cancelButton = true;
        /**
         * Determines whether to display a week number column in the `month` view of the popup Calendar
         * ([see example]({% slug datetimepicker_calendar_options %}#toc-week-number-column)).
         */
        this.weekNumber = false;
        /**
         * Determines whether the built-in min or max validators are enforced when validating a form
         * ([see example]({% slug dateranges_datetimepicker %}#toc-prevent-invalid-input)).
         */
        this.rangeValidation = true;
        /**
         * Determines whether the built-in validator for disabled
         * date ranges is enforced when validating a form
         * ([see example]({% slug disabled_dates_datetimepicker %}#toc-validation)).
         */
        this.disabledDatesValidation = true;
        /**
         * Determines whether the built-in validation for incomplete dates is to be enforced when a form is being validated.
         */
        this.incompleteDateValidation = false;
        /**
         * Fires each time the user selects a new value.
         * ([more information and example]({% slug overview_datetimepicker %}#toc-methods-and-events)).
         */
        this.valueChange = new EventEmitter();
        /**
         * Fires each time the popup is about to open.
         * This event is preventable. If you cancel the event by setting `event.preventDefault()`, the popup will remain closed.
         * ([more information and example]({% slug overview_datetimepicker %}#toc-methods-and-events)).
         */
        this.open = new EventEmitter();
        /**
         * Fires each time the popup is about to close.
         * This event is preventable. If you cancel the event by setting `event.preventDefault()`, the popup will remain open.
         * ([more information and example]({% slug overview_datetimepicker %}#toc-methods-and-events)).
         */
        this.close = new EventEmitter();
        /**
         * Fires each time the user focuses the component.
         * ([more information and example]({% slug overview_datetimepicker %}#toc-methods-and-events)).
         */
        this.onFocus = new EventEmitter();
        /**
         * Fires each time the user blurs the component.
         * ([more information and example]({% slug overview_datetimepicker %}#toc-methods-and-events)).
         */
        this.onBlur = new EventEmitter();
        /**
         * @hidden
         *
         * Controls whether the Calendar or the TimeSelector will be displayed.
         */
        this.activeTab = DEFAULT_ACTIVE_TAB;
        /**
         * @hidden
         *
         * Specifies the stripped time-related format that is used in the TimeSelector.
         * Updates each time the `format` property value changes.
         */
        this.timeSelectorFormat = DEFAULT_TIMESELECTOR_FORMAT;
        /**
         * @hidden
         */
        this.timeSelectorMin = cloneDate(MIN_TIME);
        /**
         * @hidden
         */
        this.timeSelectorMax = cloneDate(MAX_TIME);
        /**
         * @hidden
         */
        this.calendarValue = null;
        /**
         * @hidden
         */
        this.calendarMin = cloneDate(MIN_DATE);
        /**
         * @hidden
         */
        this.calendarMax = lastMillisecondOfDate(MAX_DATE);
        this._popupSettings = { animate: true };
        this._value = null;
        this._format = DEFAULT_DATEINPUT_FORMAT;
        this._tabindex = 0;
        this._defaultTab = DEFAULT_ACTIVE_TAB;
        this._min = mergeDateAndTime(MIN_DATE, MIN_TIME);
        this._max = mergeDateAndTime(MAX_DATE, MAX_TIME);
        this._isActive = false;
        this.onControlTouched = noop;
        this.onControlChange = noop;
        this.onValidatorChange = noop;
        this.minValidateFn = noop;
        this.maxValidateFn = noop;
        this.disabledDatesValidateFn = noop;
        this.incompleteValidator = noop;
        this.subscriptions = new Subscription();
    }
    /**
     * @hidden
     */
    get input() {
        return this.pickerService.input;
    }
    /**
     * @hidden
     */
    get calendar() {
        return this.pickerService.calendar;
    }
    /**
     * @hidden
     */
    get timeSelector() {
        return this.pickerService.timeSelector;
    }
    /**
     * Specifies the value of the DateTimePicker component.
     *
     * > The `value` has to be a valid [JavaScript `Date`](https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Date) instance.
     */
    set value(value) {
        this.verifyValue(value);
        this._value = cloneDate(value);
        this.setCalendarValue(value);
        this.cdr.markForCheck();
    }
    get value() {
        return this._value;
    }
    /**
     * Specifies the date format for displaying the input value
     * ([see example]({% slug formats_datetimepicker %})).
     */
    set format(value) {
        this._format = value;
        this.timeSelectorFormat = this.getTimeSelectorFormat(value);
    }
    get format() {
        return this._format;
    }
    /**
     * Specifies the [`tabindex`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex) of the DateTimePicker.
     */
    set tabindex(value) {
        const tabindex = Number(value);
        const defaultValue = 0;
        this._tabindex = !isNaN(tabindex) ? tabindex : defaultValue;
    }
    get tabindex() {
        return this.disabled ? -1 : this._tabindex;
    }
    /**
     * Sets the dates of the DateTimePicker that will be disabled
     * ([see example]({% slug disabled_dates_datetimepicker %})).
     */
    set disabledDates(value) {
        this._disabledDates = value;
        this.disabledDatesService.initialize(value);
    }
    get disabledDates() {
        return this._disabledDates;
    }
    /**
     * Configures the popup settings of the DateTimePicker
     * ([see example]({% slug datetimepicker_popup_options %}#toc-customizing-the-popup)).
     *
     * The available options are:
     * - `animate: Boolean`&mdash;Controls the popup animation. By default, the open and close animations are enabled.
     * - `appendTo: 'root' | 'component' | ViewContainerRef`&mdash;Controls the popup container. By default, the popup will be appended to the root component.
     * - `popupClass: String`&mdash;Specifies a list of CSS classes that are used to style the popup.
     */
    set popupSettings(settings) {
        this._popupSettings = Object.assign({}, { animate: true }, settings);
    }
    get popupSettings() {
        return this._popupSettings;
    }
    /**
     * Specifies the smallest valid date.
     * The Calendar will not display dates before this value.
     * If the `min` value of the Calendar is selected, the TimePicker will not display
     * time entries before the specified time portion of this value
     * ([see example]({% slug dateranges_datetimepicker %})).
     */
    set min(value) {
        if (!isPresent(value)) {
            return;
        }
        this._min = cloneDate(value);
        this.calendarMin = getDate(value);
    }
    get min() {
        return this._min;
    }
    /**
     * Specifies the biggest valid date.
     * The Calendar will not display dates after this value.
     * If the `max` value of the Calendar is selected, the TimePicker will not display
     * time entries after the specified time portion of this value
     * ([see example]({% slug dateranges_datetimepicker %})).
     */
    set max(value) {
        if (!isPresent(value)) {
            return;
        }
        this._max = cloneDate(value);
        this.calendarMax = lastMillisecondOfDate(value);
    }
    get max() {
        return this._max;
    }
    /**
     * Indicates whether the component is currently open.
     */
    get isOpen() {
        return isPresent(this.popupRef);
    }
    /**
     * Indicates whether the component or its popup content is focused.
     */
    get isActive() {
        return this._isActive;
    }
    set isActive(value) {
        if (value) {
            this.renderer.addClass(this.wrapper.nativeElement, 'k-state-focused');
        }
        else {
            this.renderer.removeClass(this.wrapper.nativeElement, 'k-state-focused');
        }
        this._isActive = value;
    }
    /**
     * Sets the active tab on opening the popup
     * ([see example]({% slug datetimepicker_popup_options %}#toc-setting-the-default-tab)).
     */
    set defaultTab(tab) {
        this._defaultTab = tab || DEFAULT_ACTIVE_TAB;
        this.activeTab = this.defaultTab;
    }
    get defaultTab() {
        return this._defaultTab;
    }
    /**
     * @hidden
     */
    get tabSwitchTransition() {
        /*
         When the popup is opening, disables the set transition in the themes. When `defaultTab` is set to `time`,
         the popup opens with an active **Time** tab and the animation of the initial transition is undesired.
         Setting the inline transition style to `none` overrides the set animation in the themes.
         Setting the inline transition style to `null` does not apply any inline styles or override the themes CSS.
        */
        return this.isOpen ? null : 'none';
    }
    /**
     * @hidden
     *
     * Indicates whether the Calendar will be disabled.
     * The inactive tab component gets disabled and becomes inaccessible on tab click.
     */
    get disableCalendar() {
        return this.activeTab !== 'date' && !this.calendar.isActive;
    }
    /**
     * @hidden
     */
    get inputRole() {
        return this.readOnlyInput ? 'listbox' : 'spinbutton';
    }
    /**
     * @hidden
     *
     * Indicates whether the TimeSelector will be disabled.
     * The inactive tab component gets disabled and becomes inaccessible on tab click.
     */
    get disableTimeSelector() {
        return this.activeTab !== 'time' && !this.timeSelector.isActive;
    }
    get activeTabComponent() {
        if (!this.isOpen) {
            return;
        }
        if (!(isPresent(this.calendar) || isPresent(this.timeSelector))) {
            this.cdr.detectChanges();
        }
        return this.activeTab === 'date' ? this.calendar : this.timeSelector;
    }
    get appendTo() {
        const { appendTo } = this.popupSettings;
        if (!isPresent(appendTo) || appendTo === 'root') {
            return undefined;
        }
        return appendTo === 'component' ? this.container : appendTo;
    }
    ngOnInit() {
        this.subscriptions.add(this.pickerService.onFocus
            // detect popup changes to disable the inactive view mark-up when the popup is open
            .pipe(tap(this.detectPopupChanges.bind(this)))
            .subscribe(this.handleFocus.bind(this)));
        this.subscriptions.add(this.pickerService.onBlur.subscribe(this.handleBlur.bind(this)));
        this.subscriptions.add(this.pickerService.sameDateSelected.subscribe(this.handleCalendarValueChange.bind(this)));
        this.subscriptions.add(this.localization.changes.subscribe(() => this.cdr.markForCheck()));
        this.subscriptions.add(this.pickerService.dateCompletenessChange.subscribe(this.handleDateCompletenessChange.bind(this)));
        if (isWindowAvailable()) {
            this.subscriptions.add(this.ngZone.runOutsideAngular(() => fromEvent(window, 'blur').subscribe(this.handleCancel.bind(this))));
        }
    }
    ngOnChanges(changes) {
        if (isPresent(changes.min) || isPresent(changes.max)) {
            this.verifyMinMaxRange();
        }
        if (changes.min || changes.max || changes.rangeValidation || changes.disabledDatesValidation || changes.disabledDates || changes.incompleteDateValidation) {
            this.minValidateFn = this.rangeValidation ? minValidator(this.min) : noop;
            this.maxValidateFn = this.rangeValidation ? maxValidator(this.max) : noop;
            this.disabledDatesValidateFn = this.disabledDatesValidation ? disabledDatesValidator(this.disabledDatesService.isDateDisabled) : noop;
            this.incompleteValidator = this.incompleteDateValidation ? incompleteDateValidator() : noop;
            this.onValidatorChange();
        }
    }
    ngOnDestroy() {
        if (this.isOpen) {
            this.closePopup();
        }
        this.subscriptions.unsubscribe();
    }
    /**
     * * If the popup is closed, focuses the DateTimePicker input.
     * * If the popup is open, the focus is moved to its content.
     */
    focus() {
        if (this.disabled) {
            return;
        }
        if (this.isOpen) {
            this.activeTabComponent.focus();
        }
        else {
            this.input.focus();
        }
    }
    /**
     * Blurs the DateTimePicker.
     */
    blur() {
        if (this.isOpen && this.activeTabComponent.isActive) {
            this.activeTabComponent.blur();
        }
        else {
            this.input.blur();
        }
    }
    /**
     * Toggles the visibility of the popup. If you use the `toggle` method to show or hide the popup,
     * the `open` and `close` events do not fire.
     * ([more information and example]({% slug overview_datetimepicker %}#toc-methods-and-events)).
     *
     * @param show - The state of the popup.
     */
    toggle(show) {
        if (this.disabled || this.readonly || show === this.isOpen) {
            return;
        }
        const shouldOpen = isPresent(show) ? show : !this.isOpen;
        if (shouldOpen) {
            this.openPopup();
        }
        else {
            this.closePopup();
            // Changes the tab and the calendar or clock icon to the designated default.
            if (this.activeTab !== this.defaultTab) {
                this.activeTab = this.defaultTab;
                this.cdr.detectChanges();
            }
        }
    }
    /**
     * @hidden
     */
    writeValue(value) {
        this.value = value;
    }
    /**
     * @hidden
     */
    registerOnChange(fn) {
        this.onControlChange = fn;
    }
    /**
     * @hidden
     */
    registerOnTouched(fn) {
        this.onControlTouched = fn;
    }
    /**
     * @hidden
     */
    setDisabledState(disabled) {
        this.disabled = disabled;
        this.cdr.markForCheck();
    }
    /**
     * @hidden
     */
    validate(control) {
        return this.minValidateFn(control) || this.maxValidateFn(control) || this.disabledDatesValidateFn(control) || this.incompleteValidator(control, this.input && this.input.isDateIncomplete);
    }
    /**
     * @hidden
     */
    registerOnValidatorChange(fn) {
        this.onValidatorChange = fn;
    }
    /**
     * @hidden
     *
     * Used by the TextBoxContainer to determine if the floating label will render in the input.
     */
    isEmpty() {
        return !isPresent(this.value) && this.input.isEmpty();
    }
    /**
     * @hidden
     */
    handleIconClick(event) {
        if (this.disabled || this.readonly) {
            return;
        }
        // prevents the event default to evade focusing the DateInput input when placed inside a label (FF/IE/Edge)
        event.preventDefault();
        const runInZone = !this.isOpen || hasObservers(this.close);
        this.run(runInZone, () => {
            const shouldOpen = !this.isOpen;
            // handle focus first to maintain correct event order `focus` => `open`
            this.handleFocus();
            this.togglePopup(shouldOpen);
            this.switchFocus();
        });
    }
    /**
     * @hidden
     */
    handleFocus() {
        if (this.isActive) {
            return;
        }
        this.isActive = true;
        if (hasObservers(this.onFocus)) {
            this.ngZone.run(() => this.onFocus.emit());
        }
    }
    /**
     * @hidden
     */
    handleBlur(event) {
        if (!this.isActive || this.focusTargetInComponent(event)) {
            return;
        }
        this.isActive = false;
        const isNgControlUntouched = this.host.nativeElement.classList.contains('ng-untouched');
        const runInZone = isNgControlUntouched || hasObservers(this.onBlur) || (this.isOpen && hasObservers(this.close));
        this.run(runInZone, () => {
            this.onBlur.emit();
            this.onControlTouched();
            this.togglePopup(false);
            this.cdr.markForCheck();
        });
    }
    /**
     * @hidden
     */
    changeActiveTab(tab) {
        if (!this.isOpen || this.activeTab === tab) {
            return;
        }
        // persists the Tcurrent value of the TimeSelector when switching between tabs
        if (!isEqual(this.timeSelector.value, this.timeSelector.current)) {
            this.timeSelector.handleAccept();
        }
        this.activeTab = tab;
        this.cdr.detectChanges();
        this.detectPopupChanges();
    }
    /**
     * @hidden
     */
    handleTabChangeTransitionEnd(dateTimeSelector, event) {
        // handle only the .k-datetime-selector element transition, ignore any child element transitions
        if (event.target !== dateTimeSelector) {
            return;
        }
        this.activeTabComponent.focus();
    }
    /**
     * @hidden
     */
    handleAccept() {
        if (!this.isOpen) {
            return;
        }
        const candidate = mergeDateAndTime(this.calendar.value, this.timeSelector.current);
        const valueChangePresent = !isEqual(this.value, candidate);
        const runInZone = valueChangePresent || hasObservers(this.close);
        this.run(runInZone, () => {
            this.handleValueChange(candidate);
            this.togglePopup(false);
        });
    }
    /**
     * @hidden
     */
    handleCancel() {
        if (!this.isOpen) {
            return;
        }
        const runInZone = hasObservers(this.close);
        this.run(runInZone, () => this.togglePopup(false));
    }
    /**
     * @hidden
     */
    handleInputValueChange(value) {
        this.handleValueChange(value);
        if (this.isOpen) {
            this.togglePopup(false);
        }
    }
    /**
     * @hidden
     */
    handleCalendarValueChange() {
        this.setTimeSelectorMinMax(this.calendar.value);
        this.changeActiveTab('time');
    }
    /**
     * @hidden
     */
    handleKeyDown(event) {
        if (this.disabled || this.readonly) {
            return;
        }
        const { keyCode, altKey } = event;
        switch (keyCode) {
            case altKey && Keys.ArrowUp:
            case Keys.Escape:
                this.handleCancel();
                break;
            case !this.isOpen && altKey && Keys.ArrowDown:
                this.ngZone.run(() => this.togglePopup(true));
                break;
            case altKey && Keys.ArrowRight:
                this.changeActiveTab('time');
                break;
            case altKey && Keys.ArrowLeft:
                this.changeActiveTab('date');
                break;
            case this.isOpen && this.timeSelector.isActive && isPresent(this.calendarValue) && Keys.Enter:
                this.handleAccept();
                break;
            default: return;
        }
        event.preventDefault();
    }
    /**
     * @hidden
     */
    handleTabOut(event) {
        const { keyCode, shiftKey, target } = event;
        // if no focusable next sibling elements exist in the controls sections, the user is tabbing out of the popup
        const focusableSiblingAvailable = isPresent(target.nextElementSibling) && !target.nextElementSibling.disabled;
        if (keyCode === Keys.Tab && !shiftKey && !focusableSiblingAvailable) {
            this.input.focus();
            this.handleCancel();
        }
    }
    /**
     * @hidden
     */
    handleBackTabOut(event) {
        const { keyCode, shiftKey } = event;
        if (keyCode === Keys.Tab && shiftKey) {
            this.input.focus();
        }
    }
    /**
     * @hidden
     *
     * Prevents the diversion of the focus from the currently active element in the component.
     */
    preventMouseDown(event) {
        event.preventDefault();
    }
    verifyValue(value) {
        if (!isDevMode()) {
            return;
        }
        if (isPresent(value) && !(value instanceof Date)) {
            throw new Error(`The 'value' should be a valid JavaScript Date instance. Check ${VALUE_DOC_LINK} for possible resolution.`);
        }
    }
    verifyMinMaxRange() {
        if (!isDevMode()) {
            return;
        }
        if (!isValidRange(this.min, this.max)) {
            throw new Error(`The max value should be bigger than the min. See ${MIN_MAX_DOC_LINK}.`);
        }
    }
    /**
     * Extracts the time slots and the literals that are not preceded by date parts
     * and concatenates the resulting parts into a string.
     * If the provided format value does not contain any time parts,
     * returns the designated format of the default popup component of the TimePicker.
     */
    getTimeSelectorFormat(format) {
        const timeSelectorFormat = this.intl
            .splitDateFormat(format)
            .filter(this.timeFormatPartFilter)
            .reduce((format, part) => format += part.pattern, '');
        return timeSelectorFormat || DEFAULT_TIMESELECTOR_FORMAT;
    }
    /**
     * The filter expression that filters out all format parts
     * except for `hour`, `minute`, `second`, `dayperiod`, and specific literals.
     * Literals will be left only if they are not preceded by date parts.
     */
    timeFormatPartFilter(part, index, parts) {
        const previousPart = index >= 1 && parts[index - 1];
        if (previousPart && part.type === 'literal') {
            return timeFormatRegExp.test(previousPart.type);
        }
        return timeFormatRegExp.test(part.type);
    }
    togglePopup(open) {
        if (open === this.isOpen) {
            return;
        }
        const event = new PreventableEvent();
        open ? this.open.emit(event) : this.close.emit(event);
        if (event.isDefaultPrevented()) {
            return;
        }
        this.toggle(open);
        this.switchFocus();
    }
    switchFocus() {
        if (!this.isActive) {
            return;
        }
        if (this.isOpen) {
            this.activeTabComponent.focus();
        }
        else if (!this.touchEnabled) {
            this.input.focus();
        }
        else if (!this.input.isActive) {
            this.handleBlur();
        }
    }
    openPopup() {
        this.setCalendarValue(this.value);
        this.setTimeSelectorMinMax(this.value);
        const direction = this.localization.rtl ? 'right' : 'left';
        this.popupRef = this.popupService.open({
            anchor: this.wrapper,
            content: this.popupTemplate,
            positionMode: 'absolute',
            animate: this.popupSettings.animate,
            appendTo: this.appendTo,
            popupClass: `k-datetime-container ${this.popupSettings.popupClass || ''}`,
            anchorAlign: { vertical: 'bottom', horizontal: direction },
            popupAlign: { vertical: 'top', horizontal: direction }
        });
        this.popupRef.popupAnchorViewportLeave.subscribe(() => this.handleCancel());
    }
    closePopup() {
        if (!this.isOpen) {
            return;
        }
        this.popupRef.close();
        this.popupRef = null;
    }
    handleValueChange(value) {
        if (isEqual(this.value, value)) {
            return;
        }
        this.value = cloneDate(value);
        this.onControlChange(cloneDate(value));
        this.valueChange.emit(cloneDate(value));
    }
    /**
     * Indicates whether the focus target is part of this component,
     * that is, whether the focus target is inside the component wrapper or in the popup.
     */
    focusTargetInComponent(event) {
        if (!isPresent(event)) {
            return false;
        }
        const relatedTarget = event.relatedTarget || document.activeElement;
        const focusInPopup = isPresent(this.popupRef) && this.popupRef.popupElement.contains(relatedTarget);
        const focusInWrapper = this.wrapper.nativeElement.contains(relatedTarget);
        return focusInPopup || focusInWrapper;
    }
    setTimeSelectorMinMax(selectedDate) {
        const minDateSelected = isPresent(selectedDate) && isEqual(getDate(selectedDate), getDate(this.min));
        this.timeSelectorMin = cloneDate(minDateSelected ? this.min : MIN_TIME);
        const maxDateSelected = isPresent(selectedDate) && isEqual(getDate(selectedDate), getDate(this.max));
        this.timeSelectorMax = cloneDate(maxDateSelected ? this.max : MAX_TIME);
    }
    setCalendarValue(value) {
        const isInCalendarRange = isPresent(value) && isInRange(value, this.calendarMin, this.calendarMax);
        this.calendarValue = isInCalendarRange ? getDate(value) : null;
    }
    /**
     * If the popup is available, runs a popup change detection.
     */
    detectPopupChanges() {
        if (!this.isOpen) {
            return;
        }
        this.popupRef.popup.changeDetectorRef.detectChanges();
    }
    /**
     * Depending on the predicate `runInZone` value that is passed,
     * runs the provided function either in the Angular or in the current zone.
     */
    run(runInZone, fn) {
        if (runInZone) {
            this.ngZone.run(() => fn());
        }
        else {
            fn();
        }
    }
    handleDateCompletenessChange() {
        this.cdr.markForCheck();
        this.ngZone.run(() => this.onValidatorChange());
    }
}
DateTimePickerComponent.decorators = [
    { type: Component, args: [{
                selector: 'kendo-datetimepicker',
                exportAs: 'kendo-datetimepicker',
                changeDetection: ChangeDetectionStrategy.OnPush,
                providers: [
                    PickerService,
                    LocalizationService,
                    DisabledDatesService,
                    { provide: L10N_PREFIX, useValue: 'kendo.datetimepicker' },
                    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DateTimePickerComponent), multi: true },
                    { provide: NG_VALIDATORS, useExisting: forwardRef(() => DateTimePickerComponent), multi: true },
                    { provide: KendoInput, useExisting: forwardRef(() => DateTimePickerComponent) }
                ],
                template: `
        <ng-container
            kendoDateTimePickerLocalizedMessages

            i18n-dateTab="kendo.datetimepicker.dateTab|The Date tab text in the datetimepicker popup header"
            dateTab="Date"

            i18n-dateTabLabel="kendo.datetimepicker.dateTabLabel|The label for the Date tab in the datetimepicker popup header"
            dateTabLabel="Date tab"

            i18n-timeTab="kendo.datetimepicker.timeTab|The Time tab text in the datetimepicker popup header"
            timeTab="Time"

            i18n-timeTabLabel="kendo.datetimepicker.timeTabLabel|The label for the Time tab in the datetimepicker popup header"
            timeTabLabel="Time tab"

            i18n-toggle="kendo.datetimepicker.toggle|The label for the toggle button in the datetimepicker component"
            toggle="Toggle popup"

            i18n-accept="kendo.datetimepicker.accept|The Accept button text in the datetimepicker component"
            accept="Set"

            i18n-acceptLabel="kendo.datetimepicker.acceptLabel|The label for the Accept button in the datetimepicker component"
            acceptLabel="Set"

            i18n-cancel="kendo.datetimepicker.cancel|The Cancel button text in the datetimepicker component"
            cancel="Cancel"

            i18n-cancelLabel="kendo.datetimepicker.cancelLabel|The label for the Cancel button in the datetimepicker component"
            cancelLabel="Cancel"

            i18n-now="kendo.datetimepicker.now|The Now button text in the timepicker component"
            now="NOW"

            i18n-nowLabel="kendo.datetimepicker.nowLabel|The label for the Now button in the timepicker component"
            nowLabel="Select now"

            i18n-today="kendo.datetimepicker.today|The label for the today button in the calendar header"
            today="TODAY"
        >
        </ng-container>

        <span
            #wrapper
            class="k-picker-wrap"
            [class.k-state-disabled]="disabled"
        >
            <kendo-dateinput
                [value]="value"
                [format]="format"
                [min]="min"
                [max]="max"
                [incompleteDateValidation]="incompleteDateValidation"
                [formatPlaceholder]="formatPlaceholder"
                [placeholder]="placeholder"
                [disabled]="disabled"
                [readonly]="readonly || readOnlyInput"
                [role]="inputRole"
                [ariaReadOnly]="readonly"
                [steps]="steps"
                [tabindex]="tabindex"
                [title]="title"
                [focusableId]="focusableId"
                [hasPopup]="true"
                [isPopupOpen]="isOpen"
                (valueChange)="handleInputValueChange($event)"
                [kendoEventsOutsideAngular]="{
                    keydown: handleKeyDown
                }"
                [scope]="this"
            >
            </kendo-dateinput>
            <span class="k-select"
                [attr.title]="localization.get('toggle')"
                [attr.aria-label]="localization.get('toggle')"
                [kendoEventsOutsideAngular]="{
                    mousedown: preventMouseDown,
                    click: handleIconClick
                }"
                [scope]="this"
            >
                <span class="k-link k-link-date">
                    <span
                        class="k-icon"
                        [class.k-i-calendar]="activeTab === 'date'"
                        [class.k-i-clock]="activeTab === 'time'"
                    >
                    </span>
                </span>
            </span>
        </span>

        <ng-container #container></ng-container>

        <ng-template #popupTemplate>
            <div
                class="k-datetime-wrap k-{{activeTab}}-tab"
                [kendoEventsOutsideAngular]="{
                    mousedown: preventMouseDown,
                    keydown: handleKeyDown
                }"
                [scope]="this"
            >
                <div class="k-datetime-buttongroup"
                    [kendoEventsOutsideAngular]="{
                        focusin: handleFocus,
                        focusout: handleBlur
                    }"
                    [scope]="this"
                >
                    <div class="k-button-group k-button-group-stretched">
                        <button
                            type="button"
                            class="k-button k-date-tab"
                            [class.k-state-active]="activeTab === 'date'"
                            [attr.title]="localization.get('dateTabLabel')"
                            [attr.aria-label]="localization.get('dateTabLabel')"
                            [kendoEventsOutsideAngular]="{
                                click: changeActiveTab.bind(this, 'date'),
                                keydown: handleBackTabOut
                            }"
                            [scope]="this"
                        >
                            {{localization.get('dateTab')}}
                        </button>
                        <button
                            type="button"
                            class="k-button k-time-tab"
                            [class.k-state-active]="activeTab === 'time'"
                            [attr.title]="localization.get('timeTabLabel')"
                            [attr.aria-label]="localization.get('timeTabLabel')"
                            [kendoEventsOutsideAngular]="{
                                click: changeActiveTab.bind(this, 'time')
                            }"
                        >
                            {{localization.get('timeTab')}}
                        </button>
                    </div>
                </div>
                <div
                    #dateTimeSelector
                    class="k-datetime-selector"
                    [style.transition]="tabSwitchTransition"
                    [kendoEventsOutsideAngular]="{
                        transitionend: handleTabChangeTransitionEnd.bind(this, dateTimeSelector)
                    }"
                >
                    <div class="k-datetime-calendar-wrap">
                        <kendo-calendar
                            [(value)]="calendarValue"
                            [min]="calendarMin"
                            [max]="calendarMax"
                            [focusedDate]="focusedDate"
                            [weekNumber]="weekNumber"
                            [navigation]="false"
                            [cellTemplate]="cellTemplate"
                            [monthCellTemplate]="monthCellTemplate"
                            [yearCellTemplate]="yearCellTemplate"
                            [decadeCellTemplate]="decadeCellTemplate"
                            [centuryCellTemplate]="centuryCellTemplate"
                            [weekNumberTemplate]="weekNumberTemplate"
                            [headerTitleTemplate]="headerTitleTemplate"
                            [disabled]="disableCalendar"
                            [disabledDates]="disabledDates"
                            (valueChange)="handleCalendarValueChange()"
                        >
                            <kendo-calendar-messages
                                [today]="localization.get('today')"
                            >
                            </kendo-calendar-messages>
                        </kendo-calendar>
                    </div>
                    <div class="k-datetime-time-wrap">
                        <kendo-timeselector
                            [value]="value"
                            [format]="timeSelectorFormat"
                            [min]="timeSelectorMin"
                            [max]="timeSelectorMax"
                            [setButton]="false"
                            [cancelButton]="false"
                            [steps]="steps"
                            [disabled]="disableTimeSelector"
                        >
                            <kendo-timeselector-messages
                                [now]="localization.get('now')"
                                [nowLabel]="localization.get('nowLabel')"
                            >
                            </kendo-timeselector-messages>
                        </kendo-timeselector>
                    </div>
                </div>
                <div
                    class="k-datetime-footer k-action-buttons"
                    [kendoEventsOutsideAngular]="{
                        keydown: handleTabOut,
                        focusin: handleFocus,
                        focusout: handleBlur
                    }"
                    [scope]="this"
                >
                    <button
                        *ngIf="cancelButton"
                        type="button"
                        class="k-button k-time-cancel"
                        [attr.title]="localization.get('cancelLabel')"
                        [attr.aria-label]="localization.get('cancelLabel')"
                        [kendoEventsOutsideAngular]="{
                            click: handleCancel
                        }"
                        [scope]="this"
                    >
                        {{localization.get('cancel')}}
                    </button>
                    <button
                        type="button"
                        class="k-time-accept k-button k-primary"
                        [attr.title]="localization.get('acceptLabel')"
                        [attr.aria-label]="localization.get('acceptLabel')"
                        [disabled]="!calendarValue"
                        [kendoEventsOutsideAngular]="{
                            click: handleAccept
                        }"
                        [scope]="this"
                    >
                        {{localization.get('accept')}}
                    </button>
                </div>
            </div>
        </ng-template>
    `
            },] },
];
/** @nocollapse */
DateTimePickerComponent.ctorParameters = () => [
    { type: PopupService },
    { type: IntlService },
    { type: ChangeDetectorRef },
    { type: PickerService },
    { type: NgZone },
    { type: ElementRef },
    { type: Boolean, decorators: [{ type: Inject, args: [TOUCH_ENABLED,] }] },
    { type: LocalizationService },
    { type: DisabledDatesService },
    { type: Renderer2 }
];
DateTimePickerComponent.propDecorators = {
    hostClasses: [{ type: HostBinding, args: ['class.k-widget',] }, { type: HostBinding, args: ['class.k-datetimepicker',] }],
    wrapper: [{ type: ViewChild, args: ['wrapper',] }],
    value: [{ type: Input }],
    format: [{ type: Input }],
    tabindex: [{ type: Input }],
    disabledDates: [{ type: Input }],
    popupSettings: [{ type: Input }],
    focusableId: [{ type: Input }],
    title: [{ type: Input }],
    disabled: [{ type: Input }],
    readonly: [{ type: Input }],
    readOnlyInput: [{ type: Input }],
    cancelButton: [{ type: Input }],
    formatPlaceholder: [{ type: Input }],
    placeholder: [{ type: Input }],
    steps: [{ type: Input }],
    focusedDate: [{ type: Input }],
    weekNumber: [{ type: Input }],
    min: [{ type: Input }],
    max: [{ type: Input }],
    rangeValidation: [{ type: Input }],
    disabledDatesValidation: [{ type: Input }],
    incompleteDateValidation: [{ type: Input }],
    valueChange: [{ type: Output }],
    open: [{ type: Output }],
    close: [{ type: Output }],
    onFocus: [{ type: Output, args: ['focus',] }],
    onBlur: [{ type: Output, args: ['blur',] }],
    defaultTab: [{ type: Input }],
    cellTemplate: [{ type: ContentChild, args: [CellTemplateDirective,] }],
    monthCellTemplate: [{ type: ContentChild, args: [MonthCellTemplateDirective,] }],
    yearCellTemplate: [{ type: ContentChild, args: [YearCellTemplateDirective,] }],
    decadeCellTemplate: [{ type: ContentChild, args: [DecadeCellTemplateDirective,] }],
    centuryCellTemplate: [{ type: ContentChild, args: [CenturyCellTemplateDirective,] }],
    weekNumberTemplate: [{ type: ContentChild, args: [WeekNumberCellTemplateDirective,] }],
    headerTitleTemplate: [{ type: ContentChild, args: [HeaderTitleTemplateDirective,] }],
    container: [{ type: ViewChild, args: ['container', { read: ViewContainerRef },] }],
    popupTemplate: [{ type: ViewChild, args: ['popupTemplate', { read: TemplateRef },] }]
};
