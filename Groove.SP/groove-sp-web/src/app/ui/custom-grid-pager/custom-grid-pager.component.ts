import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ListService } from 'src/app/core/list/list.service';

@Component({
    selector: 'app-custom-grid-pager',
    templateUrl: './custom-grid-pager.component.html',
    styleUrls: ['./custom-grid-pager.component.scss']
})
export class CustomGridPagerComponent implements OnInit {
    @Input() public service: ListService;

    @Input() public buttonCount = 9; // is accompanied with pagerType = 'numeric'.
    @Input() public info = true;
    @Input() public pageSizes: Array<number> = [20, 50, 100];
    @Input() public previousNext = true;
    @Input() public refresh = true;
    @Input() public alwayShowPager = false;
    @Input() public pagerType: 'numeric' | 'input' = 'numeric';

    @Output() pageSizeChanged: EventEmitter<any> = new EventEmitter();

    constructor() {
    }

    ngOnInit() {
    }

    public pageSizeChange(value: any): void {
        this.service.pageSizeChange(value);
        this.pageSizeChanged.emit(value);
    }
}