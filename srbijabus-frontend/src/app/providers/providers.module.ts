import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AddProviderComponent } from './add-provider/add-provider.component';
import {ReactiveFormsModule} from "@angular/forms";
import {BaseModule} from "../base/base.module";



@NgModule({
  declarations: [
    AddProviderComponent
  ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        BaseModule
    ]
})
export class ProvidersModule { }
