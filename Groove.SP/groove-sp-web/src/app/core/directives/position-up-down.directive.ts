import {
    Directive,
    ElementRef,
    HostListener,
    Input,
    Output,
    EventEmitter,
    OnInit,
    Renderer2,
    OnChanges,
    SimpleChanges,
    OnDestroy,
} from '@angular/core';
import { StringHelper } from '../helpers/string.helper';

@Directive({
    selector: '[appPositionUpDown]',
})
export class PositionUpDownDirective implements OnInit, OnChanges, OnDestroy {
    constructor(private _el: ElementRef, private _renderer: Renderer2) {}

    // tslint:disable-next-line:no-input-rename
    /**Do not show if */
    @Input('hideIf') isHidden: boolean = false;
    /**Current index of element, start from 0 */
    @Input() currentIndex: number = 0;
    /**Maximum index of the list/array, start from 0 */
    @Input() maximumIndex: number = 0;
    /**Array object value which current element belonging to */
    @Input() arrayObject: Array<any> = [];
    /**Call back as position changed */
    @Output() positionChanged: EventEmitter<PositionUpDownModel> = new EventEmitter<PositionUpDownModel>();

    ngOnInit() {
    }

    ngOnChanges() {
        if (!this.isHidden) {
            this._updateLayout();
        }
    }

    ngOnDestroy() {
    }

    private _updateLayout() {
        const removeNodes = this._el.nativeElement.getElementsByClassName('position-up-down');
        if (!StringHelper.isNullOrEmpty(removeNodes) && removeNodes.length > 0) {
            for (const node of removeNodes) {
                this._el.nativeElement.removeChild(node);
            }
        }

        if (this.currentIndex === this.maximumIndex && this.maximumIndex === 0) {
        } else {
            const child = document.createElement('div');
            let childInner = '';
            child.setAttribute('class', 'position-up-down');
            if (this.currentIndex > 0) {
                childInner += '<span class="navigation up ei ei-arrow_triangle-up"></span>';
            }

            if (this.currentIndex < this.maximumIndex) {
                childInner += '<span class="navigation down ei ei-arrow_triangle-down"></span>';
            }
            child.innerHTML = childInner;
            this._renderer.appendChild(this._el.nativeElement, child);
            this._renderer.listen(child, 'click', (e: MouseEvent) => {
                if ((e.target as Element).classList.contains('up')) {
                    const emitData = new PositionUpDownModel(PositionUpDownEnum.Up, this.currentIndex, this.arrayObject);
                    this.positionChanged.emit(emitData);
                } else if ((e.target as Element).classList.contains('down')) {
                    const emitData = new PositionUpDownModel(PositionUpDownEnum.Down, this.currentIndex, this.arrayObject);
                    this.positionChanged.emit(emitData);
                }
            });
        }
    }
}

export class PositionUpDownModel {

    /**Type of position changing: up or down */
    public type: PositionUpDownEnum;
    /**Current value */
    public currentIndex: number;
    /**New value */
    public newIndex: number;
    /**Array object */
    public arrayObject: Array<any>;

    public constructor(type: PositionUpDownEnum, currentIndex: number, arrayObject: Array<any>) {
        this.type = type;
        this.currentIndex = currentIndex;
        this.arrayObject = arrayObject;
        if (type === PositionUpDownEnum.Up) {
            this.newIndex = this.currentIndex - 1;
        } else {
            this.newIndex = this.currentIndex + 1;
        }
    }
}

export enum PositionUpDownEnum {
    /**Move backward -> index-- */
    Up = -1,
    /**Move forward -> index++ */
    Down = +1
}
