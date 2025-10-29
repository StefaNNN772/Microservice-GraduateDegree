import {Component, inject, OnInit, ViewChild} from '@angular/core';
import { Router } from '@angular/router';
import {BusLine, BusService} from "../../services/buslines.service";
import {MatTableDataSource} from "@angular/material/table";
import {AuthService} from "../../services/auth.service";
import {MatPaginator} from "@angular/material/paginator";
import {MatSort} from "@angular/material/sort";
import {UserService} from "../../services/user.service";
import {FavouriteRoute} from "../../models/favourite-route";

@Component({
  selector: 'app-favourite-routes',
  templateUrl: './favourite-routes.component.html',
  styleUrls: ['./favourite-routes.component.css']
})
export class FavouriteRoutesComponent implements OnInit {
  favouriteRoutes: FavouriteRoute[] = [];
  selectedRoute: string = '';
  dataSource: MatTableDataSource<BusLine>;

  authService = inject(AuthService);
  userRole: string | null = null;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = ['departure', 'arrival', 'departureDate', 'departureTime', 'arrivalTime', 'availableSeats', 'provider', 'discount', 'price', 'options'];

  availableLines: BusLine[] = [];
  filteredLines: any[] = [];

  selectedDate: string = '';
  minDate: string = '';

  constructor(private busService: BusService,
              private userService: UserService,
              private router: Router) {
    this.dataSource = new MatTableDataSource(this.availableLines);
  }

  ngOnInit() {
    this.userService.getFavouriteRoutes().subscribe({
      next: (response) => {
        this.favouriteRoutes = response;
      },
      error: (err) => {
        alert('An error occured: ' + err);
        console.log(err)
      }
    });

    this.minDate = new Date().toISOString().split('T')[0];

    this.dataSource.filterPredicate = (data: BusLine, filter: string) => {
      const dataStr = (
        data.departureTime + ' ' +
        data.arrivalTime + ' ' +
        data.provider
      ).toLowerCase();

      return dataStr.includes(filter);
    };
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  selectRoute(route: FavouriteRoute) {
    this.selectedDate = '';
    this.filteredLines = [];
    this.dataSource.data = [];
    this.availableLines = [];
    this.selectedRoute = route.departure+'-'+route.arrival;
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

  removeFromFavourites(route: FavouriteRoute, $event: MouseEvent) {
    this.userService.removeRouteFromFavourites(route.departure, route.arrival).subscribe({
      next: (response) => {
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
