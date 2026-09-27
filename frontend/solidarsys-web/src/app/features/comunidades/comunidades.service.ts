import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtualizarComunidadeRequest, CriarComunidadeRequest, PagedResult, ComunidadeListItem } from './models/comunidade.model';

@Injectable({ providedIn: 'root' })
export class ComunidadesService {
  private readonly url = `${environment.apiUrl}/comunidades`;

  constructor(private http: HttpClient) {}

  listar(pagina: number, tamanhoPagina: number, busca?: string, ativo?: boolean): Observable<PagedResult<ComunidadeListItem>> {
    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanhoPagina', tamanhoPagina);
    if (busca) params = params.set('busca', busca);
    if (ativo !== undefined) params = params.set('ativo', ativo);
    return this.http.get<PagedResult<ComunidadeListItem>>(this.url, { params });
  }

  obterPorId(id: number): Observable<ComunidadeListItem> {
    return this.http.get<ComunidadeListItem>(`${this.url}/${id}`);
  }

  criar(dto: CriarComunidadeRequest): Observable<ComunidadeListItem> {
    return this.http.post<ComunidadeListItem>(this.url, dto);
  }

  atualizar(id: number, dto: AtualizarComunidadeRequest): Observable<ComunidadeListItem> {
    return this.http.put<ComunidadeListItem>(`${this.url}/${id}`, dto);
  }

  alternarAtivo(id: number): Observable<void> {
    return this.http.patch<void>(`${this.url}/${id}/ativo`, {});
  }
}
