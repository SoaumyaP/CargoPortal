/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Day } from '@progress/kendo-date-math';
import { Subject } from 'rxjs';
/**
 * @hidden
 */
export declare class DisabledDatesService {
    /**
     * Emits every time the `isDateDisabled` method changes.
     */
    changes: Subject<void>;
    /**
     * Based on the user-defined `disabledDates` input evaluates if the date is disabled.
     * If not set, returns `false`.
     */
    isDateDisabled: (date: Date) => boolean;
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
    initialize(disabledDates: ((date: Date) => boolean) | Date[] | Day[]): void;
    private notifyInvalidInput;
    private notifyServiceChange;
}
