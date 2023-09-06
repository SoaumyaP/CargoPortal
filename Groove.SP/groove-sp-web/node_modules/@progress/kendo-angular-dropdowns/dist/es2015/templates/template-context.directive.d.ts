/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ViewContainerRef } from '@angular/core';
/**
 * @hidden
 */
export declare class TemplateContextDirective {
    private insertedViewRef;
    private viewContainerRef;
    constructor(viewContainerRef: ViewContainerRef);
    templateContext: any;
}
