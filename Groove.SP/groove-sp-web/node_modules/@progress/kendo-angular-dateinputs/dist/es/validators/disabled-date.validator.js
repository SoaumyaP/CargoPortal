/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * @hidden
 */
export var disabledDatesValidator = function (isDateDisabled) {
    return function (control) {
        if (!isDateDisabled || !control.value) {
            return null;
        }
        var error = {
            disabledDate: true
        };
        return isDateDisabled(control.value) ? error : null;
    };
};
