import { AfterViewInit, Directive, ElementRef, Input, OnInit, Renderer2 } from '@angular/core';
import { StringHelper } from '../helpers';


@Directive({
    selector: '[breakLine]'
})
export class BreakLineDirective implements AfterViewInit {
    @Input() breakLine: boolean;

    /**
     * If breakLine input() is true, break a string into multiple lines based on comma or semi-colon chars
     */
    constructor(
        private renderer2: Renderer2,
        private elementRef: ElementRef
    ) {
    }

    ngAfterViewInit(): void {
        if (!this.breakLine) {
            this.renderer2.setStyle(this.elementRef.nativeElement, 'word-break', 'break-all');
        } else {
            if (!StringHelper.isNullOrEmpty(this.elementRef.nativeElement.innerText)) {
                let arrInnerText = this.elementRef.nativeElement.innerText.split(/[,;]/g);
                if (arrInnerText.length > 0) {
                    arrInnerText = arrInnerText.filter(c => !StringHelper.isNullOrWhiteSpace(c)).map(c => c.trim());
                    if (arrInnerText.length > 0) {
                        this.renderer2.setStyle(this.elementRef.nativeElement, 'white-space', 'pre-line');
                        const breakLineText = arrInnerText.join("\n");
                        this.elementRef.nativeElement.innerText = breakLineText;
                    }
                }
            }
        }
    }
}