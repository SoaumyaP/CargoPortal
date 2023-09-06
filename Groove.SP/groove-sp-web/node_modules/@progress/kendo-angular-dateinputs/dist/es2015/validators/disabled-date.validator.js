/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * @hidden
 */
export const disabledDatesValidator = (isDateDisabled) => {
    return (control) => {
        if (!isDateDisabled || !control.value) {
            return null;
        }
        const error = {
            disabledDate: true
        };
        return isDateDisabled(control.value) ? error : null;
    };
};
