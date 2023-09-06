import { ElementRef } from '@angular/core';

export interface BulkFulfillmentTabModel {
    /**
     * Text displayed on sticky bar
     */
    text: string;

    /**
     * Indicate if current tab is on focus
     */
    selected: boolean;

    /**
     * Indicate if current tab is view only
     */
    readonly: boolean;

    /**
     * Section element id which links to tab
     */
    sectionId: string;

    /**
     * Section element which links to tab
     */
    sectionElementRef?: ElementRef;
}
