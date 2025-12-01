import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BusreservationComponent } from './busreservation.component';

describe('BusreservationComponent', () => {
  let component: BusreservationComponent;
  let fixture: ComponentFixture<BusreservationComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [BusreservationComponent]
    });
    fixture = TestBed.createComponent(BusreservationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
