import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserProfileViewComponent } from './user-profile-view/user-profile-view.component';
import {FormsModule} from "@angular/forms";
import {BaseModule} from "../base/base.module";
import { AdminDiscountReviewComponent } from './admin-discount-review/admin-discount-review.component';
import {MatIconModule} from "@angular/material/icon";
import { AdminApprovedDiscountsComponent } from './admin-approved-discounts/admin-approved-discounts.component';



@NgModule({
  declarations: [
    UserProfileViewComponent,
    AdminDiscountReviewComponent,
    AdminApprovedDiscountsComponent
  ],
    imports: [
        CommonModule,
        FormsModule,
        BaseModule,
        MatIconModule
    ]
})
export class ProfilesModule { }
