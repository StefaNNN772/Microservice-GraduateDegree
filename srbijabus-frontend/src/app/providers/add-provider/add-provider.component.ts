import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {ProviderService} from "../../services/provider.service";
import {CustomJwtPayload} from "../../models/custom-jwt-payload";
import {jwtDecode} from "jwt-decode/build/esm";

@Component({
  selector: 'app-add-provider',
  templateUrl: './add-provider.component.html',
  styleUrls: ['./add-provider.component.css']
})
export class AddProviderComponent implements OnInit {
  @Output() onSubmit = new EventEmitter();

  addProviderForm!: FormGroup;

  constructor(private fb: FormBuilder,
              private providerService: ProviderService) {}

  ngOnInit(): void {
    this.addProviderForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      address: ['', Validators.required],
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9+ ]+$')]],
    });
  }

  addProvider(): void {
    if (this.addProviderForm.valid) {
      console.log('Dodavanje prevoznika:', this.addProviderForm.value);
      this.providerService.addProvider(this.addProviderForm.value).subscribe({
        next: (response) => {
          alert(response);
          this.addProviderForm.reset();
        },
        error: (err) => {
          alert('An error occured: '+ err);
          console.log(err);
        }
      });
      //alert('New transport provider added successfully!');
    } else {
      alert('Please enter valid data.');
    }
  }

}
