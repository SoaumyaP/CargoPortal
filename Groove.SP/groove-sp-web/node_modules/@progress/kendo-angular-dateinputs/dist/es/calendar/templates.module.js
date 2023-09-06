/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CellTemplateDirective } from './templates/cell-template.directive';
import { MonthCellTemplateDirective } from './templates/month-cell-template.directive';
import { YearCellTemplateDirective } from './templates/year-cell-template.directive';
import { DecadeCellTemplateDirective } from './templates/decade-cell-template.directive';
import { CenturyCellTemplateDirective } from './templates/century-cell-template.directive';
import { WeekNumberCellTemplateDirective } from './templates/weeknumber-cell-template.directive';
import { HeaderTitleTemplateDirective } from './templates/header-title-template.directive';
import { NavigationItemTemplateDirective } from './templates/navigation-item-template.directive';
/**
 * @hidden
 *
 * The exported package module.
 *
 * The package exports:
 * - `CellTemplateDirective`&mdash;The month cell template directive.
 * - `MonthCellTemplateDirective`&mdash;The month cell template directive.
 * - `YearCellTemplateDirective`&mdash;The year cell template directive.
 * - `DecadeCellTemplateDirective`&mdash;The decade cell template directive.
 * - `CenturyCellTemplateDirective`&mdash;The century cell template directive.
 * - `WeekNumberCellTemplateDirective`&mdash;The month week number cell template directive.
 * - `HeaderTitleTemplateDirective`&mdash;The header title template directive.
 * - `NavigationItemTemplateDirective`&mdash;The navigation item template directive.
 */
var TemplatesModule = /** @class */ (function () {
    function TemplatesModule() {
    }
    TemplatesModule.decorators = [
        { type: NgModule, args: [{
                    declarations: [
                        CellTemplateDirective,
                        MonthCellTemplateDirective,
                        YearCellTemplateDirective,
                        DecadeCellTemplateDirective,
                        CenturyCellTemplateDirective,
                        WeekNumberCellTemplateDirective,
                        HeaderTitleTemplateDirective,
                        NavigationItemTemplateDirective
                    ],
                    exports: [
                        CellTemplateDirective,
                        MonthCellTemplateDirective,
                        YearCellTemplateDirective,
                        DecadeCellTemplateDirective,
                        CenturyCellTemplateDirective,
                        WeekNumberCellTemplateDirective,
                        HeaderTitleTemplateDirective,
                        NavigationItemTemplateDirective
                    ]
                },] },
    ];
    return TemplatesModule;
}());
export { TemplatesModule };
