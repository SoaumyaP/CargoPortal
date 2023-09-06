/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var sliders_util_1 = require("../sliders-common/sliders-util");
var slider_model_base_1 = require("../sliders-common/slider-model.base");
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
        var value = sliders_util_1.trimValue(max, min, this.props.value);
        this.handlePosition = sliders_util_1.calculateHandlePosition({
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
}(slider_model_base_1.SliderModelBase));
exports.SliderModel = SliderModel;
