import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, ResponseModel, ResponseModelData } from '../models/common.model';
import { CreateCustomerRequest, CustomerResponse, UpdateCustomerRequest } from '../models/customer.model';

export interface CustomerListQuery {
  pageNumber: number;
  pageSize: number;
  nameFilter?: string | null;
  sortBy?: string | null;
  sortDescending?: boolean;
  includeArchived?: boolean;
}

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/customers`;

  getList(query: CustomerListQuery): Observable<ResponseModelData<PagedResult<CustomerResponse>>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize)
      .set('sortDescending', String(!!query.sortDescending))
      .set('includeArchived', String(!!query.includeArchived));

    if (query.nameFilter) {
      params = params.set('nameFilter', query.nameFilter);
    }
    if (query.sortBy) {
      params = params.set('sortBy', query.sortBy);
    }

    return this.http.get<ResponseModelData<PagedResult<CustomerResponse>>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<ResponseModelData<CustomerResponse>> {
    return this.http.get<ResponseModelData<CustomerResponse>>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateCustomerRequest): Observable<ResponseModelData<CustomerResponse>> {
    return this.http.post<ResponseModelData<CustomerResponse>>(this.baseUrl, request);
  }

  update(id: string, request: UpdateCustomerRequest): Observable<ResponseModelData<CustomerResponse>> {
    return this.http.put<ResponseModelData<CustomerResponse>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<ResponseModel> {
    return this.http.delete<ResponseModel>(`${this.baseUrl}/${id}`);
  }

  archive(id: string): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.baseUrl}/${id}/archive`, {});
  }

  unarchive(id: string): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.baseUrl}/${id}/unarchive`, {});
  }
}
