import {Injectable} from "@angular/core";
import {HttpClient, HttpHeaders, HttpParams} from "@angular/common/http";
import {Observable} from "rxjs";
import {User} from "../models/User";
import {environment} from "../../assets/environments/environment";
import {UpdateUserProfileDto} from "../dtos/update-user-profile-dto";
import {FavouriteRoute} from "../models/favourite-route";

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private headers = new HttpHeaders({
    'Content-Type': 'application/json',
    skip: 'true',
  });

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`
    });
  }

  private apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {
  }

  getUserById(userId: number): Observable<User> {
    console.log(`${this.apiUrl}users/${userId}`)
    return this.http.get<User>(`${this.apiUrl}users/${userId}`);
  }

  updateProfile(userId: number, user: UpdateUserProfileDto): Observable<{message: string}> {
    return this.http.put<{message: string}>(`${this.apiUrl}users/update/${userId}`, user);
  }

  submitDiscountRequest(userId: number, data: FormData): Observable<{message: string}> {
    return this.http.post<{message: string}>(`${this.apiUrl}users/request/discount/${userId}`, data);
  }

  getDiscountRequests(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}users/discount-requests`);
  }

  updateDiscountRequest(userId: number, approved: boolean): Observable<{message: string}> {
    return this.http.put<{message: string}>(`${this.apiUrl}users/update-discount-request/${userId}/${approved}`, {});
  }

  deleteDiscountRequest(userId: number): Observable<{ message: string }> {
    return this.http.put<{message: string}>(`${this.apiUrl}users/delete-discount-request/${userId}`, {});
  }

  getApprovedDiscounts(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}users/approved-discounts`);
  }

  addProfilePicture(userId: number, data: FormData): Observable<{message: string}> {
    return this.http.post<{message: string}>(`${this.apiUrl}users/add-profile-image/${userId}`, data);
  }

  deleteProfileImage(userId: number): Observable<{ message: string }> {
    return this.http.put<{message: string}>(`${this.apiUrl}users/delete-profile-image/${userId}`, {});
  }

  isRouteFavourite(departure: string, arrival: string): Observable<boolean> {
    const params = new HttpParams()
      .set('departure', departure)
      .set('arrival', arrival);
    return this.http.get<boolean>(`${this.apiUrl}favourites/route/check`, { params, headers: this.getAuthHeaders() });
  }

  addRouteToFavourites(departure: string, arrival: string): Observable<{ message: string }> {
    const data = {
      departure: departure,
      arrival: arrival
    };
    return this.http.post<{ message: string }>(`${this.apiUrl}favourites/route/add`, data, {headers: this.getAuthHeaders()});
  }

  removeRouteFromFavourites(departure: string, arrival: string): Observable<{ message: string }> {
    const data = {
      departure: departure,
      arrival: arrival
    };
    return this.http.post<{ message: string }>(`${this.apiUrl}favourites/route/remove`, data, {headers: this.getAuthHeaders()});
  }

  getFavouriteRoutes(): Observable<FavouriteRoute[]> {
    return this.http.get<FavouriteRoute[]>(`${this.apiUrl}favourites/routes`, {headers: this.getAuthHeaders()});
  }
}
