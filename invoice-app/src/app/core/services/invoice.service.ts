import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, ResponseModel, ResponseModelData } from '../models/common.model';
import {
  CreateInvoiceRequest,
  InvoiceResponse,
  InvoiceStatus,
  UpdateInvoiceRequest,
} from '../models/invoice.model';

export interface InvoiceListQuery {
  pageNumber: number;
  pageSize: number;
  customerId?: string | null;
  status?: InvoiceStatus | null;
  sortBy?: string | null;
  sortDescending?: boolean;
}

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/invoices`;

  getList(query: InvoiceListQuery): Observable<ResponseModelData<PagedResult<InvoiceResponse>>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize)
      .set('sortDescending', String(!!query.sortDescending));

    if (query.customerId) {
      params = params.set('customerId', query.customerId);
    }
    if (query.status) {
      params = params.set('status', query.status);
    }
    if (query.sortBy) {
      params = params.set('sortBy', query.sortBy);
    }

    return this.http.get<ResponseModelData<PagedResult<InvoiceResponse>>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<ResponseModelData<InvoiceResponse>> {
    return this.http.get<ResponseModelData<InvoiceResponse>>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateInvoiceRequest): Observable<ResponseModelData<InvoiceResponse>> {
    return this.http.post<ResponseModelData<InvoiceResponse>>(this.baseUrl, request);
  }

  update(id: string, request: UpdateInvoiceRequest): Observable<ResponseModelData<InvoiceResponse>> {
    return this.http.put<ResponseModelData<InvoiceResponse>>(`${this.baseUrl}/${id}`, request);
  }

  updateStatus(id: string, status: InvoiceStatus): Observable<ResponseModel> {
    return this.http.put<ResponseModel>(`${this.baseUrl}/${id}/status`, { status });
  }

  delete(id: string): Observable<ResponseModel> {
    return this.http.delete<ResponseModel>(`${this.baseUrl}/${id}`);
  }

  archive(id: string): Observable<ResponseModel> {
    return this.http.put<ResponseModel>(`${this.baseUrl}/${id}/archive`, {});
  }

  exportToPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/export`, { responseType: 'blob' });
  }
}
