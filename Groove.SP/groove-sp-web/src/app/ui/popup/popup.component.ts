import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-popup',
  templateUrl: './popup.component.html',
  styleUrls: ['./popup.component.scss']
})
export class PopupComponent implements OnInit {
  @Input()  titleHeader: string = '';

  @Output() closePopup: EventEmitter<any> = new EventEmitter<any>();

  constructor() { }

  ngOnInit() {

  }

  close() {
    this.closePopup.emit();
  }
}
