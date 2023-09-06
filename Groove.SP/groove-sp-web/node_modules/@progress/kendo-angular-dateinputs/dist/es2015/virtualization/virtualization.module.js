/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VirtualizationComponent } from '../virtualization/virtualization.component';
const COMPONENT_DIRECTIVES = [
    VirtualizationComponent
];
/**
 * @hidden
 *
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the Virtualization component.
 */
export class VirtualizationModule {
}
VirtualizationModule.decorators = [
    { type: NgModule, args: [{
                declarations: [COMPONENT_DIRECTIVES],
                exports: [COMPONENT_DIRECTIVES],
                imports: [CommonModule]
            },] },
];
