import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BackupService {
  private readonly http = inject(HttpClient);

  /** Admin only: full-database JSON backup. */
  downloadBackup(): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/admin/backup`, { responseType: 'blob' });
  }

  /** Current user's own data export (profile, customers, invoices, payments, history). */
  exportMyData(): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/account/profile/export`, { responseType: 'blob' });
  }
}
