import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import {HttpClientModule} from "@angular/common/http";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import { MatDialogModule } from "@angular/material/dialog";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {LoginComponent} from "./auth/login/login.component";
import {AuthModule} from "./auth/auth.module";
import { ScheduleComponent } from './schedule/schedule.component';
import {ProvidersModule} from "./providers/providers.module";
import {ProfilesModule} from "./profiles/profiles.module";
import {MatIconModule} from "@angular/material/icon";
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { BuslinesComponent } from './buslines/buslines.component';
import { BusreservationComponent } from './busreservation/busreservation.component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {BaseModule} from "./base/base.module";
import {MatMenuModule} from "@angular/material/menu";
import {FavouritesModule} from "./favourites/favourites.module";

@NgModule({
  declarations: [
    AppComponent,
    ScheduleComponent,
    BuslinesComponent,
    BusreservationComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    MatDialogModule,
    ProvidersModule,
    MatIconModule,
    BrowserAnimationsModule,
    ProfilesModule,
    ProvidersModule,
    BrowserModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,  
    FormsModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    BaseModule,
    MatMenuModule,
    FavouritesModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
