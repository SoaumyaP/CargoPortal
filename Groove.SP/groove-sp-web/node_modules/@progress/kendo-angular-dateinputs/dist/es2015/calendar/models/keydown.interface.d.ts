/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * @hidden
 */
export interface KeyDown {
    /**
     * @hidden
     */
    keyCode: number;
    /**
     * @hidden
     */
    ctrlKey?: boolean;
    /**
     * @hidden
     */
    metaKey?: boolean;
    /**
     * @hidden
     */
    preventDefault?: Function;
}
