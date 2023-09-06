/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { POINT } from './constants';
/**
 * @hidden
 */
export var numericRegex = function (options) {
    var autoCorrect = options.autoCorrect, decimals = options.decimals, min = options.min;
    var separator = options.separator;
    if (separator === POINT) {
        separator = '\\' + separator;
    }
    var signPattern = autoCorrect && min !== null && min >= 0 ? '' : '-?';
    var numberPattern;
    if (decimals === 0) {
        numberPattern = '\\d*';
    }
    else {
        numberPattern = "(?:(?:\\d+(" + separator + "\\d*)?)|(?:" + separator + "\\d*))?";
    }
    return new RegExp("^" + signPattern + numberPattern + "$");
};
/**
 * @hidden
 */
export var decimalPart = function (value) {
    return value >= 0 ? Math.floor(value) : Math.ceil(value);
};
/**
 * @hidden
 */
export var noop = function (_) { }; // tslint:disable-line:no-empty
/**
 * @hidden
 */
export var defined = function (value) {
    return typeof value !== 'undefined';
};
/**
 * @hidden
 */
export var isNumber = function (value) {
    return !isNaN(value) && value !== null;
};
/**
 * @hidden
 */
export function pad(value, digits) {
    var count = digits - String(value).length;
    var result = value;
    if (count > 0) {
        var padString = new Array(count + 1).join("0");
        result = parseFloat(value + padString);
    }
    return result;
}
/**
 * @hidden
 */
export var getDeltaFromMouseWheel = function (e) {
    var delta = 0;
    if (e.wheelDelta) {
        delta = e.wheelDelta / 120;
        delta = delta > 0 ? Math.ceil(delta) : Math.floor(delta);
    }
    else if (e.detail) {
        delta = Math.round(-e.detail / 3);
    }
    return delta;
};
/**
 * @hidden
 */
export var getCaretPosition = function (element) { return element.selectionStart; };
/**
 * @hidden
 */
export var extractSignificantNumericChars = function (formattedString, separator) {
    var significantCharacters = separator + "0123456789-";
    return formattedString.split('').reduce(function (acc, curr) { return significantCharacters.includes(curr) ? ++acc : acc; }, 0);
};
