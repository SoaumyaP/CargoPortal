/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { SelectionService } from './selection.service';
/**
 * @hidden
 */
export declare class SelectableDirective {
    index: number;
    height: number;
    multipleSelection: boolean;
    private selectionService;
    constructor(selectionService: SelectionService);
    readonly focusedClassName: boolean;
    readonly selectedClassName: boolean;
    onClick(event: any): void;
}
