import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, ResponseModel, ResponseModelData } from '../models/common.model';
import {
  CreateRecurringInvoiceRequest,
  RecurringInvoiceResponse,
  UpdateRecurringInvoiceRequest,
} from '../models/recurring.model';

@Injectable({ providedIn: 'root' })
export class RecurringService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/recurring-invoices`;

  getList(pageNumber: number, pageSize: number): Observable<ResponseModelData<PagedResult<RecurringInvoiceResponse>>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<ResponseModelData<PagedResult<RecurringInvoiceResponse>>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<ResponseModelData<RecurringInvoiceResponse>> {
    return this.http.get<ResponseModelData<RecurringInvoiceResponse>>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateRecurringInvoiceRequest): Observable<ResponseModelData<RecurringInvoiceResponse>> {
    return this.http.post<ResponseModelData<RecurringInvoiceResponse>>(this.baseUrl, request);
  }

  update(id: string, request: UpdateRecurringInvoiceRequest): Observable<ResponseModelData<RecurringInvoiceResponse>> {
    return this.http.put<ResponseModelData<RecurringInvoiceResponse>>(`${this.baseUrl}/${id}`, request);
  }

  toggle(id: string): Observable<ResponseModelData<RecurringInvoiceResponse>> {
    return this.http.put<ResponseModelData<RecurringInvoiceResponse>>(`${this.baseUrl}/${id}/toggle`, {});
  }

  delete(id: string): Observable<ResponseModel> {
    return this.http.delete<ResponseModel>(`${this.baseUrl}/${id}`);
  }
}
