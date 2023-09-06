/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @hidden
 *
 * Checks if the value is `null` or `undefined`. Falsy values like '', 0, false, NaN, etc. are regarded as present.
 */
exports.isPresent = function (value) { return value !== null && value !== undefined; };
/**
 * @hidden
 */
exports.areSame = function (value1, value2) {
    return value1 === value2 || (value1 === null && value2 === undefined) || (value1 === undefined && value2 === null);
};
/**
 * @hidden
 */
exports.requiresZoneOnBlur = function (ngControl) { return ngControl &&
    (!ngControl.touched || (ngControl.control && ngControl.control.updateOn === 'blur')); };
/**
 * @hidden
 *
 * Fits the contender number into the specified bounds. If the number is NaN or null, the min is returned.
 *
 * @param contender Represents the number you want to fit into specified bounds.
 * @param min The inclusive lower bound number.
 * @param max The inclusive upper bound number.
 */
exports.fitIntoBounds = function (contender, min, max) {
    if (!exports.isPresent(contender) || isNaN(contender)) {
        return min;
    }
    return contender <= min ? min : contender >= max ? max : contender;
};
