import {UserRole} from "../enum/user-role";

export interface UpdateUserProfileDto{
  name: string,
  lastName: string,
  birthday: Date,
}
