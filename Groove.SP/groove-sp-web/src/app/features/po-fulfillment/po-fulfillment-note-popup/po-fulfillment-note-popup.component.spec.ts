import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PoFulfillmentNotePopupComponent } from './po-fulfillment-note-popup.component';

describe('PoFulfillmentNotePopupComponent', () => {
  let component: PoFulfillmentNotePopupComponent;
  let fixture: ComponentFixture<PoFulfillmentNotePopupComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PoFulfillmentNotePopupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PoFulfillmentNotePopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
