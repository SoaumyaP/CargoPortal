/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { isInTimeRange } from '../util';
/**
 * @hidden
 */
export var timeRangeValidator = function (min, max) {
    return function (control) {
        if (!min || !max || !control.value) {
            return null;
        }
        var err = {
            timeRangeError: {
                maxValue: max,
                minValue: min,
                value: control.value
            }
        };
        return isInTimeRange(control.value, min, max) ? null : err;
    };
};
