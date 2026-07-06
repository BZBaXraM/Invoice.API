import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ResponseModelData } from '../models/common.model';
import {
  CustomerStatResponse,
  InvoiceStatusStatResponse,
  ServiceStatResponse,
} from '../models/report.model';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/reports`;

  private buildParams(from: Date, to: Date): HttpParams {
    return new HttpParams().set('from', from.toISOString()).set('to', to.toISOString());
  }

  getCustomerStats(from: Date, to: Date): Observable<ResponseModelData<CustomerStatResponse[]>> {
    return this.http.get<ResponseModelData<CustomerStatResponse[]>>(`${this.baseUrl}/customers`, {
      params: this.buildParams(from, to),
    });
  }

  getServiceStats(from: Date, to: Date): Observable<ResponseModelData<ServiceStatResponse[]>> {
    return this.http.get<ResponseModelData<ServiceStatResponse[]>>(`${this.baseUrl}/services`, {
      params: this.buildParams(from, to),
    });
  }

  getInvoiceStatusStats(from: Date, to: Date): Observable<ResponseModelData<InvoiceStatusStatResponse[]>> {
    return this.http.get<ResponseModelData<InvoiceStatusStatResponse[]>>(`${this.baseUrl}/invoice-status`, {
      params: this.buildParams(from, to),
    });
  }
}
