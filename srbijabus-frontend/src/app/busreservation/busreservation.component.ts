import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, NgZone } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BusLine, BusReservationService, ReservationRequest, ReservationResponse } from "../services/busreservation.service";
import { AuthService } from "../services/auth.service";
import { UserService } from "../services/user.service";
import { loadStripe, Stripe, StripeElements, StripeCardElement } from '@stripe/stripe-js';
import { environment } from "../../assets/environments/environment";

@Component({
  selector: 'app-busreservation',
  templateUrl: './busreservation.component.html',
  styleUrls: ['./busreservation.component.css']
})
export class BusreservationComponent implements OnInit, OnDestroy {
  busLine: BusLine | null = null;
  numberOfPassengers: number = 1;
  returnTicket: boolean = false;
  paymentMethod: string = '';
  loading: boolean = false;
  showStripeOverlay: boolean = false; // NOVO: koristi se samo za Stripe overlay, nikad za sakrivanje Stripe Elementa
  busId: number = 0;

  userRole: string | null = null;
  authService = inject(AuthService);

  // Stripe properties
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  cardElement: StripeCardElement | null = null;
  cardholderName: string = '';
  paymentError: string = '';
  stripeElementsReady: boolean = false;
  stripeInitialized: boolean = false;
  
  stripePublicKey = environment.stripePublicKey;
  
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private snackBar: MatSnackBar,
    private busReservationService: BusReservationService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  async ngOnInit(): Promise<void> {
    this.authService.user$.subscribe(role => this.userRole = role);

    // Inicijalizacija Stripe
    await this.initializeStripe();

    // Dobijanje ID-a iz route parametara
    this.route.params.subscribe(params => {
      this.busId = +params['id'];
      if (this.busId) {
        this.loadBusLineData();
      }
    });
  }

  async initializeStripe(): Promise<void> {
    try {
      this.stripe = await loadStripe(this.stripePublicKey);
      if (this.stripe) {
        this.elements = this.stripe.elements();
        this.stripeInitialized = true;
      }
    } catch (error) {
      console.error('Error initializing Stripe:', error);
    }
  }

  ngOnDestroy(): void {
    this.cleanupStripeElements();
  }

  loadBusLineData(): void {
    this.loading = true;
    
    this.busReservationService.getBusLine(this.busId)
      .subscribe({
        next: (data) => {
          this.busLine = data;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading bus line data:', error);
          this.snackBar.open('Greška pri učitavanju podataka o liniji', 'Zatvori', {
            duration: 3000
          });
          this.loading = false;
        }
      });
  }

  onPassengerCountChange(): void {
    if (this.busLine) {
      if (this.numberOfPassengers > this.busLine.availableSeats) {
        this.numberOfPassengers = this.busLine.availableSeats;
      }
      if (this.numberOfPassengers < 1) {
        this.numberOfPassengers = 1;
      }
    }
  }

  onPaymentMethodChange(): void {
    this.paymentError = '';
    
    if (this.paymentMethod === 'online') {
      if (this.getTotalPrice() < 50) {
        this.paymentError = 'Minimum amount for online payment is 50 RSD. Please add more passengers or pay at the station.';
        return;
      }
      // Trigger setup after DOM update
      this.cdr.detectChanges();
      setTimeout(() => {
        this.initializeStripeElements();
      }, 300);
    } else {
      this.cleanupStripeElements();
    }
  }

  cleanupStripeElements(): void {
    if (this.cardElement) {
      try {
        this.cardElement.destroy();
      } catch (error) {
        console.log('Error destroying card element:', error);
      }
      this.cardElement = null;
    }
    this.stripeElementsReady = false;
  }

  async initializeStripeElements(): Promise<void> {
    if (!this.elements || !this.stripeInitialized) {
      return;
    }
    this.cleanupStripeElements();
    await this.waitForDOMElement();

    const style = {
      base: {
        fontSize: '16px',
        color: '#424770',
        fontFamily: 'Arial, sans-serif',
        '::placeholder': {
          color: '#aab7c4',
        },
      },
      invalid: {
        color: '#dc3545',
        iconColor: '#dc3545'
      }
    };

    try {
      // Create single card element
      this.cardElement = this.elements.create('card', { style });
      this.cardElement.mount('#card-element');

      // Add event listener
      this.cardElement.on('change', (event) => {
        this.ngZone.run(() => {
          if (event.error) {
            this.paymentError = event.error.message;
          } else {
            this.paymentError = '';
          }
        });
      });

      this.stripeElementsReady = true;

    } catch (error) {
      console.error('Error setting up Stripe elements:', error);
      this.paymentError = 'Error setting up payment form. Please try again.';
      this.stripeElementsReady = false;
    }
  }

  private waitForDOMElement(): Promise<void> {
    return new Promise((resolve, reject) => {
      const maxAttempts = 50;
      let attempts = 0;

      const checkElement = () => {
        const cardEl = document.getElementById('card-element');
        if (cardEl) {
          resolve();
        } else {
          attempts++;
          if (attempts < maxAttempts) {
            setTimeout(checkElement, 100);
          } else {
            reject(new Error('Card element not found after maximum attempts'));
          }
        }
      };
      checkElement();
    });
  }

  getTotalPrice(): number {
    if (!this.busLine) return 0;
    return this.busLine.price * this.numberOfPassengers;
  }

  canPurchase(): boolean {
    const basicValidation = this.paymentMethod !== '' && this.busLine !== null && !this.showStripeOverlay;
    if (this.paymentMethod === 'online') {
      const totalAmount = this.getTotalPrice();
      return basicValidation && 
             this.cardholderName.trim() !== '' && 
             totalAmount >= 50 && 
             this.stripeElementsReady;
    }
    return basicValidation;
  }

  onPurchase(): void {
    if (!this.canPurchase() || !this.busLine) return;

    if (this.paymentMethod === 'online') {
      this.processOnlinePayment();
    } else {
      this.loading = true;
      this.processReservation();
    }
  }

  async processOnlinePayment(): Promise<void> {
    if (!this.stripe || !this.cardElement || !this.busLine) {
      console.error('Stripe not properly initialized');
      this.paymentError = 'Payment system not properly initialized.';
      return;
    }

    const totalAmount = this.getTotalPrice();
    if (totalAmount < 50) {
      this.paymentError = 'Minimum amount for online payment is 50 RSD.';
      return;
    }

    if (!this.stripeElementsReady) {
      this.paymentError = 'Payment form is not ready. Please wait a moment and try again.';
      return;
    }

    this.showStripeOverlay = true;
    this.paymentError = '';

    try {
      // Step 1: Create Payment Intent
      const paymentIntentResponse = await this.busReservationService.createPaymentIntent({
        amount: totalAmount,
        currency: 'rsd'
      }).toPromise();

      if (!paymentIntentResponse || !paymentIntentResponse.clientSecret) {
        throw new Error('Failed to create payment intent');
      }

      // Step 2: Confirm payment
      const { error, paymentIntent } = await this.stripe.confirmCardPayment(
        paymentIntentResponse.clientSecret,
        {
          payment_method: {
            card: this.cardElement,
            billing_details: {
              name: this.cardholderName.trim(),
            },
          },
        }
      );

      if (error) {
        console.error('Payment confirmation error:', error);
        this.ngZone.run(() => {
          this.paymentError = error.message || 'Payment failed. Please try again.';
          this.showStripeOverlay = false;
        });
      } else if (paymentIntent && paymentIntent.status === 'succeeded') {
        this.ngZone.run(() => {
          this.processReservation();
        });
      } else {
        console.error('Payment not successful:', paymentIntent);
        this.ngZone.run(() => {
          this.paymentError = 'Payment was not completed successfully.';
          this.showStripeOverlay = false;
        });
      }
    } catch (error) {
      console.error('Payment error:', error);
      this.ngZone.run(() => {
        if (error instanceof HttpErrorResponse && error.error?.error) {
          this.paymentError = error.error.error;
        } else {
          this.paymentError = 'Payment processing failed. Please try again.';
        }
        this.showStripeOverlay = false;
      });
    }
  }

  processReservation(): void {
    if (!this.busLine) return;

    // loading je true ili showStripeOverlay je true
    const reservationData: ReservationRequest = {
      id: this.busId,
      numberOfPassengers: this.numberOfPassengers,
      paymentMethod: this.paymentMethod,
      price: this.busLine.price * this.numberOfPassengers
    };

    this.busReservationService.addReservation(reservationData)
      .subscribe({
        next: (response) => {
          this.loading = false;
          this.showStripeOverlay = false;
          if (response.success) {
            this.snackBar.open(response.message, 'Close', {
              duration: 5000
            });
            setTimeout(() => {
              this.router.navigate(['/buslines']);
            }, 2000);
          } else {
            this.snackBar.open('Error: ' + response.message, 'Close', {
              duration: 5000
            });
          }
        },
        error: (error) => {
          console.error('Error:', error);
          this.snackBar.open('Error. Please try again.', 'Close', {
            duration: 5000
          });
          this.loading = false;
          this.showStripeOverlay = false;
        }
      });
  }
}