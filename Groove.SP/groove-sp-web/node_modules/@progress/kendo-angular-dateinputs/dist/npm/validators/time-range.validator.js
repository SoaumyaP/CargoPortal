/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var util_1 = require("../util");
/**
 * @hidden
 */
exports.timeRangeValidator = function (min, max) {
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
        return util_1.isInTimeRange(control.value, min, max) ? null : err;
    };
};
