import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderNoteListComponent } from './order-note-list.component';

describe('NoteListComponent', () => {
  let component: OrderNoteListComponent;
  let fixture: ComponentFixture<OrderNoteListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OrderNoteListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderNoteListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
