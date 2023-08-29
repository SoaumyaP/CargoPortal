import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ArticleMasterListComponent } from './article-master-list.component';

describe('ArticleMasterListComponent', () => {
  let component: ArticleMasterListComponent;
  let fixture: ComponentFixture<ArticleMasterListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ArticleMasterListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ArticleMasterListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
