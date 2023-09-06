/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { MessageAlign } from './models/message-align';
/**
 * Represents a hint message that will be shown underneath a form-bound component.
 */
export declare class HintComponent {
    /**
     * Specifies the alignment of the Hint message.
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
    hostClass: boolean;
    readonly startClass: boolean;
    readonly endClass: boolean;
    readonly idAttribute: string;
}
