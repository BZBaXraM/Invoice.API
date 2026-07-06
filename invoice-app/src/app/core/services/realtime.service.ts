import { isPlatformBrowser } from '@angular/common';
import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CustomerResponse } from '../models/customer.model';
import { InvoiceResponse } from '../models/invoice.model';
import { CustomerIdEvent, InvoiceIdEvent, InvoiceStatusChangedEvent } from '../models/realtime.model';
import { AuthService } from './auth.service';

function hubUrl(): string {
  return `${environment.apiUrl.replace(/\/api\/?$/, '')}/hubs/notifications`;
}

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly auth = inject(AuthService);

  private connection: signalR.HubConnection | null = null;

  readonly connected = signal(false);

  readonly customerCreated$ = new Subject<CustomerResponse>();
  readonly customerUpdated$ = new Subject<CustomerResponse>();
  readonly customerArchived$ = new Subject<CustomerIdEvent>();
  readonly customerUnarchived$ = new Subject<CustomerIdEvent>();
  readonly customerDeleted$ = new Subject<CustomerIdEvent>();

  readonly invoiceCreated$ = new Subject<InvoiceResponse>();
  readonly invoiceUpdated$ = new Subject<InvoiceResponse>();
  readonly invoiceStatusChanged$ = new Subject<InvoiceStatusChangedEvent>();
  readonly invoiceArchived$ = new Subject<InvoiceIdEvent>();
  readonly invoiceDeleted$ = new Subject<InvoiceIdEvent>();

  connect(): void {
    if (!this.isBrowser || this.connection) {
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl(), { accessTokenFactory: () => this.auth.getAccessToken() ?? '' })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('CustomerCreated', (payload: CustomerResponse) => this.customerCreated$.next(payload));
    connection.on('CustomerUpdated', (payload: CustomerResponse) => this.customerUpdated$.next(payload));
    connection.on('CustomerArchived', (payload: CustomerIdEvent) => this.customerArchived$.next(payload));
    connection.on('CustomerUnarchived', (payload: CustomerIdEvent) => this.customerUnarchived$.next(payload));
    connection.on('CustomerDeleted', (payload: CustomerIdEvent) => this.customerDeleted$.next(payload));

    connection.on('InvoiceCreated', (payload: InvoiceResponse) => this.invoiceCreated$.next(payload));
    connection.on('InvoiceUpdated', (payload: InvoiceResponse) => this.invoiceUpdated$.next(payload));
    connection.on('InvoiceStatusChanged', (payload: InvoiceStatusChangedEvent) =>
      this.invoiceStatusChanged$.next(payload),
    );
    connection.on('InvoiceArchived', (payload: InvoiceIdEvent) => this.invoiceArchived$.next(payload));
    connection.on('InvoiceDeleted', (payload: InvoiceIdEvent) => this.invoiceDeleted$.next(payload));

    connection.onreconnected(() => this.connected.set(true));
    connection.onreconnecting(() => this.connected.set(false));
    connection.onclose(() => this.connected.set(false));

    this.connection = connection;

    connection
      .start()
      .then(() => this.connected.set(true))
      .catch(() => this.connected.set(false));
  }

  disconnect(): void {
    this.connection?.stop();
    this.connection = null;
    this.connected.set(false);
  }
}
