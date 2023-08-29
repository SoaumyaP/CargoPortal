import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MasterEventListComponent } from './master-event-list.component';

describe('MasterEventListComponent', () => {
  let component: MasterEventListComponent;
  let fixture: ComponentFixture<MasterEventListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MasterEventListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MasterEventListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
