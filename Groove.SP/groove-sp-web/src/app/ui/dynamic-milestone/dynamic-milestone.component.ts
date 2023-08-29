import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
export interface DynamicMileStoneModel {
  stage: number | string;
  title: string;
  class: string;
  active: boolean;
  current: boolean;
}
@Component({
  selector: 'app-dynamic-milestone',
  templateUrl: './dynamic-milestone.component.html',
  styleUrls: ['./dynamic-milestone.component.scss']
})
export class DynamicMilestoneComponent implements OnInit {
  @Input() data: DynamicMileStoneModel[] = [];
  @Input() currentStage: number | string;

  constructor() {}

  ngOnChanges(changes: SimpleChanges): void {
      if (changes.currentStage.currentValue) {
          this.updateState();
      }
  }

  private updateState(): void {
      for (const item of this.data) {
          item.active = item.stage <= this.currentStage;
          item.current = item.stage === this.currentStage;
      }
  }

  ngOnInit() {
  }

}
