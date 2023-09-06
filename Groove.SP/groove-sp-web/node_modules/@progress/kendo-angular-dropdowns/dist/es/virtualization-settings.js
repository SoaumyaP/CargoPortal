/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import * as tslib_1 from "tslib";
/**
 * @hidden
 */
var DEFAULTS = {
    pageSize: 50,
    itemHeight: 28
};
/**
 * @hidden
 */
export var normalizeVirtualizationSettings = function (settings) {
    if (settings === true) {
        return DEFAULTS;
    }
    if (!settings) {
        return null;
    }
    return tslib_1.__assign({ pageSize: DEFAULTS.pageSize }, settings);
};
