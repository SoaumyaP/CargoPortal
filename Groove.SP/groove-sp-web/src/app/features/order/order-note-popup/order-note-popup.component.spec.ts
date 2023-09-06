import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderNotePopupComponent } from './order-note-popup.component';

describe('NotePopupComponent', () => {
  let component: OrderNotePopupComponent;
  let fixture: ComponentFixture<OrderNotePopupComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OrderNotePopupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderNotePopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
