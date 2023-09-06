import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ArticleMasterFormComponent } from './article-master-form.component';

describe('ArticleMasterFormComponent', () => {
  let component: ArticleMasterFormComponent;
  let fixture: ComponentFixture<ArticleMasterFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ArticleMasterFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ArticleMasterFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
