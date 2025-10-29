import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuslinesComponent } from './buslines.component';

describe('BuslinesComponent', () => {
  let component: BuslinesComponent;
  let fixture: ComponentFixture<BuslinesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [BuslinesComponent]
    });
    fixture = TestBed.createComponent(BuslinesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
