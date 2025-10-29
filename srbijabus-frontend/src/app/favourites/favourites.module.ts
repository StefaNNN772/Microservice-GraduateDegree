import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FavouriteRoutesComponent } from './favourite-routes/favourite-routes.component';
import {BaseModule} from "../base/base.module";
import {MatFormFieldModule} from "@angular/material/form-field";
import {MatInputModule} from "@angular/material/input";
import {MatSortModule} from "@angular/material/sort";
import {MatTableModule} from "@angular/material/table";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {MatIconModule} from "@angular/material/icon";



@NgModule({
  declarations: [
    FavouriteRoutesComponent
  ],
  imports: [
    CommonModule,
    BaseModule,
    MatFormFieldModule,
    MatInputModule,
    MatSortModule,
    MatTableModule,
    ReactiveFormsModule,
    FormsModule,
    MatIconModule
  ]
})
export class FavouritesModule { }
