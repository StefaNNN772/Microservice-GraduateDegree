import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminDiscountReviewComponent } from './admin-discount-review.component';

describe('AdminDiscountReviewComponent', () => {
  let component: AdminDiscountReviewComponent;
  let fixture: ComponentFixture<AdminDiscountReviewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdminDiscountReviewComponent]
    });
    fixture = TestBed.createComponent(AdminDiscountReviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
