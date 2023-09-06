/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ElementRef, OnChanges, QueryList, TemplateRef } from '@angular/core';
/**
 * @hidden
 */
export declare class SliderTicksComponent implements OnChanges {
    private rtl;
    wrapperClasses: string;
    tickTitle: (value: number) => string;
    vertical: boolean;
    step: number;
    largeStep: number;
    min: number;
    max: number;
    labelTemplate: TemplateRef<any>;
    tickElements: QueryList<ElementRef>;
    ticks: Array<any>;
    constructor(rtl: boolean);
    ngOnChanges(_: any): void;
    private createTicks;
    private endTickClasses;
}
