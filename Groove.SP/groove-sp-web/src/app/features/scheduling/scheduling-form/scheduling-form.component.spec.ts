import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SchedulingFormComponent } from './scheduling-form.component';

describe('SchedulingFormComponent', () => {
  let component: SchedulingFormComponent;
  let fixture: ComponentFixture<SchedulingFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SchedulingFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SchedulingFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
