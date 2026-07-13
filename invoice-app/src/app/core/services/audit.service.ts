import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLogResponse } from '../models/audit.model';
import { PagedResult, ResponseModelData } from '../models/common.model';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/audit`;

  getList(
    pageNumber: number,
    pageSize: number,
    entityType?: string | null,
    entityId?: string | null,
  ): Observable<ResponseModelData<PagedResult<AuditLogResponse>>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (entityType) {
      params = params.set('entityType', entityType);
    }
    if (entityId) {
      params = params.set('entityId', entityId);
    }

    return this.http.get<ResponseModelData<PagedResult<AuditLogResponse>>>(this.baseUrl, { params });
  }
}
