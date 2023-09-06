/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { disabledDatesInRange } from '../util';
import { isPresent } from '../common/utils';
var noop = function () { return null; };
var ɵ0 = noop;
/**
 * @hidden
 */
export var disabledDatesRangeValidator = function (isDateDisabled) {
    if (!isPresent(isDateDisabled)) {
        return noop;
    }
    return function (selectedRange) {
        var isRangeComplete = isPresent(selectedRange) && isPresent(selectedRange.start) && isPresent(selectedRange.end);
        if (!isRangeComplete || selectedRange.start > selectedRange.end) {
            return null;
        }
        var disabledDates = disabledDatesInRange(selectedRange.start, selectedRange.end, isDateDisabled);
        var error = {
            disabledDatesInRange: disabledDates
        };
        return disabledDates.length ? error : null;
    };
};
export { ɵ0 };
