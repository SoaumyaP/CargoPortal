import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faChevronRight, faChevronLeft } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-pager',
  templateUrl: './pager.component.html',
  styleUrls: ['./pager.component.scss']
})
export class PagerComponent implements OnChanges {
  @Input() current: number = 0
  @Input() total: number = 0
  @Input() firstRecordIndex: number = 0
  @Input() lastRecordIndex: number = 0
  @Input() recordCount: number = 0

  @Output() goTo: EventEmitter<number> = new EventEmitter<number>()
  @Output() next: EventEmitter<number> = new EventEmitter<number>()
  @Output() previous: EventEmitter<number> = new EventEmitter<number>()

  public pages: number[] = []

  faChevronRight = faChevronRight;
  faChevronLeft = faChevronLeft;


  constructor() { }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      (changes.current && changes.current.currentValue) ||
      (changes.total && changes.total.currentValue)
    ) {
      this.pages = this.getPages(this.current, this.total)
    }
  }

  private getPages(current: number, total: number): number[] {
    if (total <= 7) {
      return [...Array(total).keys()].map(x => ++x)
    }

    if (current > 5) {
      if (current >= total - 4) {
        return [1, -1, total - 4, total - 3, total - 2, total - 1, total]
      } else {
        return [1, -1, current - 1, current, current + 1, -1, total]
      }
    }

    return [1, 2, 3, 4, 5, -1, total]
  }

  public onGoTo(pageIndex: number): void {
    let page = this.pages[pageIndex];
    let currentPageIndex = this.pages.findIndex(x => x === this.current);
    if (page === -1){
      if (pageIndex > currentPageIndex)
      {
        page = this.pages[pageIndex - 1] + 1;
      }
      else {
        page = this.pages[pageIndex + 1] - 1;
      }
    }
    this.goTo.emit(page)
  }
  public onNext(): void {
    this.next.emit(this.current)
  }
  public onPrevious(): void {
    this.previous.next(this.current)
  }

}
