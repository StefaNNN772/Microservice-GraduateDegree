import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from "../../assets/environments/environment";

export interface BusLine {
  id: number;
  departure: string;
  arrival: string;
  departureDate: string;
  departureTime: string;
  arrivalTime: string;
  availableSeats: number;
  price: number;
  provider: string;
}

export interface ReservationRequest {
  id: number;
  numberOfPassengers: number;
  paymentMethod: string;
  price: number;
}

export interface ReservationResponse {
  success: boolean;
  message: string;
  reservationId?: string;
}

export interface PaymentIntentRequest {
  amount: number;
  currency: string;
}

export interface PaymentIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
}

@Injectable({ providedIn: 'root' })
export class BusReservationService {
  private apiUrl = `${environment.apiUrl}busReservation`;

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`
    });
  }

  getBusLine(id: number): Observable<BusLine> {
    return this.http.get<BusLine>(`${this.apiUrl}/getBusLine/${id}`, { headers: this.getAuthHeaders() });
  }

  addReservation(data: ReservationRequest): Observable<ReservationResponse> {
    return this.http.post<ReservationResponse>(`${this.apiUrl}/add`, data, { headers: this.getAuthHeaders() });
  }

  createPaymentIntent(data: PaymentIntentRequest): Observable<PaymentIntentResponse> {
    return this.http.post<PaymentIntentResponse>(`${this.apiUrl}/create-payment-intent`, data, {headers: this.getAuthHeaders()});
  }
}