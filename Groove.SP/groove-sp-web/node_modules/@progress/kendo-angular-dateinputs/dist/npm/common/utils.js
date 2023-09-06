/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @hidden
 */
exports.requiresZoneOnBlur = function (ngControl) { return ngControl &&
    (!ngControl.touched || (ngControl.control && ngControl.control.updateOn === 'blur')); };
/**
 * @hidden
 */
exports.preventDefault = function (args) { return args.preventDefault(); };
/**
 * @hidden
 */
exports.currentFocusTarget = function (blurArgs) { return blurArgs.relatedTarget || document.activeElement; };
/**
 * @hidden
 */
exports.isPresent = function (value) { return value !== undefined && value !== null; };
/**
 * @hidden
 *
 * If the provided parameter is an array with at least one item
 * and all items in the array are numbers, returns `true.
 */
exports.isNumberArray = function (value) { return Array.isArray(value) && value.length > 0 && value.every(function (item) { return typeof item === 'number'; }); };
/**
 * @hidden
 *
 * If the provided parameter is an array with at least one item
 * and all items in the array are dates, returns `true`.
 */
exports.isDateArray = function (value) { return Array.isArray(value) && value.length > 0 && value.every(function (item) { return item instanceof Date; }); };
