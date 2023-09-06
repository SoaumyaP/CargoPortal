/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { areSame, isPresent } from '../common/utils';
import { add, subtract, multiply, divide, remainder, fractionLength, toFixedPrecision } from '../common/math';
import { isDevMode } from '@angular/core';
/**
 * @hidden
 */
export var calculateFixedTrackSize = function (_a) {
    var max = _a.max, min = _a.min, smallStep = _a.smallStep, fixedTickWidth = _a.fixedTickWidth;
    return ((max - min) / smallStep) * fixedTickWidth;
};
/**
 * @hidden
 */
export var calculateTrackSize = function (wrapperWidth, offset, showButtons) {
    if (showButtons === void 0) { showButtons = true; }
    var BUTTONS_COUNT = 2;
    var trackOffset = showButtons ? parseFloat(offset) * BUTTONS_COUNT : 0;
    var trackWidth = wrapperWidth - trackOffset;
    return Math.floor(trackWidth);
};
/**
 * @hidden
 */
export var calculateTicksCount = function (min, max, smallStep) {
    if (min === void 0) { min = 0; }
    if (max === void 0) { max = 0; }
    if (smallStep === void 0) { smallStep = 1; }
    if (smallStep <= 0) {
        throw new Error('Invalid argument: smallStep must be a positive number');
    }
    var adjustedRange = Math.abs(subtract(max, min));
    var adjustedRatio = Math.floor(divide(adjustedRange, smallStep));
    var result = add(adjustedRatio, 1);
    return result;
};
/**
 * @hidden
 */
export var calculateValueFromTick = function (index, _a) {
    var max = _a.max, min = _a.min, smallStep = _a.smallStep, reverse = _a.reverse, vertical = _a.vertical;
    var value = add(min, multiply(index, smallStep));
    return vertical || reverse ? Math.abs(subtract(value, max)) : value;
};
/**
 * @hidden
 */
export var calculateHandlePosition = function (_a) {
    var handleWidth = _a.handleWidth, trackWidth = _a.trackWidth, min = _a.min, max = _a.max, reverse = _a.reverse, value = _a.value;
    var halfHandleWidth = Math.floor(handleWidth / 2);
    var step = trackWidth / Math.abs(max - min);
    var pos = isPresent(value) ? step * (value - min) : min;
    if (reverse) {
        pos = trackWidth - pos;
    }
    return Math.floor(pos - halfHandleWidth);
};
/**
 * @hidden
 */
export var decreaseValueToStep = function (value, _a, large) {
    var max = _a.max, min = _a.min, smallStep = _a.smallStep, largeStep = _a.largeStep;
    if (large === void 0) { large = false; }
    var step = large && largeStep ? multiply(smallStep, largeStep) : smallStep;
    var stepValue = subtract(value, min);
    var result;
    var stepRemainder = remainder(stepValue, step);
    if (stepRemainder === 0) {
        result = subtract(stepValue, step);
    }
    else {
        result = subtract(stepValue, stepRemainder);
    }
    return limitValue(add(result, min), min, max);
};
/**
 * @hidden
 */
export var increaseValueToStep = function (value, _a, large) {
    var max = _a.max, min = _a.min, smallStep = _a.smallStep, largeStep = _a.largeStep;
    if (large === void 0) { large = false; }
    var step = large && largeStep ? multiply(smallStep, largeStep) : smallStep;
    var stepValue = subtract(value, min);
    var stepRemainder = remainder(stepValue, step);
    var result = add(subtract(stepValue, stepRemainder), step);
    return limitValue(add(result, min), min, max);
};
/**
 * @hidden
 */
export var isStartHandle = function (dragHandle) { return dragHandle.id.indexOf('k-start-handle') > -1; };
/**
 * @hidden
 */
export var snapValue = function (value, options) {
    var smallStep = options.smallStep, min = options.min, max = options.max;
    var limited = limitValue(value, min, max);
    if (value !== limited) {
        return limited;
    }
    var left = decreaseValueToStep(value, options);
    var right = increaseValueToStep(value, options);
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
export var trimValue = function (max, min, value) {
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
export var trimValueRange = function (max, min, value) {
    return value ? [trimValue(max, min, value[0]), trimValue(max, min, value[1])] : [min, min];
};
/**
 * @hidden
 */
export var identity = function (value) { return value; };
/**
 * @hidden
 */
export var isSameRange = function (value1, value2) {
    return areSame(value1[0], value2[0]) && areSame(value1[1], value2[1]);
};
/**
 * @hidden
 */
export var elementOffset = function (element) {
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
export var limitValue = function (value, min, max) {
    return Math.max(Math.min(value, max), min);
};
/**
 * @hidden
 */
export var eventValue = function (eventArgs, scaleElement, options) {
    var min = options.min, max = options.max, vertical = options.vertical, rtl = options.rtl;
    var trackOffset = elementOffset(scaleElement);
    var offset = vertical ? eventArgs.pageY - trackOffset.top : eventArgs.pageX - trackOffset.left;
    var scale = (max - min) / (vertical ? scaleElement.clientHeight : scaleElement.clientWidth);
    var offsetValue = offset * scale;
    var value = rtl || vertical ? max - offsetValue : min + offsetValue;
    var stepFractionLength = fractionLength(options.smallStep);
    value = toFixedPrecision(value, stepFractionLength + 1);
    return snapValue(value, options);
};
/**
 * @hidden
 */
export var isButton = function (element) {
    return element.className.indexOf('k-button-increase') >= 0 || element.className.indexOf('k-button-decrease') >= 0;
};
/**
 * @hidden
 */
export var increment = function (options) {
    return increaseValueToStep(options.value, options);
};
/**
 * @hidden
 */
export var decrement = function (options) {
    return decreaseValueToStep(options.value, options);
};
/**
 * @hidden
 */
export var incrementLarge = function (options) {
    return increaseValueToStep(options.value, options, true);
};
/**
 * @hidden
 */
export var decrementLarge = function (options) {
    return decreaseValueToStep(options.value, options, true);
};
/**
 * @hidden
 */
export var validateValue = function (value) {
    if (isDevMode && value && value[0] > value[1]) {
        throw new Error('[RangeSlider] The start value should not be greater than the end value.');
    }
};
/**
 * @hidden
 */
export default {
    calculateFixedTrackSize: calculateFixedTrackSize,
    calculateValueFromTick: calculateValueFromTick,
    calculateTrackSize: calculateTrackSize,
    calculateTicksCount: calculateTicksCount,
    calculateHandlePosition: calculateHandlePosition,
    decreaseValueToStep: decreaseValueToStep,
    decrement: decrement,
    decrementLarge: decrementLarge,
    eventValue: eventValue,
    identity: identity,
    increment: increment,
    incrementLarge: incrementLarge,
    isButton: isButton,
    isSameRange: isSameRange,
    isStartHandle: isStartHandle,
    increaseValueToStep: increaseValueToStep,
    trimValue: trimValue,
    trimValueRange: trimValueRange,
    snapValue: snapValue,
    validateValue: validateValue
};
