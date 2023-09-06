/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { DoCheck, IterableDiffers, NgIterable, OnChanges, SimpleChanges, TemplateRef, TrackByFunction, ViewContainerRef } from '@angular/core';
/**
 * @hidden
 */
export declare class KForOfContext<T> {
    $implicit: T;
    kForOf: NgIterable<T>;
    index: number;
    count: number;
    constructor($implicit: T, kForOf: NgIterable<T>, index: number, count: number);
    readonly first: boolean;
    readonly last: boolean;
    readonly even: boolean;
    readonly odd: boolean;
}
/**
 * @hidden
 */
export declare class KForOf<T> implements DoCheck, OnChanges {
    private _viewContainer;
    private _template;
    private _differs;
    kForOf: NgIterable<T>;
    kForTrackBy: TrackByFunction<T>;
    private _differ;
    constructor(_viewContainer: ViewContainerRef, _template: TemplateRef<KForOfContext<T>>, _differs: IterableDiffers);
    kForTemplate: TemplateRef<KForOfContext<T>>;
    ngOnChanges(changes: SimpleChanges): void;
    ngDoCheck(): void;
    private _applyChanges;
}
/**
 * @hidden
 */
export declare function getTypeNameForDebugging(type: any): string;
