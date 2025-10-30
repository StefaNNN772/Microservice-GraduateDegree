import {Component, OnInit} from '@angular/core';
import {User} from "../../models/User";
import {UserRole} from "../../enum/user-role";
import {UserService} from "../../services/user.service";
import {formatDate} from "@angular/common";
import {UpdateUserProfileDto} from "../../dtos/update-user-profile-dto";
import {DiscountType} from "../../enum/discount-type";
import {DiscountStatus} from "../../enum/discount-status";
import {environment} from "../../../assets/environments/environment";

@Component({
  selector: 'app-user-profile-view',
  templateUrl: './user-profile-view.component.html',
  styleUrls: ['./user-profile-view.component.css']
})
export class UserProfileViewComponent implements OnInit{
  user : User = {
    id: 0,
    name: '',
    lastName: '',
    email: '',
    passwordHash: '',
    birthday: new Date(),
    role: UserRole.User,
    discountType: DiscountType.Pupil,
    discountStatus: DiscountStatus.NoRequest,
    discountValidUntil: new Date(),
    discountDocumentPath: '',
    profileImagePath: ''
  };
  birthdayString: string = "";
  discountType: string = '';
  proofImage: File | null = null;
  showModal = false;
  confirmingDelete = false;
  selectedImagePath = '';
  selectedProfileImagePath = '';
  hoveringImage = false;

  constructor(private userService: UserService) {
  }

  ngOnInit(): void {
    const userId = Number(localStorage.getItem("userId"));
    console.log(userId);
    this.userService.getUserById(userId).subscribe({
      next: (response) => {
        this.user = response;
        console.log(this.user)
        console.log(response.birthday)
        this.birthdayString = formatDate(response.birthday, 'yyyy-MM-dd', 'en-US');
        if (this.user.profileImagePath != null || this.user.profileImagePath != ''){
          this.selectedProfileImagePath = this.getFullUrl(this.user.profileImagePath);
        }
      },
      error: (err) => {
        alert('An error occured: '+ err);
      }
    });
  }

  isFormValid(): boolean {
    const today = new Date();
    this.user.birthday = new Date(this.birthdayString);

    const isBirthdayValid = this.user.birthday < today;

    return (
      this.user.name?.trim() &&
      this.user.lastName?.trim() &&
      this.birthdayString?.trim() &&
      isBirthdayValid
    ) ? true : false;
  }

  updateProfile(): void{
    if (this.isFormValid()){
      let updatedData: UpdateUserProfileDto = {
        name: this.user.name,
        lastName: this.user.lastName,
        birthday: this.user.birthday
      }
        this.userService.updateProfile(this.user.id, updatedData).subscribe({
          next: (response) => {
            console.log(response)
            alert(response.message);
          },
          error: (err) => {
            alert('An error occured: ' + err);
            console.log(err)
          }
        });
    }
    else{
      alert("Data not valid.")
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.proofImage = input.files[0];
    }
  }

  submitDiscountRequest(): void {
    if (this.discountType && this.proofImage) {
      const formData = new FormData();
      formData.append('discountType', this.discountType);
      formData.append('proofImage', this.proofImage);
      this.userService.submitDiscountRequest(this.user.id, formData).subscribe({
        next: (response) => {
          console.log(response)
          alert(response.message);
          location.reload();
        },
        error: (err) => {
          alert('An error occured: ' + err);
          console.log(err)
        }
      });
    }
  }

  getFileName(path: string): string {
    const segments = path.split('\\');
    return segments[segments.length - 1];
  }

  getFullUrl(path: string): string {
    return `${environment.apiUrl}${path}`;
  }

  openImage(path: string) {
    this.selectedImagePath = this.getFullUrl(path);
    this.showModal = true;
  }

  deleteImage() {
    if (this.user.discountStatus === DiscountStatus.Pending){
      this.userService.deleteDiscountRequest(this.user.id).subscribe({
        next: (response) => {
          console.log(response)
          console.log(response.message)
          alert(response.message);
          location.reload();
        },
        error: (err) => {
          alert('An error occured: ' + err);
          console.log(err)
        }
      });
    }
  }

  closeImage() {
    this.showModal = false;
  }

  DiscountTypeEnum = DiscountType;
  protected readonly DiscountStatus = DiscountStatus;

  triggerImageUpload() {
    const input = document.getElementById('profileImageInput') as HTMLInputElement;
    input.click();
  }

  onProfileImageSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      const formData = new FormData();
      formData.append('profileImage', file);
      this.userService.addProfilePicture(this.user.id, formData).subscribe({
        next: (response) => {
          console.log(response)
          alert(response.message);
          location.reload();
        },
        error: (err) => {
          alert('An error occured: ' + err);
          console.log(err)
        }
      });
    }
  }

  deleteProfileImage(event: Event) {
    event.stopPropagation();
    this.userService.deleteProfileImage(this.user.id).subscribe({
      next: (response) => {
        console.log(response)
        console.log(response.message)
        alert(response.message);
        location.reload();
      },
      error: (err) => {
        alert('An error occured: ' + err);
        console.log(err)
      }
    });
    this.user.profileImagePath = '';
  }
}
