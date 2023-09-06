/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { calculateTrackSize, calculateFixedTrackSize, calculateTicksCount } from '../sliders-common/sliders-util';
import { subtract } from '../common/math';
/**
 * @hidden
 */
export class SliderModelBase {
    constructor(props, wrapper, track, renderer) {
        this.props = props;
        this.wrapper = wrapper;
        this.track = track;
        this.renderer = renderer;
        this.props = props;
        this.wrapper = wrapper;
        this.track = track;
        this.tickSizes = this.getTickSizes();
    }
    resizeTrack() {
        const orientation = this.props.vertical ? 'height' : 'width';
        const trackWidth = this.trackWidth();
        this.track.style[orientation] = `${trackWidth}px`;
    }
    resizeTicks(ticksContainer, ticks) {
        const dimension = this.props.vertical ? "height" : "width";
        [...ticks].map((tick, index) => tick.style[dimension] = `${this.tickSizes[index]}px`);
        if (this.props.vertical) {
            this.adjustPadding(ticksContainer);
        }
    }
    resizeWrapper() {
        const dimension = this.props.vertical ? "height" : "width";
        const wrapperSize = this.elementSize(this.wrapper);
        const trackWidth = calculateTrackSize(wrapperSize, this.elementOffset(this.track));
        const fixedTrackWidth = calculateFixedTrackSize(this.props);
        const wrapperParentEl = this.wrapper.parentElement;
        if (trackWidth > fixedTrackWidth) {
            wrapperParentEl.style[dimension] = `${wrapperSize - (trackWidth - fixedTrackWidth)}px`;
        }
        else {
            wrapperParentEl.style[dimension] = `${wrapperSize + (fixedTrackWidth - trackWidth)}px`;
        }
    }
    trackWidth() {
        if (this.props.fixedTickWidth) {
            return calculateFixedTrackSize(this.props);
        }
        return calculateTrackSize(this.elementSize(this.wrapper), this.elementOffset(this.track), this.props.buttons);
    }
    getTickSizes() {
        const { min, max, smallStep } = this.props;
        const count = calculateTicksCount(min, max, smallStep);
        const trackSize = this.trackWidth();
        const distStep = trackSize / subtract(max, min);
        const result = [];
        let usedSpace = 0;
        let endPoint = 0;
        for (let i = 0; i < count; i++) {
            if (i === 0 || i === count - 1) {
                endPoint += (smallStep / 2) * distStep;
            }
            else {
                endPoint += smallStep * distStep;
            }
            // ensure that the sum of the tick sizes does not exceed the track width
            endPoint = +endPoint.toFixed(2) - 0.01;
            const size = Math.round(endPoint - usedSpace);
            result.push(size);
            usedSpace += size;
        }
        if (usedSpace >= trackSize) {
            result[result.length - 1] -= 1;
        }
        return result;
    }
    adjustPadding(ticksContainer) {
        const totalTickSize = this.tickSizes.reduce((prev, curr) => prev + curr, 0);
        const trackWidth = this.trackWidth();
        const reminder = trackWidth - totalTickSize;
        if (reminder !== 0) {
            const padding = reminder + this.elementOffset(this.track);
            ticksContainer.style.paddingTop = `${padding}px`;
        }
    }
    elementOffset(element) {
        const { vertical } = this.props;
        const style = getComputedStyle(element);
        return parseInt(vertical ? style.bottom : style.left, 10);
    }
    elementSize(element) {
        const { vertical } = this.props;
        return vertical ? element.clientHeight : element.clientWidth;
    }
}
