import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BulkFulfillmentDuplicatedCompanyDialogComponent } from './bulk-fulfillment-duplicated-company-dialog.component';

describe('BulkFulfillmentDuplicatedCompanyDialogComponent', () => {
  let component: BulkFulfillmentDuplicatedCompanyDialogComponent;
  let fixture: ComponentFixture<BulkFulfillmentDuplicatedCompanyDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BulkFulfillmentDuplicatedCompanyDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BulkFulfillmentDuplicatedCompanyDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
