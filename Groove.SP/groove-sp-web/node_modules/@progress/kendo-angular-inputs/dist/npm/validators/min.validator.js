/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var utils_1 = require("../common/utils");
/**
 * @hidden
 */
exports.createMinValidator = function (minValue) {
    return function (c) {
        if (!utils_1.isPresent(minValue) || !utils_1.isPresent(c.value) || c.value >= minValue) {
            return null;
        }
        return {
            minError: {
                minValue: minValue,
                value: c.value
            }
        };
    };
};
