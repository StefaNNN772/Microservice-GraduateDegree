import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminApprovedDiscountsComponent } from './admin-approved-discounts.component';

describe('AdminApprovedDiscountsComponent', () => {
  let component: AdminApprovedDiscountsComponent;
  let fixture: ComponentFixture<AdminApprovedDiscountsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdminApprovedDiscountsComponent]
    });
    fixture = TestBed.createComponent(AdminApprovedDiscountsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
