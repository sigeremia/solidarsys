import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PeriodoRelatorio, Relatorio } from './models/relatorio.model';

@Injectable({ providedIn: 'root' })
export class RelatoriosService {
  private readonly url = `${environment.apiUrl}/relatorios`;

  constructor(private http: HttpClient) {}

  obter(periodo: PeriodoRelatorio): Observable<Relatorio> {
    const params = new HttpParams().set('periodo', periodo);
    return this.http.get<Relatorio>(this.url, { params });
  }
}
