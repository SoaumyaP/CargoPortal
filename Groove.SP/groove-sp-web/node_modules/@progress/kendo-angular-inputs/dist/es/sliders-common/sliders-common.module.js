/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SliderTicksComponent } from './slider-ticks.component';
import { LabelTemplateDirective } from './label-template.directive';
import { DraggableModule, EventsModule, ResizeSensorModule } from '@progress/kendo-angular-common';
/**
 * @hidden
 */
var SlidersCommonModule = /** @class */ (function () {
    function SlidersCommonModule() {
    }
    SlidersCommonModule.decorators = [
        { type: NgModule, args: [{
                    declarations: [
                        SliderTicksComponent,
                        LabelTemplateDirective
                    ],
                    exports: [
                        LabelTemplateDirective,
                        SliderTicksComponent,
                        DraggableModule,
                        EventsModule,
                        ResizeSensorModule
                    ],
                    imports: [CommonModule, DraggableModule, EventsModule, ResizeSensorModule]
                },] },
    ];
    return SlidersCommonModule;
}());
export { SlidersCommonModule };
