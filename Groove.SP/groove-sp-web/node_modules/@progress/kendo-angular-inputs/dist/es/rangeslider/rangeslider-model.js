/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { isStartHandle } from './../sliders-common/sliders-util';
import { trimValueRange, calculateHandlePosition } from '../sliders-common/sliders-util';
import { SliderModelBase } from '../sliders-common/slider-model.base';
/**
 * @hidden
 */
var RangeSliderModel = /** @class */ (function (_super) {
    tslib_1.__extends(RangeSliderModel, _super);
    function RangeSliderModel() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    RangeSliderModel.prototype.positionHandle = function (dragHandle) {
        if (!dragHandle.id) {
            return;
        }
        var _a = this.props, max = _a.max, min = _a.min, reverse = _a.reverse, vertical = _a.vertical;
        var position = vertical ? 'bottom' : 'left';
        var trackWidth = this.trackWidth();
        var value = isStartHandle(dragHandle) ? trimValueRange(max, min, this.props.value)[0]
            : trimValueRange(max, min, this.props.value)[1];
        if (isStartHandle(dragHandle)) {
            this.startHandlePosition = calculateHandlePosition({
                min: min,
                max: max,
                reverse: reverse,
                value: value,
                trackWidth: trackWidth,
                handleWidth: dragHandle.offsetWidth
            });
            this.renderer.setStyle(dragHandle, position, this.startHandlePosition + "px");
        }
        else {
            this.endHandlePosition = calculateHandlePosition({
                min: min,
                max: max,
                reverse: reverse,
                value: value,
                trackWidth: trackWidth,
                handleWidth: dragHandle.offsetWidth
            });
            this.renderer.setStyle(dragHandle, position, this.endHandlePosition + "px");
        }
    };
    RangeSliderModel.prototype.positionSelection = function (dragHandle, selection) {
        var _a = this.props, reverse = _a.reverse, vertical = _a.vertical;
        var dimension = vertical ? 'height' : 'width';
        var position = vertical ? 'bottom' : reverse ? 'right' : 'left';
        var handleWidth = Math.floor(dragHandle.offsetWidth / 2);
        var size = Math.abs(this.endHandlePosition - this.startHandlePosition);
        var currentSelectionPosition = vertical ? dragHandle.style.bottom : dragHandle.style.left;
        this.renderer.setStyle(selection, dimension, size + "px");
        this.renderer.setStyle(selection, position, reverse ? this.trackWidth() - parseFloat(currentSelectionPosition) - handleWidth + 'px'
            : parseFloat(currentSelectionPosition) + handleWidth + 'px');
    };
    return RangeSliderModel;
}(SliderModelBase));
export { RangeSliderModel };
