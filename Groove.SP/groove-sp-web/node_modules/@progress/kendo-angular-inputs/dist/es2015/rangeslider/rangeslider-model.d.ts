/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { SliderModelBase } from '../sliders-common/slider-model.base';
/**
 * @hidden
 */
export declare class RangeSliderModel extends SliderModelBase {
    protected tickSizes: number[];
    protected startHandlePosition: number;
    protected endHandlePosition: number;
    positionHandle(dragHandle: HTMLElement): void;
    positionSelection(dragHandle: HTMLElement, selection: HTMLElement): void;
}
