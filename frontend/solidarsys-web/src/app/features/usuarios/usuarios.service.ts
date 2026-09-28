import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtualizarUsuarioRequest, CriarUsuarioRequest, PagedResult, UsuarioListItem } from './models/usuario.model';

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly url = `${environment.apiUrl}/usuarios`;

  constructor(private http: HttpClient) {}

  listar(pagina: number, tamanhoPagina: number, busca?: string, ativo?: boolean): Observable<PagedResult<UsuarioListItem>> {
    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanhoPagina', tamanhoPagina);
    if (busca) params = params.set('busca', busca);
    if (ativo !== undefined) params = params.set('ativo', ativo);
    return this.http.get<PagedResult<UsuarioListItem>>(this.url, { params });
  }

  obterPorId(id: number): Observable<UsuarioListItem> {
    return this.http.get<UsuarioListItem>(`${this.url}/${id}`);
  }

  criar(dto: CriarUsuarioRequest): Observable<UsuarioListItem> {
    return this.http.post<UsuarioListItem>(this.url, dto);
  }

  atualizar(id: number, dto: AtualizarUsuarioRequest): Observable<UsuarioListItem> {
    return this.http.put<UsuarioListItem>(`${this.url}/${id}`, dto);
  }

  alternarAtivo(id: number): Observable<void> {
    return this.http.patch<void>(`${this.url}/${id}/ativo`, {});
  }
}