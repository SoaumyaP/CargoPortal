/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var util_1 = require("../util");
var utils_1 = require("../common/utils");
var noop = function () { return null; };
var ɵ0 = noop;
exports.ɵ0 = ɵ0;
/**
 * @hidden
 */
exports.disabledDatesRangeValidator = function (isDateDisabled) {
    if (!utils_1.isPresent(isDateDisabled)) {
        return noop;
    }
    return function (selectedRange) {
        var isRangeComplete = utils_1.isPresent(selectedRange) && utils_1.isPresent(selectedRange.start) && utils_1.isPresent(selectedRange.end);
        if (!isRangeComplete || selectedRange.start > selectedRange.end) {
            return null;
        }
        var disabledDates = util_1.disabledDatesInRange(selectedRange.start, selectedRange.end, isDateDisabled);
        var error = {
            disabledDatesInRange: disabledDates
        };
        return disabledDates.length ? error : null;
    };
};
