/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * @hidden
 */
export var maxValidator = function (maxValue) {
    return function (control) {
        var err = {
            maxError: {
                maxValue: maxValue,
                value: control.value
            }
        };
        if (!maxValue || !control.value) {
            return null;
        }
        return control.value > maxValue ? err : null;
    };
};
