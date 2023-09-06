import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ReportPermissionFormComponent } from './report-permission-form.component';

describe('ReportPermissionFormComponent', () => {
  let component: ReportPermissionFormComponent;
  let fixture: ComponentFixture<ReportPermissionFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ReportPermissionFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReportPermissionFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
