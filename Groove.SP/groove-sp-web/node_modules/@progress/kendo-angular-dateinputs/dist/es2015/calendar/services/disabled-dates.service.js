/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Injectable, isDevMode } from '@angular/core';
import { getDate } from '@progress/kendo-date-math';
import { Subject } from 'rxjs';
import { isNumberArray, isDateArray, isPresent } from '../../common/utils';
const noop = () => false;
const ɵ0 = noop;
const DISABLED_DATES_DOC_LINK = 'https://www.telerik.com/kendo-angular-ui/components/dateinputs/calendar/disabled-dates/';
/**
 * @hidden
 */
export class DisabledDatesService {
    constructor() {
        /**
         * Emits every time the `isDateDisabled` method changes.
         */
        this.changes = new Subject();
        /**
         * Based on the user-defined `disabledDates` input evaluates if the date is disabled.
         * If not set, returns `false`.
         */
        this.isDateDisabled = noop;
    }
    /**
     * Configures the `isDateDisabled` function.
     *
     * * If a function is provided, uses it as-is and passes each date to it for evaluation.
     * The time part is set to `midnight`.
     * * If a `Date[]` is provided, creates a function that checks the targeted date against
     * the listed dates and, if the targeted date is listed, marks it as disabled.
     * * If a `Day[]` is provided, creates a function that evaluates the provided days of the
     * week as disabled.
     */
    initialize(disabledDates) {
        if (typeof disabledDates === 'function') {
            this.isDateDisabled = (date) => disabledDates(getDate(date));
        }
        else if (isNumberArray(disabledDates)) {
            const disabledWeekDays = new Set(disabledDates);
            this.isDateDisabled = (date) => disabledWeekDays.has(date.getDay());
        }
        else if (isDateArray(disabledDates)) {
            const normalizedDisabledDates = new Set(disabledDates.map(date => getDate(date).getTime()));
            this.isDateDisabled = (date) => normalizedDisabledDates.has(getDate(date).getTime());
        }
        else {
            this.isDateDisabled = noop;
            this.notifyInvalidInput(disabledDates);
        }
        this.notifyServiceChange();
    }
    notifyInvalidInput(disabledDates) {
        if (isPresent(disabledDates) && isDevMode()) {
            throw new Error(`The 'disabledDates' value should be a function, a Day array or a Date array. Check ${DISABLED_DATES_DOC_LINK} for more information.`);
        }
    }
    notifyServiceChange() {
        this.changes.next();
    }
}
DisabledDatesService.decorators = [
    { type: Injectable },
];
export { ɵ0 };
