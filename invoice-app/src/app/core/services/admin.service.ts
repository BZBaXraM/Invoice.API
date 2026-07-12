import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminUser } from '../models/admin.model';
import { PagedResult, ResponseModel, ResponseModelData } from '../models/common.model';

export interface AdminUserListQuery {
  pageNumber: number;
  pageSize: number;
  searchFilter?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/admin`;

  getUsers(query: AdminUserListQuery): Observable<ResponseModelData<PagedResult<AdminUser>>> {
    let params = new HttpParams().set('pageNumber', query.pageNumber).set('pageSize', query.pageSize);

    if (query.searchFilter) {
      params = params.set('searchFilter', query.searchFilter);
    }

    return this.http.get<ResponseModelData<PagedResult<AdminUser>>>(`${this.baseUrl}/users`, { params });
  }

  disableUser(id: string): Observable<ResponseModel> {
    return this.http.put<ResponseModel>(`${this.baseUrl}/users/${id}/disable`, {});
  }

  enableUser(id: string): Observable<ResponseModel> {
    return this.http.put<ResponseModel>(`${this.baseUrl}/users/${id}/enable`, {});
  }

  deleteUser(id: string): Observable<ResponseModel> {
    return this.http.delete<ResponseModel>(`${this.baseUrl}/users/${id}`);
  }
}
