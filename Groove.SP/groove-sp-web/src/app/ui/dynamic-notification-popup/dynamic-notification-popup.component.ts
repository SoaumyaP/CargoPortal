import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { StringHelper } from 'src/app/core';

@Component({
  selector: 'app-dynamic-notification-popup',
  templateUrl: './dynamic-notification-popup.component.html',
  styleUrls: ['./dynamic-notification-popup.component.scss']
})
export class DynamicNotificationPopupComponent implements OnInit {

    @Input()
    title: string = '';

    @Input()
    headline: string = '';

    @Input()
    content: Array<string> = [];

    @Output()
    popupClosing: EventEmitter<void> = new EventEmitter<void>();


    StringHelper = StringHelper;

    constructor() { }

    ngOnInit() {
    }

    closePopup() {
      this.popupClosing.next();
    }

}
