import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, Observable, forkJoin, switchMap} from 'rxjs';
import {environment} from "../../assets/environments/environment";

export interface Schedule {
    id: number;
    departure: string;
    buslineid: string;
    arrival: string;
    departureTime: string;
    arrivalTime: string;
    availableSeats: number;
    price: number;
    pricePerKilometer: number;
    days: string;
    discount: number;
}

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {
  private apiUrl = `${environment.apiUrl}schedules`;

  constructor(private http: HttpClient) {}
  
  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`
    });
  }
  
  getSchedules(): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(`${this.apiUrl}/getSchedules`, {headers: this.getAuthHeaders()});
  }

  addSchedule(schedule: any): Observable<Schedule> {
    schedule.arrivalTime='';
    schedule.arrival='';
    schedule.buslineid='';
    return this.http.post<Schedule>(`${this.apiUrl}/addSchedule`, schedule, {headers: this.getAuthHeaders()});
  }

  updateSchedule(schedule: Schedule): Observable<{message: string}> {
    schedule.arrivalTime='';
    schedule.arrival='';
    schedule.buslineid='';
    schedule.price=0;
    schedule.departure='';
    return this.http.put<{message: string}>(`${this.apiUrl}/updateSchedule/${schedule.id}`, schedule, {headers: this.getAuthHeaders()});
  }

  deleteSchedule(id: number): Observable<{message: string}> {
    return this.http.delete<{message: string}>(`${this.apiUrl}/${id}`);
  }
}
