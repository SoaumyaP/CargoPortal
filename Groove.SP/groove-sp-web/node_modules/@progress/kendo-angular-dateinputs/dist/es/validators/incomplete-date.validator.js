/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { isPresent } from '../common/utils';
/**
 * @hidden
 */
export var incompleteDateValidator = function () {
    return function (control, incomplete) {
        if (!isPresent(control.value) && incomplete) {
            return { incompleteDate: true };
        }
        else {
            return null;
        }
    };
};
