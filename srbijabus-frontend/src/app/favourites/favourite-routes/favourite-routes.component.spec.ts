import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FavouriteRoutesComponent } from './favourite-routes.component';

describe('FavouriteRoutesComponent', () => {
  let component: FavouriteRoutesComponent;
  let fixture: ComponentFixture<FavouriteRoutesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [FavouriteRoutesComponent]
    });
    fixture = TestBed.createComponent(FavouriteRoutesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
