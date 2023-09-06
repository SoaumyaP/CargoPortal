/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { isStartHandle } from './../sliders-common/sliders-util';
import { trimValueRange, calculateHandlePosition } from '../sliders-common/sliders-util';
import { SliderModelBase } from '../sliders-common/slider-model.base';
/**
 * @hidden
 */
export class RangeSliderModel extends SliderModelBase {
    positionHandle(dragHandle) {
        if (!dragHandle.id) {
            return;
        }
        const { max, min, reverse, vertical } = this.props;
        const position = vertical ? 'bottom' : 'left';
        const trackWidth = this.trackWidth();
        const value = isStartHandle(dragHandle) ? trimValueRange(max, min, this.props.value)[0]
            : trimValueRange(max, min, this.props.value)[1];
        if (isStartHandle(dragHandle)) {
            this.startHandlePosition = calculateHandlePosition({
                min,
                max,
                reverse,
                value,
                trackWidth,
                handleWidth: dragHandle.offsetWidth
            });
            this.renderer.setStyle(dragHandle, position, `${this.startHandlePosition}px`);
        }
        else {
            this.endHandlePosition = calculateHandlePosition({
                min,
                max,
                reverse,
                value,
                trackWidth,
                handleWidth: dragHandle.offsetWidth
            });
            this.renderer.setStyle(dragHandle, position, `${this.endHandlePosition}px`);
        }
    }
    positionSelection(dragHandle, selection) {
        const { reverse, vertical } = this.props;
        const dimension = vertical ? 'height' : 'width';
        const position = vertical ? 'bottom' : reverse ? 'right' : 'left';
        const handleWidth = Math.floor(dragHandle.offsetWidth / 2);
        const size = Math.abs(this.endHandlePosition - this.startHandlePosition);
        const currentSelectionPosition = vertical ? dragHandle.style.bottom : dragHandle.style.left;
        this.renderer.setStyle(selection, dimension, `${size}px`);
        this.renderer.setStyle(selection, position, reverse ? this.trackWidth() - parseFloat(currentSelectionPosition) - handleWidth + 'px'
            : parseFloat(currentSelectionPosition) + handleWidth + 'px');
    }
}
