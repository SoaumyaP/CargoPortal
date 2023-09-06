/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var utils_1 = require("../common/utils");
var math_1 = require("../common/math");
var core_1 = require("@angular/core");
/**
 * @hidden
 */
exports.calculateFixedTrackSize = function (_a) {
    var max = _a.max, min = _a.min, smallStep = _a.smallStep, fixedTickWidth = _a.fixedTickWidth;
    return ((max - min) / smallStep) * fixedTickWidth;
};
/**
 * @hidden
 */
exports.calculateTrackSize = function (wrapperWidth, offset, showButtons) {
    if (showButtons === void 0) { showButtons = true; }
    var BUTTONS_COUNT = 2;
    var trackOffset = showButtons ? parseFloat(offset) * BUTTONS_COUNT : 0;
    var trackWidth = wrapperWidth - trackOffset;
    return Math.floor(trackWidth);
};
/**
 * @hidden
 */
exports.calculateTicksCount = function (min, max, smallStep) {
    if (min === void 0) { min = 0; }
    if (max === void 0) { max = 0; }
    if (smallStep === void 0) { smallStep = 1; }
    if (smallStep <= 0) {
        throw new Error('Invalid argument: smallStep must be a positive number');
    }
    var adjustedRange = Math.abs(math_1.subtract(max, min));
    var adjustedRatio = Math.floor(math_1.divide(adjustedRange, smallStep));
    var result = math_1.add(adjustedRatio, 1);
    return result;
};
/**
 * @hidden
 */
exports.calculateValueFromTick = function (index, _a) {
    var max = _a.max, min = _a.min, smallStep = _a.smallStep, reverse = _a.reverse, vertical = _a.vertical;
    var value = math_1.add(min, math_1.multiply(index, smallStep));
    return vertical || reverse ? Math.abs(math_1.subtract(value, max)) : value;
};
/**
 * @hidden
 */
exports.calculateHandlePosition = function (_a) {
    var handleWidth = _a.handleWidth, trackWidth = _a.trackWidth, min = _a.min, max = _a.max, reverse = _a.reverse, value = _a.value;
    var halfHandleWidth = Math.floor(handleWidth / 2);
    var step = trackWidth / Math.abs(max - min);
    var pos = utils_1.isPresent(value) ? step * (value - min) : min;
    if (reverse) {
        pos = trackWidth - pos;
    }
    return Math.floor(pos - halfHandleWidth);
};
/**
 * @hidden
 */
exports.decreaseValueToStep = function (value, _a, large) {
    var max = _a.max, min = _a.min, smallStep = _a.smallStep, largeStep = _a.largeStep;
    if (large === void 0) { large = false; }
    var step = large && largeStep ? math_1.multiply(smallStep, largeStep) : smallStep;
    var stepValue = math_1.subtract(value, min);
    var result;
    var stepRemainder = math_1.remainder(stepValue, step);
    if (stepRemainder === 0) {
        result = math_1.subtract(stepValue, step);
    }
    else {
        result = math_1.subtract(stepValue, stepRemainder);
    }
    return exports.limitValue(math_1.add(result, min), min, max);
};
/**
 * @hidden
 */
exports.increaseValueToStep = function (value, _a, large) {
    var max = _a.max, min = _a.min, smallStep = _a.smallStep, largeStep = _a.largeStep;
    if (large === void 0) { large = false; }
    var step = large && largeStep ? math_1.multiply(smallStep, largeStep) : smallStep;
    var stepValue = math_1.subtract(value, min);
    var stepRemainder = math_1.remainder(stepValue, step);
    var result = math_1.add(math_1.subtract(stepValue, stepRemainder), step);
    return exports.limitValue(math_1.add(result, min), min, max);
};
/**
 * @hidden
 */
exports.isStartHandle = function (dragHandle) { return dragHandle.id.indexOf('k-start-handle') > -1; };
/**
 * @hidden
 */
exports.snapValue = function (value, options) {
    var smallStep = options.smallStep, min = options.min, max = options.max;
    var limited = exports.limitValue(value, min, max);
    if (value !== limited) {
        return limited;
    }
    var left = exports.decreaseValueToStep(value, options);
    var right = exports.increaseValueToStep(value, options);
    if ((value - min) % smallStep === 0) {
        return value;
    }
    if (right - value <= (right - left) / 2) {
        return right;
    }
    return left;
};
/**
 * @hidden
 */
exports.trimValue = function (max, min, value) {
    if (value > max) {
        return max;
    }
    if (value < min) {
        return min;
    }
    return value;
};
/**
 * @hidden
 */
exports.trimValueRange = function (max, min, value) {
    return value ? [exports.trimValue(max, min, value[0]), exports.trimValue(max, min, value[1])] : [min, min];
};
/**
 * @hidden
 */
exports.identity = function (value) { return value; };
/**
 * @hidden
 */
exports.isSameRange = function (value1, value2) {
    return utils_1.areSame(value1[0], value2[0]) && utils_1.areSame(value1[1], value2[1]);
};
/**
 * @hidden
 */
exports.elementOffset = function (element) {
    var box = element.getBoundingClientRect();
    var documentElement = document.documentElement;
    return {
        left: box.left + (window.pageXOffset || documentElement.scrollLeft) - (documentElement.clientLeft || 0),
        top: box.top + (window.pageYOffset || documentElement.scrollTop) - (documentElement.clientTop || 0)
    };
};
/**
 * @hidden
 */
exports.limitValue = function (value, min, max) {
    return Math.max(Math.min(value, max), min);
};
/**
 * @hidden
 */
exports.eventValue = function (eventArgs, scaleElement, options) {
    var min = options.min, max = options.max, vertical = options.vertical, rtl = options.rtl;
    var trackOffset = exports.elementOffset(scaleElement);
    var offset = vertical ? eventArgs.pageY - trackOffset.top : eventArgs.pageX - trackOffset.left;
    var scale = (max - min) / (vertical ? scaleElement.clientHeight : scaleElement.clientWidth);
    var offsetValue = offset * scale;
    var value = rtl || vertical ? max - offsetValue : min + offsetValue;
    var stepFractionLength = math_1.fractionLength(options.smallStep);
    value = math_1.toFixedPrecision(value, stepFractionLength + 1);
    return exports.snapValue(value, options);
};
/**
 * @hidden
 */
exports.isButton = function (element) {
    return element.className.indexOf('k-button-increase') >= 0 || element.className.indexOf('k-button-decrease') >= 0;
};
/**
 * @hidden
 */
exports.increment = function (options) {
    return exports.increaseValueToStep(options.value, options);
};
/**
 * @hidden
 */
exports.decrement = function (options) {
    return exports.decreaseValueToStep(options.value, options);
};
/**
 * @hidden
 */
exports.incrementLarge = function (options) {
    return exports.increaseValueToStep(options.value, options, true);
};
/**
 * @hidden
 */
exports.decrementLarge = function (options) {
    return exports.decreaseValueToStep(options.value, options, true);
};
/**
 * @hidden
 */
exports.validateValue = function (value) {
    if (core_1.isDevMode && value && value[0] > value[1]) {
        throw new Error('[RangeSlider] The start value should not be greater than the end value.');
    }
};
/**
 * @hidden
 */
exports.default = {
    calculateFixedTrackSize: exports.calculateFixedTrackSize,
    calculateValueFromTick: exports.calculateValueFromTick,
    calculateTrackSize: exports.calculateTrackSize,
    calculateTicksCount: exports.calculateTicksCount,
    calculateHandlePosition: exports.calculateHandlePosition,
    decreaseValueToStep: exports.decreaseValueToStep,
    decrement: exports.decrement,
    decrementLarge: exports.decrementLarge,
    eventValue: exports.eventValue,
    identity: exports.identity,
    increment: exports.increment,
    incrementLarge: exports.incrementLarge,
    isButton: exports.isButton,
    isSameRange: exports.isSameRange,
    isStartHandle: exports.isStartHandle,
    increaseValueToStep: exports.increaseValueToStep,
    trimValue: exports.trimValue,
    trimValueRange: exports.trimValueRange,
    snapValue: exports.snapValue,
    validateValue: exports.validateValue
};
