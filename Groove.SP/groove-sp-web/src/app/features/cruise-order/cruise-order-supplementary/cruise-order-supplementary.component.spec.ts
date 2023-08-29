import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { CruiseOrderSupplementaryComponent } from './cruise-order-supplementary.component';


describe('CruiseOrderSupplementaryComponent', () => {
  let component: CruiseOrderSupplementaryComponent;
  let fixture: ComponentFixture<CruiseOrderSupplementaryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CruiseOrderSupplementaryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CruiseOrderSupplementaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
