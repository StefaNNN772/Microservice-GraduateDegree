import {Component, OnInit} from '@angular/core';
import {DiscountType} from "../../enum/discount-type";
import {UserService} from "../../services/user.service";

@Component({
  selector: 'app-admin-approved-discounts',
  templateUrl: './admin-approved-discounts.component.html',
  styleUrls: ['./admin-approved-discounts.component.css']
})
export class AdminApprovedDiscountsComponent implements OnInit{
  requests: any[] = [];
  showModal = false;
  selectedImagePath = '';
  discountEnum = DiscountType;

  constructor(private userService: UserService) {
  }

  ngOnInit() {
    this.userService.getApprovedDiscounts().subscribe({
      next: (response) => {
        this.requests = response;
      },
      error: (err) => {
        alert('An error occured: ' + err);
        console.log(err)
      }
    });
  }

  getFullUrl(path: string): string {
    return `http://localhost:5231/${path}`;
  }

  getFileName(path: string): string {
    const segments = path.split('\\');
    return segments[segments.length - 1];
  }

  openImage(path: string) {
    this.selectedImagePath = this.getFullUrl(path);
    this.showModal = true;
  }

  closeImage() {
    this.showModal = false;
  }

}
