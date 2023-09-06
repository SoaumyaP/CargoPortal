/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
import { trimValue, calculateHandlePosition } from '../sliders-common/sliders-util';
import { SliderModelBase } from '../sliders-common/slider-model.base';
/**
 * @hidden
 */
var SliderModel = /** @class */ (function (_super) {
    tslib_1.__extends(SliderModel, _super);
    function SliderModel() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    SliderModel.prototype.positionHandle = function (dragHandle) {
        var _a = this.props, max = _a.max, min = _a.min, reverse = _a.reverse, vertical = _a.vertical;
        var position = vertical ? 'bottom' : 'left';
        var trackWidth = this.trackWidth();
        var value = trimValue(max, min, this.props.value);
        this.handlePosition = calculateHandlePosition({
            min: min,
            max: max,
            reverse: reverse,
            value: value,
            trackWidth: trackWidth,
            handleWidth: dragHandle.offsetWidth
        });
        this.renderer.setStyle(dragHandle, position, this.handlePosition + "px");
    };
    SliderModel.prototype.positionSelection = function (dragHandle, selection) {
        var _a = this.props, reverse = _a.reverse, vertical = _a.vertical;
        var dimension = vertical ? 'height' : 'width';
        var handleWidth = Math.floor(dragHandle.offsetWidth / 2);
        var size = this.handlePosition + handleWidth;
        if (reverse) {
            size = this.trackWidth() - size;
        }
        this.renderer.setStyle(selection, dimension, size + "px");
    };
    return SliderModel;
}(SliderModelBase));
export { SliderModel };
