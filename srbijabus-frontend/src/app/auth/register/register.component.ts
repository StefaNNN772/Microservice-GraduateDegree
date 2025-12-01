import { Component } from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {AuthService} from "../../services/auth.service";
import {ActivatedRoute, Router} from "@angular/router";
import {NgxImageCompressService} from "ngx-image-compress";
import {CustomJwtPayload} from "../../models/custom-jwt-payload";
import {jwtDecode} from "jwt-decode";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  selectedFile: File | null = null;
  profilePic: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private imageCompress: NgxImageCompressService,
    private route: ActivatedRoute

  ) {
    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      lastname: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      birthday: ['', Validators.required]
      //profilePic: [null]
    });
  }

  base64ToFile(base64String: string, fileName: string): File {
    const arr = base64String.split(',');
    const mime = arr[0].match(/:(.*?);/)![1];
    const bstr = atob(arr[1]);
    let n = bstr.length;
    const u8arr = new Uint8Array(n);
    while (n--) {
      u8arr[n] = bstr.charCodeAt(n);
    }
    return new File([u8arr], fileName, { type: mime });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.imageCompress.compressFile(URL.createObjectURL(file), -1, 50, 50).then(
        (compressedImage) => {
          this.profilePic = compressedImage;
          this.selectedFile = this.base64ToFile(compressedImage, file.name);
        });
    }
  }



  register(): void {
    // TODO: add profile pic

    if (this.registerForm.valid) { //&& this.selectedFile != null) {
      const formData = {
        email: this.registerForm.get('username')?.value,
        password: this.registerForm.get('password')?.value,
        name: this.registerForm.get('name')?.value,
        lastname: this.registerForm.get('lastname')?.value,
        birthday: this.registerForm.get('birthday')?.value
      };
      //formData.append('userPhoto', this.selectedFile);

      this.authService.register(formData).subscribe({
        next: (response) => {
          alert('Registration successful!');
          localStorage.setItem('jwt', response.accessToken);
          const decoded : CustomJwtPayload = jwtDecode(response.accessToken);
          localStorage.setItem('userId', decoded.id);
          localStorage.setItem('userEmail', decoded.sub);
          localStorage.setItem('userRole', decoded.role);
          this.router.navigate(['/main']);
        },
        error: () => {
          alert('Registration failed. Please try again.');
        }
      });
    } else {
      alert('Please fill out all required fields with valid information.');
    }
  }

  googleRegister(): void{

  }

}
