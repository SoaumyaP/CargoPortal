import { ElementRef, TemplateRef, EventEmitter } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { Position } from '../models/position.type';
/**
 * @hidden
 */
export declare class TooltipContentComponent {
    private content;
    private localizationService;
    /**
     * @hidden
     */
    direction: string;
    close: EventEmitter<any>;
    readonly cssClasses: string;
    readonly className: boolean;
    readonly cssPosition: string;
    tooltipWidth: number;
    tooltipHeight: number;
    titleTemplate?: TemplateRef<any>;
    anchor: ElementRef;
    closable: boolean;
    templateRef?: TemplateRef<any>;
    templateString?: string;
    callout: boolean;
    position: string;
    private dynamicRTLSubscription;
    constructor(content: ElementRef, localizationService: LocalizationService);
    ngOnInit(): void;
    ngOnDestroy(): void;
    calloutPositionClass(): string;
    onCloseClick(event: any): void;
    updateCalloutPosition(position: Position, isFlip: boolean): void;
    private calloutStyles;
}
