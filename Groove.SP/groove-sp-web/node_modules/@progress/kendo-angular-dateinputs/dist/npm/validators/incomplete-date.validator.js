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
exports.incompleteDateValidator = function () {
    return function (control, incomplete) {
        if (!utils_1.isPresent(control.value) && incomplete) {
            return { incompleteDate: true };
        }
        else {
            return null;
        }
    };
};
