/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * @hidden
 *
 * Checks if the value is `null` or `undefined`. Falsy values like '', 0, false, NaN, etc. are regarded as present.
 */
export const isPresent = (value) => value !== null && value !== undefined;
/**
 * @hidden
 */
export const areSame = (value1, value2) => value1 === value2 || (value1 === null && value2 === undefined) || (value1 === undefined && value2 === null);
/**
 * @hidden
 */
export const requiresZoneOnBlur = (ngControl) => ngControl &&
    (!ngControl.touched || (ngControl.control && ngControl.control.updateOn === 'blur'));
/**
 * @hidden
 *
 * Fits the contender number into the specified bounds. If the number is NaN or null, the min is returned.
 *
 * @param contender Represents the number you want to fit into specified bounds.
 * @param min The inclusive lower bound number.
 * @param max The inclusive upper bound number.
 */
export const fitIntoBounds = (contender, min, max) => {
    if (!isPresent(contender) || isNaN(contender)) {
        return min;
    }
    return contender <= min ? min : contender >= max ? max : contender;
};
