/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { trimValue, calculateHandlePosition } from '../sliders-common/sliders-util';
import { SliderModelBase } from '../sliders-common/slider-model.base';
/**
 * @hidden
 */
export class SliderModel extends SliderModelBase {
    positionHandle(dragHandle) {
        const { max, min, reverse, vertical } = this.props;
        const position = vertical ? 'bottom' : 'left';
        const trackWidth = this.trackWidth();
        const value = trimValue(max, min, this.props.value);
        this.handlePosition = calculateHandlePosition({
            min,
            max,
            reverse,
            value,
            trackWidth,
            handleWidth: dragHandle.offsetWidth
        });
        this.renderer.setStyle(dragHandle, position, `${this.handlePosition}px`);
    }
    positionSelection(dragHandle, selection) {
        const { reverse, vertical } = this.props;
        const dimension = vertical ? 'height' : 'width';
        const handleWidth = Math.floor(dragHandle.offsetWidth / 2);
        let size = this.handlePosition + handleWidth;
        if (reverse) {
            size = this.trackWidth() - size;
        }
        this.renderer.setStyle(selection, dimension, `${size}px`);
    }
}
