import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MasterEventDialogComponent } from './master-event-dialog.component';

describe('MasterEventDialogComponent', () => {
  let component: MasterEventDialogComponent;
  let fixture: ComponentFixture<MasterEventDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MasterEventDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MasterEventDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
