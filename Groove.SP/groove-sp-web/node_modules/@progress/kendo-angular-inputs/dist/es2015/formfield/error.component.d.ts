/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { MessageAlign } from './models/message-align';
/**
 * Represents an error message that will be shown underneath
 * a Kendo control or native HTML form-bound component after a validation.
 */
export declare class ErrorComponent {
    hostClass: boolean;
    /**
     * Specifies the alignment of the Error message.
     *
     * The possible values are:
     * * (Default) `start`
     * * `end`
     */
    align: MessageAlign;
    /**
     * @hidden
     */
    readonly id: string;
    roleAttribute: string;
    readonly startClass: boolean;
    readonly endClass: boolean;
    readonly idAttribute: string;
}
