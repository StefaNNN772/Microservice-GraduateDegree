import {UserRole} from "../enum/user-role";
import {DiscountType} from "../enum/discount-type";
import {DiscountStatus} from "../enum/discount-status";
import {FavouriteRoute} from "./favourite-route";

export interface User{
  id: number,
  name: string,
  lastName: string,
  email: string,
  passwordHash: string,
  birthday: Date,
  role: UserRole,
  discountType: DiscountType,
  discountStatus: DiscountStatus,
  discountValidUntil: Date,
  discountDocumentPath: string,
  profileImagePath: string,
  favouriteRoutes?: FavouriteRoute[];
}
