import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AtualizarBeneficiarioRequest,
  BeneficiarioListItem,
  ComunidadeOpcao,
  CriarBeneficiarioRequest,
  PagedResult,
} from './models/beneficiario.model';

@Injectable({ providedIn: 'root' })
export class BeneficiariosService {
  private readonly url = `${environment.apiUrl}/beneficiarios`;

  constructor(private http: HttpClient) {}

  listar(pagina: number, tamanhoPagina: number, busca?: string, ativo: boolean = true): Observable<PagedResult<BeneficiarioListItem>> {
    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanhoPagina', tamanhoPagina)
      .set('ativo', ativo);
    if (busca) params = params.set('busca', busca);
    return this.http.get<PagedResult<BeneficiarioListItem>>(this.url, { params });
  }

  obterPorId(id: number): Observable<BeneficiarioListItem> {
    return this.http.get<BeneficiarioListItem>(`${this.url}/${id}`);
  }

  listarComunidades(): Observable<ComunidadeOpcao[]> {
    return this.http.get<ComunidadeOpcao[]>(`${this.url}/comunidades`);
  }

  criar(dto: CriarBeneficiarioRequest): Observable<BeneficiarioListItem> {
    return this.http.post<BeneficiarioListItem>(this.url, dto);
  }

  atualizar(id: number, dto: AtualizarBeneficiarioRequest): Observable<BeneficiarioListItem> {
    return this.http.put<BeneficiarioListItem>(`${this.url}/${id}`, dto);
  }

  inativar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}