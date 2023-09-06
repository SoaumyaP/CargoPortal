/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { calculateTrackSize, calculateFixedTrackSize, calculateTicksCount } from '../sliders-common/sliders-util';
import { subtract } from '../common/math';
/**
 * @hidden
 */
var SliderModelBase = /** @class */ (function () {
    function SliderModelBase(props, wrapper, track, renderer) {
        this.props = props;
        this.wrapper = wrapper;
        this.track = track;
        this.renderer = renderer;
        this.props = props;
        this.wrapper = wrapper;
        this.track = track;
        this.tickSizes = this.getTickSizes();
    }
    SliderModelBase.prototype.resizeTrack = function () {
        var orientation = this.props.vertical ? 'height' : 'width';
        var trackWidth = this.trackWidth();
        this.track.style[orientation] = trackWidth + "px";
    };
    SliderModelBase.prototype.resizeTicks = function (ticksContainer, ticks) {
        var _this = this;
        var dimension = this.props.vertical ? "height" : "width";
        ticks.slice().map(function (tick, index) { return tick.style[dimension] = _this.tickSizes[index] + "px"; });
        if (this.props.vertical) {
            this.adjustPadding(ticksContainer);
        }
    };
    SliderModelBase.prototype.resizeWrapper = function () {
        var dimension = this.props.vertical ? "height" : "width";
        var wrapperSize = this.elementSize(this.wrapper);
        var trackWidth = calculateTrackSize(wrapperSize, this.elementOffset(this.track));
        var fixedTrackWidth = calculateFixedTrackSize(this.props);
        var wrapperParentEl = this.wrapper.parentElement;
        if (trackWidth > fixedTrackWidth) {
            wrapperParentEl.style[dimension] = wrapperSize - (trackWidth - fixedTrackWidth) + "px";
        }
        else {
            wrapperParentEl.style[dimension] = wrapperSize + (fixedTrackWidth - trackWidth) + "px";
        }
    };
    SliderModelBase.prototype.trackWidth = function () {
        if (this.props.fixedTickWidth) {
            return calculateFixedTrackSize(this.props);
        }
        return calculateTrackSize(this.elementSize(this.wrapper), this.elementOffset(this.track), this.props.buttons);
    };
    SliderModelBase.prototype.getTickSizes = function () {
        var _a = this.props, min = _a.min, max = _a.max, smallStep = _a.smallStep;
        var count = calculateTicksCount(min, max, smallStep);
        var trackSize = this.trackWidth();
        var distStep = trackSize / subtract(max, min);
        var result = [];
        var usedSpace = 0;
        var endPoint = 0;
        for (var i = 0; i < count; i++) {
            if (i === 0 || i === count - 1) {
                endPoint += (smallStep / 2) * distStep;
            }
            else {
                endPoint += smallStep * distStep;
            }
            // ensure that the sum of the tick sizes does not exceed the track width
            endPoint = +endPoint.toFixed(2) - 0.01;
            var size = Math.round(endPoint - usedSpace);
            result.push(size);
            usedSpace += size;
        }
        if (usedSpace >= trackSize) {
            result[result.length - 1] -= 1;
        }
        return result;
    };
    SliderModelBase.prototype.adjustPadding = function (ticksContainer) {
        var totalTickSize = this.tickSizes.reduce(function (prev, curr) { return prev + curr; }, 0);
        var trackWidth = this.trackWidth();
        var reminder = trackWidth - totalTickSize;
        if (reminder !== 0) {
            var padding = reminder + this.elementOffset(this.track);
            ticksContainer.style.paddingTop = padding + "px";
        }
    };
    SliderModelBase.prototype.elementOffset = function (element) {
        var vertical = this.props.vertical;
        var style = getComputedStyle(element);
        return parseInt(vertical ? style.bottom : style.left, 10);
    };
    SliderModelBase.prototype.elementSize = function (element) {
        var vertical = this.props.vertical;
        return vertical ? element.clientHeight : element.clientWidth;
    };
    return SliderModelBase;
}());
export { SliderModelBase };
