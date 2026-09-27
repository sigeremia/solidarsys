import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtualizarSetoresRequest, CriarSetoresRequest, PagedResult, SetoresListItem } from './models/setores.model';

@Injectable({ providedIn: 'root' })
export class SetoresService {
  private readonly url = `${environment.apiUrl}/setores`;

  constructor(private http: HttpClient) {}

  listar(pagina: number, tamanhoPagina: number, busca?: string, ativo?: boolean): Observable<PagedResult<SetoresListItem>> {
    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanhoPagina', tamanhoPagina);
    if (busca) params = params.set('busca', busca);
    if (ativo !== undefined) params = params.set('ativo', ativo);
    return this.http.get<PagedResult<SetoresListItem>>(this.url, { params });
  }

  obterPorId(id: number): Observable<SetoresListItem> {
    return this.http.get<SetoresListItem>(`${this.url}/${id}`);
  }

  criar(dto: CriarSetoresRequest): Observable<SetoresListItem> {
    return this.http.post<SetoresListItem>(this.url, dto);
  }

  atualizar(id: number, dto: AtualizarSetoresRequest): Observable<SetoresListItem> {
    return this.http.put<SetoresListItem>(`${this.url}/${id}`, dto);
  }

  alternarAtivo(id: number): Observable<void> {
    return this.http.patch<void>(`${this.url}/${id}/ativo`, {});
  }
}
