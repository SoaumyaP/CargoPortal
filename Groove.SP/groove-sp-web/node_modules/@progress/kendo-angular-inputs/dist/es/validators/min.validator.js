/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { isPresent } from '../common/utils';
/**
 * @hidden
 */
export var createMinValidator = function (minValue) {
    return function (c) {
        if (!isPresent(minValue) || !isPresent(c.value) || c.value >= minValue) {
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
