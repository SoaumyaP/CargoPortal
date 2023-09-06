/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * @hidden
 *
 * Checks if the value is `null` or `undefined`. Falsy values like '', 0, false, NaN, etc. are regarded as present.
 */
export declare const isPresent: (value: any) => boolean;
/**
 * @hidden
 */
export declare const areSame: (value1: any, value2: any) => boolean;
/**
 * @hidden
 */
export declare const requiresZoneOnBlur: (ngControl: any) => any;
/**
 * @hidden
 *
 * Fits the contender number into the specified bounds. If the number is NaN or null, the min is returned.
 *
 * @param contender Represents the number you want to fit into specified bounds.
 * @param min The inclusive lower bound number.
 * @param max The inclusive upper bound number.
 */
export declare const fitIntoBounds: (contender: number, min: number, max: number) => number;
