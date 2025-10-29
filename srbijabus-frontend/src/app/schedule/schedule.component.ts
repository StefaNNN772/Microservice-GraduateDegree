import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { Schedule, ScheduleService } from "../services/schedule.service";
import {FormBuilder, FormControl, FormGroup, Validators} from "@angular/forms";
import {ValidatorFn, AbstractControl, ValidationErrors} from "@angular/forms";
import { scheduled } from 'rxjs';
import {MatPaginator, MatPaginatorModule} from '@angular/material/paginator';
import {MatSort, MatSortModule} from '@angular/material/sort';
import {MatTableDataSource, MatTableModule} from '@angular/material/table';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';

@Component({
  selector: 'app-schedule',
  templateUrl: './schedule.component.html',
  styleUrls: ['./schedule.component.css'],
})
export class ScheduleComponent implements OnInit, AfterViewInit {
  schedules: Schedule[] = [];
  schedulesForm: FormGroup;
  editingSchedule: Schedule | null = null;
  dataSource: MatTableDataSource<Schedule>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = ['departure', 'arrival', 'departureTime', 'arrivalTime', 'availableSeats', 'price', 'discount', 'options'];

  daysList = [
  { key: 'monday', short: 'Mon', full: 'Monday' },
  { key: 'tuesday', short: 'Tue', full: 'Tuesday' },
  { key: 'wednesday', short: 'Wed', full: 'Wednesday' },
  { key: 'thursday', short: 'Thu', full: 'Thursday' },
  { key: 'friday', short: 'Fri', full: 'Friday' },
  { key: 'saturday', short: 'Sat', full: 'Saturday' },
  { key: 'sunday', short: 'Sun', full: 'Sunday' }
];

  constructor(private schedulesService: ScheduleService, private fb: FormBuilder) {
    this.schedulesForm = this.fb.group({
      id: [null],
      buslineid: [null],
      departure: ['', [Validators.required, Validators.pattern(/^[A-Z][a-z]+(?:[ -][A-Z][a-z]+)*$/)]],
      departureTime: ['', Validators.required],
      availableSeats: ['', [Validators.required, Validators.min(1), Validators.pattern(/^\d+$/)]],
      discount: [0, [Validators.required, Validators.min(0), Validators.max(100), Validators.pattern(/^\d+(\.\d{1,2})?$/)]],
      pricePerKilometer: [0, [Validators.required, Validators.min(1), Validators.pattern(/^\d+(\.\d{1,2})?$/)]],
      days: this.fb.group({
        everyday: [false],
        monday: [false],
        tuesday: [false],
        wednesday: [false],
        thursday: [false],
        friday: [false],
        saturday: [false],
        sunday: [false],
      }, {Validators: [this.atLeastOneDayValidator()]})
    });
    this.dataSource = new MatTableDataSource(this.schedules);
  }

  ngOnInit() {
    this.loadSchedules();
  }

  get daysGroup(): FormGroup {
    return this.schedulesForm.get('days') as FormGroup;
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

  loadSchedules() {
    this.schedulesService.getSchedules().subscribe(data => {
      this.schedules = data;
      this.dataSource.data = data;
    });
  }

  get departure() {
    return this.schedulesForm.get('departure');
  }

  get pricePerKilometer() {
    return this.schedulesForm.get('pricePerKilometer');
  }

  get discount() {
    return this.schedulesForm.get('discount');
  }

  get availableSeats() {
    return this.schedulesForm.get('availableSeats');
  }

  get departureTime() {
    return this.schedulesForm.get('departureTime');
  }

  toggleEveryday(): void {
    const days = this.daysGroup;
    const everydayControl = days.get('everyday');
    const isEverydayChecked = everydayControl?.value;
  
    const dayNames = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday'];
  
    dayNames.forEach(day => {
      const control = days.get(day);
      if (isEverydayChecked) {
        control?.disable({ emitEvent: false });
        control?.setValue(false, { emitEvent: false });
      } else {
        control?.enable({ emitEvent: false });
      }
    });
  }

  checkIndividualDays(): void {
    const days = this.daysGroup;
    const individualSelected = Object.keys(days.controls)
      .filter(day => day !== 'everyday')
      .some(day => days.get(day)?.value);
  
    if (individualSelected) {
      days.get('everyday')?.setValue(false, { emitEvent: false });
      this.daysGroup.get('everyday')?.enable({ emitEvent: false });
    }
  
    // Ako nijedan dan nije selektovan
    const noneSelected = Object.keys(days.controls)
      .filter(day => day !== 'everyday')
      .every(day => !days.get(day)?.value);
  
    if (!individualSelected && noneSelected) {
      days.get('everyday')?.enable({ emitEvent: false });
    }
  }

  atLeastOneDayValidator(): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const value = group.value;
      const selected = Object.keys(value).some(day => value[day]);
      return selected ? null : { noDaySelected: true };
    };
  }

  getFormattedDays(): string {
    const days = this.daysGroup.value;
  
    if (days.everyday) {
      return 'Everyday';
    }
  
    const selectedDays = Object.keys(days)
      .filter(day => day !== 'everyday' && days[day])
      .map(day => this.capitalize(day));
  
    return selectedDays.join(',');
  }

  capitalize(word: string): string {
    return word.charAt(0).toUpperCase() + word.slice(1);
  }

  submitForm() {
    if (this.schedulesForm.valid) {
      if (this.editingSchedule) {
        const formValue = this.schedulesForm.getRawValue();
        formValue.days = this.getFormattedDays();

        this.schedulesService.updateSchedule(formValue).subscribe(response => {
          alert(response.message);
          this.loadSchedules();
          this.cancelEdit();
          this.schedulesForm.get("departure")?.enable();
          this.schedulesForm.get("days")?.enable();
        }, error => {
          alert("Error while updating schedule.");
          console.error(error);
        });
      } else {
        const formValue = this.schedulesForm.getRawValue();
        formValue.days = this.getFormattedDays();

        this.schedulesService.addSchedule(formValue).subscribe(() => {
          this.loadSchedules();
          this.schedulesForm.reset();
          this.schedulesForm.get("days")?.enable();
        });
      }
    } else {
      alert('Please provide valid credentials');
    }
  }
  cancelEdit() {
    this.editingSchedule = null;
    this.schedulesForm.reset();
    this.schedulesForm.get("departure")?.enable();
    this.schedulesForm.get("days")?.enable();
  }

  editSchedule(schedule: Schedule) {
    this.editingSchedule = schedule;
    this.editingSchedule.departure = this.editingSchedule.departure + "-" + this.editingSchedule.arrival;
    this.schedulesForm.get("departure")?.disable();
    this.schedulesForm.get("days")?.disable();
    this.schedulesForm.patchValue(schedule)
  }

  deleteSchedule(id: number) {
    if (confirm('Are you sure?\nThis may affect some other lines.')) {
      this.schedulesService.deleteSchedule(id).subscribe(response => {
        alert(response.message)
        this.loadSchedules();
      }, error => {
        alert("Error while deleting schedule!");
        console.error(error);
      });
    }
  }
}
