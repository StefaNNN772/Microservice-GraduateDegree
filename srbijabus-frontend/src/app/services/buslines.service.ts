import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import {environment} from "../../assets/environments/environment";

export interface BusLine {
    id: number;
    departure: string;
    arrival: string;
    departureTime: string;
    arrivalTime: string;
    availableSeats: number;
    departureDate: string;
    price: number;
    provider: string;
    discount: number;
}

@Injectable({ providedIn: 'root' })
export class BusService {
  private apiUrl = `${environment.apiUrl}buslines`;

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
      const token = localStorage.getItem('jwt');
      return new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
      });
    }

  getRoutes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/routes`);
  }

  getLinesForRoute(route: string): Observable<any[]> {
    return this.http.get<BusLine[]>(`${this.apiUrl}/routes/${route}`);
  }
}