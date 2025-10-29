import {Component, OnInit} from '@angular/core';
import {UserService} from "../../services/user.service";
import {DiscountType} from "../../enum/discount-type";

@Component({
  selector: 'app-admin-discount-review',
  templateUrl: './admin-discount-review.component.html',
  styleUrls: ['./admin-discount-review.component.css']
})
export class AdminDiscountReviewComponent implements OnInit{
  requests: any[] = [];
  showModal = false;
  selectedImagePath = '';
  discountEnum = DiscountType;

  constructor(private userService: UserService) {
  }

  ngOnInit() {
    this.userService.getDiscountRequests().subscribe({
      next: (response) => {
        this.requests = response;
      },
      error: (err) => {
        alert('An error occured: ' + err);
        console.log(err)
      }
    });
  }

  updateStatus(userId: number, status: string) {
    if (status === 'approve'){
      this.userService.updateDiscountRequest(userId, true).subscribe({
        next: (response) => {
          alert(response.message);
        },
        error: (err) => {
          alert('An error occured: ' + err);
          console.log(err)
        }
      });
    }
    else if (status === 'reject'){
      this.userService.updateDiscountRequest(userId, false).subscribe({
        next: (response) => {
          alert(response.message);
        },
        error: (err) => {
          alert('An error occured: ' + err);
          console.log(err)
        }
      });
    }
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
