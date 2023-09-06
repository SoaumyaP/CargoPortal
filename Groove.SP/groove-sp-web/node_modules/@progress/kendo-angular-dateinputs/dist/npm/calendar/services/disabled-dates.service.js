/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var kendo_date_math_1 = require("@progress/kendo-date-math");
var rxjs_1 = require("rxjs");
var utils_1 = require("../../common/utils");
var noop = function () { return false; };
var ɵ0 = noop;
exports.ɵ0 = ɵ0;
var DISABLED_DATES_DOC_LINK = 'https://www.telerik.com/kendo-angular-ui/components/dateinputs/calendar/disabled-dates/';
/**
 * @hidden
 */
var DisabledDatesService = /** @class */ (function () {
    function DisabledDatesService() {
        /**
         * Emits every time the `isDateDisabled` method changes.
         */
        this.changes = new rxjs_1.Subject();
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
    DisabledDatesService.prototype.initialize = function (disabledDates) {
        if (typeof disabledDates === 'function') {
            this.isDateDisabled = function (date) { return disabledDates(kendo_date_math_1.getDate(date)); };
        }
        else if (utils_1.isNumberArray(disabledDates)) {
            var disabledWeekDays_1 = new Set(disabledDates);
            this.isDateDisabled = function (date) { return disabledWeekDays_1.has(date.getDay()); };
        }
        else if (utils_1.isDateArray(disabledDates)) {
            var normalizedDisabledDates_1 = new Set(disabledDates.map(function (date) { return kendo_date_math_1.getDate(date).getTime(); }));
            this.isDateDisabled = function (date) { return normalizedDisabledDates_1.has(kendo_date_math_1.getDate(date).getTime()); };
        }
        else {
            this.isDateDisabled = noop;
            this.notifyInvalidInput(disabledDates);
        }
        this.notifyServiceChange();
    };
    DisabledDatesService.prototype.notifyInvalidInput = function (disabledDates) {
        if (utils_1.isPresent(disabledDates) && core_1.isDevMode()) {
            throw new Error("The 'disabledDates' value should be a function, a Day array or a Date array. Check " + DISABLED_DATES_DOC_LINK + " for more information.");
        }
    };
    DisabledDatesService.prototype.notifyServiceChange = function () {
        this.changes.next();
    };
    DisabledDatesService.decorators = [
        { type: core_1.Injectable },
    ];
    return DisabledDatesService;
}());
exports.DisabledDatesService = DisabledDatesService;
