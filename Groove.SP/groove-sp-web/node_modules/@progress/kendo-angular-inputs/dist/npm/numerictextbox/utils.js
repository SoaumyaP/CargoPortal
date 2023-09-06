/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var constants_1 = require("./constants");
/**
 * @hidden
 */
exports.numericRegex = function (options) {
    var autoCorrect = options.autoCorrect, decimals = options.decimals, min = options.min;
    var separator = options.separator;
    if (separator === constants_1.POINT) {
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
exports.decimalPart = function (value) {
    return value >= 0 ? Math.floor(value) : Math.ceil(value);
};
/**
 * @hidden
 */
exports.noop = function (_) { }; // tslint:disable-line:no-empty
/**
 * @hidden
 */
exports.defined = function (value) {
    return typeof value !== 'undefined';
};
/**
 * @hidden
 */
exports.isNumber = function (value) {
    return !isNaN(value) && value !== null;
};
/**
 * @hidden
 */
function pad(value, digits) {
    var count = digits - String(value).length;
    var result = value;
    if (count > 0) {
        var padString = new Array(count + 1).join("0");
        result = parseFloat(value + padString);
    }
    return result;
}
exports.pad = pad;
/**
 * @hidden
 */
exports.getDeltaFromMouseWheel = function (e) {
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
exports.getCaretPosition = function (element) { return element.selectionStart; };
/**
 * @hidden
 */
exports.extractSignificantNumericChars = function (formattedString, separator) {
    var significantCharacters = separator + "0123456789-";
    return formattedString.split('').reduce(function (acc, curr) { return significantCharacters.includes(curr) ? ++acc : acc; }, 0);
};
