import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {BehaviorSubject, Observable} from "rxjs";
import {environment} from "../../assets/environments/environment";
import {jwtDecode} from "jwt-decode";



export interface AuthResponse {
  accessToken: string;
  expiresIn: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private headers = new HttpHeaders({
    'Content-Type': 'application/json',
    skip: 'true',
  });

  private userSubject = new BehaviorSubject<string | null>(null);
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadUserFromLocalStorage();
  }

  login(auth: any): Observable<AuthResponse> {
    const body = {
      ...auth,
    };
    return this.http.post<AuthResponse>(`${environment.apiUrl}login`, body);
  }

  register(formData: any): Observable<AuthResponse> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post<AuthResponse>(`${environment.apiUrl}register`, formData, { headers });
  }

  setUserRole(role: string) {
    this.userSubject.next(role);
  }

  logout() {
    localStorage.removeItem('jwt');
    localStorage.removeItem('userId');
    localStorage.removeItem('userEmail');
    localStorage.removeItem('userRole');
    this.userSubject.next(null);
  }

  private loadUserFromLocalStorage() {
    const token = localStorage.getItem('jwt');
    if (token) {
      const decoded: any = jwtDecode(token);
      this.setUserRole(decoded.role);
    }
  }
}
