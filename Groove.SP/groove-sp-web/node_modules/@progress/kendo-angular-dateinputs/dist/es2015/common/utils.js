/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * @hidden
 */
export const requiresZoneOnBlur = (ngControl) => ngControl &&
    (!ngControl.touched || (ngControl.control && ngControl.control.updateOn === 'blur'));
/**
 * @hidden
 */
export const preventDefault = (args) => args.preventDefault();
/**
 * @hidden
 */
export const currentFocusTarget = (blurArgs) => blurArgs.relatedTarget || document.activeElement;
/**
 * @hidden
 */
export const isPresent = (value) => value !== undefined && value !== null;
/**
 * @hidden
 *
 * If the provided parameter is an array with at least one item
 * and all items in the array are numbers, returns `true.
 */
export const isNumberArray = (value) => Array.isArray(value) && value.length > 0 && value.every(item => typeof item === 'number');
/**
 * @hidden
 *
 * If the provided parameter is an array with at least one item
 * and all items in the array are dates, returns `true`.
 */
export const isDateArray = (value) => Array.isArray(value) && value.length > 0 && value.every(item => item instanceof Date);
