/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * Configures the color gradient that opens in the popup.
 */
export interface GradientSettings {
    /**
     * Specifies if the component will render an alpha slider.
     */
    opacity?: boolean;
    /**
     * Specifies whether the ColorGradient should display a 'Clear color' button.
     * Applicable only for ColorPicker with ColorGradient, when a pallete is not specified.
     */
    clearButton?: boolean;
}
