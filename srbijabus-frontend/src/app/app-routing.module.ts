import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {LoginComponent} from "./auth/login/login.component";
import {RegisterComponent} from "./auth/register/register.component";
import {MainComponent} from "./base/main/main.component";
import { ScheduleComponent } from './schedule/schedule.component';
import { BuslinesComponent } from './buslines/buslines.component';
import { BusreservationComponent } from './busreservation/busreservation.component';
import {authGuard} from "./auth/guard/auth.guard";
import {AddProviderComponent} from "./providers/add-provider/add-provider.component";
import {UserProfileViewComponent} from "./profiles/user-profile-view/user-profile-view.component";
import {AdminDiscountReviewComponent} from "./profiles/admin-discount-review/admin-discount-review.component";
import {AdminApprovedDiscountsComponent} from "./profiles/admin-approved-discounts/admin-approved-discounts.component";
import {FavouriteRoutesComponent} from "./favourites/favourite-routes/favourite-routes.component";

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'main', component: MainComponent, canActivate: [authGuard]  },
  { path: 'schedules', component: ScheduleComponent, canActivate: [authGuard], data: { role: ['TransportProvider'] } },
  { path: 'buslines', component: BuslinesComponent },
  { path: 'busreservation/:id', component: BusreservationComponent, canActivate: [authGuard], data: { role: ['User'] } },
  { path: 'main', component: MainComponent, canActivate: [authGuard]  },
  { path: 'add-provider', component: AddProviderComponent, canActivate: [authGuard], data: { role: ['Admin'] } },
  { path: 'profile', component: UserProfileViewComponent, canActivate: [authGuard], data: { role: ['User'] } },
  { path: 'discount-requests', component: AdminDiscountReviewComponent, canActivate: [authGuard], data: { role: ['Admin'] } },
  { path: 'approved-discounts', component: AdminApprovedDiscountsComponent, canActivate: [authGuard], data: { role: ['Admin'] } },
  { path: 'favourite-routes', component: FavouriteRoutesComponent, canActivate: [authGuard], data: { role: ['User'] } },
  // ovo je uvek na kraju
  { path: '', redirectTo: '/buslines', pathMatch: 'full' },  // Redirect to login by default
  { path: '**', redirectTo: '/buslines', pathMatch: 'full' }, // Catch-all for undefined paths

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
