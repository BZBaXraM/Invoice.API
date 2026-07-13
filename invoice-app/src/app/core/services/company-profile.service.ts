import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ResponseModel, ResponseModelData } from '../models/common.model';
import {
  CompanyImageKind,
  CompanyProfileResponse,
  UpsertCompanyProfileRequest,
} from '../models/company-profile.model';

@Injectable({ providedIn: 'root' })
export class CompanyProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/company-profile`;

  get(): Observable<ResponseModelData<CompanyProfileResponse | null>> {
    return this.http.get<ResponseModelData<CompanyProfileResponse | null>>(this.baseUrl);
  }

  upsert(request: UpsertCompanyProfileRequest): Observable<ResponseModelData<CompanyProfileResponse>> {
    return this.http.put<ResponseModelData<CompanyProfileResponse>>(this.baseUrl, request);
  }

  uploadImage(kind: CompanyImageKind, file: File): Observable<ResponseModel> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ResponseModel>(`${this.baseUrl}/${kind}`, form);
  }

  /** <img> can't send the Authorization header — fetch the bytes as a blob instead. */
  getImageBlob(kind: CompanyImageKind): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${kind}`, { responseType: 'blob' });
  }

  deleteImage(kind: CompanyImageKind): Observable<ResponseModel> {
    return this.http.delete<ResponseModel>(`${this.baseUrl}/${kind}`);
  }
}
