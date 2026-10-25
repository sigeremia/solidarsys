import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DoadorListItem, DoadorRequest, PagedResult } from './models/doador.model';

@Injectable({ providedIn: 'root' })
export class DoadoresService {
  private readonly url = `${environment.apiUrl}/doadores`;

  constructor(private http: HttpClient) {}

  listar(pagina: number, tamanhoPagina: number, busca?: string): Observable<PagedResult<DoadorListItem>> {
    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanhoPagina', tamanhoPagina)
      .set('ativo', true);
    if (busca) params = params.set('busca', busca);
    return this.http.get<PagedResult<DoadorListItem>>(this.url, { params });
  }

  criar(dto: DoadorRequest): Observable<DoadorListItem> {
    return this.http.post<DoadorListItem>(this.url, dto);
  }

  atualizar(id: number, dto: DoadorRequest): Observable<DoadorListItem> {
    return this.http.put<DoadorListItem>(`${this.url}/${id}`, dto);
  }

  inativar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}