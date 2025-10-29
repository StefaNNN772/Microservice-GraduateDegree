import {Component, OnInit} from '@angular/core';
import {Router} from "@angular/router";
import {AuthService} from "../../services/auth.service";

@Component({
  selector: 'app-navbar-admin',
  templateUrl: './navbar-admin.component.html',
  styleUrls: ['./navbar-admin.component.css']
})
export class NavbarAdminComponent implements OnInit{
  userId: number = 0;

  constructor(private router: Router, private authService:AuthService) {}

  ngOnInit() {
    this.userId = Number(localStorage.getItem("userId"));
  }

  menuItemClicked(option: string) {
    switch (option) {
      case 'logout':
        this.authService.logout();
        this.router.navigate(['/login']);
        break;
      default:
        break;
    }
  }

}
