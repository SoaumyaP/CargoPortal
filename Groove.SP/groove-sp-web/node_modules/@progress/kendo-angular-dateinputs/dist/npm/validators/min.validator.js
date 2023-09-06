/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @hidden
 */
exports.minValidator = function (minValue) {
    return function (control) {
        var err = {
            minError: {
                minValue: minValue,
                value: control.value
            }
        };
        if (!minValue || !control.value) {
            return null;
        }
        return control.value < minValue ? err : null;
    };
};
