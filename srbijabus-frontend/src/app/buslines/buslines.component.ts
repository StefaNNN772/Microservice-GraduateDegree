import { Component, OnInit, ViewChild, AfterViewInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BusLine, BusService } from "../services/buslines.service";
import {MatPaginator, MatPaginatorModule} from '@angular/material/paginator';
import {MatSort, MatSortModule} from '@angular/material/sort';
import {MatTableDataSource, MatTableModule} from '@angular/material/table';
import {jwtDecode} from 'jwt-decode';
import {AuthService} from "../services/auth.service";
import {UserService} from "../services/user.service";

@Component({
  selector: 'app-buslines',
  templateUrl: './buslines.component.html',
  styleUrls: ['./buslines.component.css']
})
export class BuslinesComponent implements OnInit {
  busLines: BusLine[] = [];
  routes: string[] = [];
  selectedRoute: string = '';
  dataSource: MatTableDataSource<BusLine>;

  authService = inject(AuthService);
  userRole: string | null = null;
  isSelectedRouteFavourite: boolean = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = ['departure', 'arrival', 'departureDate', 'departureTime', 'arrivalTime', 'availableSeats', 'provider', 'discount', 'price'];

  availableLines: BusLine[] = [];
  filteredLines: any[] = [];

  selectedDate: string = '';
  minDate: string = '';

  constructor(private busService: BusService, private router: Router, private userService: UserService) {
    this.dataSource = new MatTableDataSource(this.availableLines);
  }

  ngOnInit() {
    // Dobijanje role-a od korisnika da bi posle znali da li prikazati opciju za rezervaciju karte
    this.authService.user$.subscribe(role => this.userRole = role);

    if (this.canSeeOptionsColumn()) {
    this.displayedColumns.push('options');
  }

    this.minDate = new Date().toISOString().split('T')[0];
    this.loadRoutes();

    this.dataSource.filterPredicate = (data: BusLine, filter: string) => {
      const dataStr = (
        data.departureTime + ' ' +
        data.arrivalTime + ' ' +
        data.provider
      ).toLowerCase();

      return dataStr.includes(filter);
    };
  }

  hasRole(role: string): boolean {
    return this.userRole == role;
  }

  canSeeOptionsColumn(): boolean {
    return this.hasRole('User');
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  loadRoutes() {
    this.busService.getRoutes().subscribe(data => {
      this.routes = data;
    });
  }

  onRouteChange() {
    this.isFavourite(this.selectedRoute);
    this.selectedDate = '';
    this.filteredLines = [];
    this.dataSource.data = [];
    this.availableLines = [];
    if (this.selectedRoute !== '') {
      this.busService.getLinesForRoute(this.selectedRoute).subscribe(data => {
        this.availableLines = data;
      });
    }
  }

  onDateChange() {
    this.filteredLines = this.availableLines.filter(
      line => line.departureDate === this.selectedDate
    );

    this.dataSource.data = this.filteredLines;
  }

  busReservation(id: number) {
    if (id && id > 0) {
      this.router.navigate(['/busreservation', id]);
    } else {
      console.error('Invalid bus line ID:', id);
    }
  }

  toggleFavourite() {
    let [departure, arrival] = this.selectedRoute.split('-');
    if (!this.isSelectedRouteFavourite){
      this.userService.addRouteToFavourites(departure, arrival).subscribe({
        next: (response) => {
          alert(response.message);
          this.isSelectedRouteFavourite = true;
        },
        error: (err) => {
          alert('An error occured: ' + err);
          console.log(err)
        }
      });
    }
    else if (this.isSelectedRouteFavourite){
      this.userService.removeRouteFromFavourites(departure, arrival).subscribe({
        next: (response) => {
          alert(response.message);
          this.isSelectedRouteFavourite = false;
        },
        error: (err) => {
          alert('An error occured: ' + err);
          console.log(err)
        }
      });
    }
  }

  isFavourite(route: string): void {
    if (this.selectedRoute !== ''){
      let [departure, arrival] = this.selectedRoute.split('-');
      this.userService.isRouteFavourite(departure, arrival).subscribe({
        next: (response) => {
          this.isSelectedRouteFavourite = response;
        },
        error: (err) => {
          alert('An error occured: ' + err);
          console.log(err)
        }
      });
    }
  }
}
