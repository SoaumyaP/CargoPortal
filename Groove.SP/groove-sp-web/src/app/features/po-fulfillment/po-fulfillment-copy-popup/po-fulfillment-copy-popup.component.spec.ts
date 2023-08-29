import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { POFulfillmentCopyPopupComponent } from './po-fulfillment-copy-popup.component';

describe('POFulfillmentCopyPopupComponent', () => {
  let component: POFulfillmentCopyPopupComponent;
  let fixture: ComponentFixture<POFulfillmentCopyPopupComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ POFulfillmentCopyPopupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(POFulfillmentCopyPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
