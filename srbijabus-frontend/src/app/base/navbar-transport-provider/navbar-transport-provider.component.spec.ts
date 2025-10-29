import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavbarTransportProviderComponent } from './navbar-transport-provider.component';

describe('NavbarTransportProviderComponent', () => {
  let component: NavbarTransportProviderComponent;
  let fixture: ComponentFixture<NavbarTransportProviderComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [NavbarTransportProviderComponent]
    });
    fixture = TestBed.createComponent(NavbarTransportProviderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
