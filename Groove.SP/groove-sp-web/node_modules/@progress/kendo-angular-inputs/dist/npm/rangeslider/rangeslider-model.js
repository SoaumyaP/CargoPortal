/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var sliders_util_1 = require("./../sliders-common/sliders-util");
var sliders_util_2 = require("../sliders-common/sliders-util");
var slider_model_base_1 = require("../sliders-common/slider-model.base");
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
        var value = sliders_util_1.isStartHandle(dragHandle) ? sliders_util_2.trimValueRange(max, min, this.props.value)[0]
            : sliders_util_2.trimValueRange(max, min, this.props.value)[1];
        if (sliders_util_1.isStartHandle(dragHandle)) {
            this.startHandlePosition = sliders_util_2.calculateHandlePosition({
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
            this.endHandlePosition = sliders_util_2.calculateHandlePosition({
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
}(slider_model_base_1.SliderModelBase));
exports.RangeSliderModel = RangeSliderModel;
