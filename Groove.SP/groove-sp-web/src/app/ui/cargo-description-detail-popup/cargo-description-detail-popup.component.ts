import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-cargo-description-detail-popup',
  templateUrl: './cargo-description-detail-popup.component.html',
  styleUrls: ['./cargo-description-detail-popup.component.scss']
})
export class CargoDescriptionDetailPopupComponent implements OnInit {
  @Output() close: EventEmitter<any> = new EventEmitter<any>();
  @Input() cargoDescription: string;
  
  constructor() { }

  ngOnInit() {
  }

  closePopup() {
    this.close.emit();
  }

}
