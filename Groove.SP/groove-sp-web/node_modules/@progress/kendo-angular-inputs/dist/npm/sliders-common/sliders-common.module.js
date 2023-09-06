/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var common_1 = require("@angular/common");
var slider_ticks_component_1 = require("./slider-ticks.component");
var label_template_directive_1 = require("./label-template.directive");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
/**
 * @hidden
 */
var SlidersCommonModule = /** @class */ (function () {
    function SlidersCommonModule() {
    }
    SlidersCommonModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [
                        slider_ticks_component_1.SliderTicksComponent,
                        label_template_directive_1.LabelTemplateDirective
                    ],
                    exports: [
                        label_template_directive_1.LabelTemplateDirective,
                        slider_ticks_component_1.SliderTicksComponent,
                        kendo_angular_common_1.DraggableModule,
                        kendo_angular_common_1.EventsModule,
                        kendo_angular_common_1.ResizeSensorModule
                    ],
                    imports: [common_1.CommonModule, kendo_angular_common_1.DraggableModule, kendo_angular_common_1.EventsModule, kendo_angular_common_1.ResizeSensorModule]
                },] },
    ];
    return SlidersCommonModule;
}());
exports.SlidersCommonModule = SlidersCommonModule;
