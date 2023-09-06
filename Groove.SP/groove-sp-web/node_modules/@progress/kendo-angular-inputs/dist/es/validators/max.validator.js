/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { isPresent } from '../common/utils';
/**
 * @hidden
 */
export var createMaxValidator = function (maxValue) {
    return function (c) {
        if (!isPresent(maxValue) || !isPresent(c.value) || c.value <= maxValue) {
            return null;
        }
        return {
            maxError: {
                maxValue: maxValue,
                value: c.value
            }
        };
    };
};
