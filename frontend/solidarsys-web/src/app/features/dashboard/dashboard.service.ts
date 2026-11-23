import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardResumo } from './models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly url = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  obter(): Observable<DashboardResumo> {
    return this.http.get<DashboardResumo>(this.url);
  }
}
