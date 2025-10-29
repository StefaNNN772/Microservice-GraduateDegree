import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {AuthService} from "../../services/auth.service";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {Router} from "@angular/router";
import {jwtDecode} from "jwt-decode";
import {CustomJwtPayload} from "../../models/custom-jwt-payload";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  @Input() title: string = "Login to SerbiaBus";
  @Input() primaryBtnText: string = "Sign In";
  @Input() secondaryBtnText: string = "Sign Up";
  @Input() disablePrimaryBtn: boolean = true;
  @Output() onSubmit = new EventEmitter();

  loginForm!: FormGroup;

  constructor(private fb: FormBuilder,
              private authService: AuthService,
              private router: Router) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });

    // @ts-ignore
    google.accounts.id.initialize({
      client_id: "859918477842-uol8btg50e4a0ksuub00djok1ha0vgn0.apps.googleusercontent.com",
      callback: this.handleCredentialResponse.bind(this),
      auto_select: false,
      cancel_on_tap_outside: true,

    });
    // @ts-ignore
    google.accounts.id.renderButton(
      // @ts-ignore
      document.getElementById("google-button"),
      { theme: "outline", size: "large", width: "100%" }
    );
    // @ts-ignore
    google.accounts.id.prompt((notification: PromptMomentNotification) => {});
  }

  async handleCredentialResponse(response: any) {
    // Here will be your response from Google.
    console.log(response);
  }

  login() {
    if (this.loginForm.valid) {
      const loginData = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password
      };

      this.authService.login(loginData).subscribe({
        next: (response) => {
          localStorage.setItem('jwt', response.accessToken);
          const decoded : CustomJwtPayload = jwtDecode(response.accessToken);
          localStorage.setItem('userId', decoded.id);
          localStorage.setItem('userEmail', decoded.sub);
          localStorage.setItem('userRole', decoded.role);
          this.authService.setUserRole(decoded.role);
          if (decoded.role === 'Admin') {
            this.router.navigate(['/discount-requests']);
          }
          if (decoded.role === 'User') {
            this.router.navigate(['/main']);
          }
          if (decoded.role === 'TransportProvider') {
            this.router.navigate(['/schedules']);
          }
        },
        error: (err) => {
          alert('Bad credentials');
        }
      });
    } else {
      alert('Please provide valid credentials');
    }
  }

  googleLogin() : void {
    const googleSignInUrl = 'https://accounts.google.com/o/oauth2/v2/auth' +
      '?client_id=859918477842-uol8btg50e4a0ksuub00djok1ha0vgn0.apps.googleusercontent.com' +
      '&redirect_uri=http://localhost:5231/auth/google/callback' +
      '&response_type=code' +
      '&scope=email profile';

    //alert(googleSignInUrl)

    window.location.href = googleSignInUrl;
  }


}
