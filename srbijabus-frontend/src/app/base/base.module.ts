import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MainComponent } from './main/main.component';
import { NavbarUserComponent } from './navbar-user/navbar-user.component';
import {MatIconModule} from "@angular/material/icon";
import {RouterLink, RouterLinkActive, RouterModule} from "@angular/router";
import {MatMenuModule} from "@angular/material/menu";
import {MatButtonModule} from "@angular/material/button";
import {MatToolbarModule} from "@angular/material/toolbar";
import { NavbarAdminComponent } from './navbar-admin/navbar-admin.component';
import { NavbarTransportProviderComponent } from './navbar-transport-provider/navbar-transport-provider.component';



@NgModule({
  declarations: [
    MainComponent,
    NavbarUserComponent,
    NavbarAdminComponent,
    NavbarTransportProviderComponent
  ],
  exports: [
    NavbarUserComponent,
    NavbarAdminComponent,
    NavbarTransportProviderComponent
  ],
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatMenuModule,
    MatIconModule,
    RouterLink,
    RouterLinkActive,
    RouterModule
  ]
})
export class BaseModule { }
