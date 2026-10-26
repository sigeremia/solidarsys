import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AtualizarVisitaRequest,
  BeneficiarioOpcao,
  CriarVisitaRequest,
  PagedResult,
  VisitaListItem,
} from './models/visita.model';

@Injectable({ providedIn: 'root' })
export class VisitasService {
  private readonly url = `${environment.apiUrl}/visitas`;

  constructor(private http: HttpClient) {}

  listar(pagina: number, tamanhoPagina: number, busca?: string): Observable<PagedResult<VisitaListItem>> {
    let params = new HttpParams().set('pagina', pagina).set('tamanhoPagina', tamanhoPagina);
    if (busca) params = params.set('busca', busca);
    return this.http.get<PagedResult<VisitaListItem>>(this.url, { params });
  }

  listarBeneficiarios(): Observable<BeneficiarioOpcao[]> {
    return this.http.get<BeneficiarioOpcao[]>(`${this.url}/beneficiarios`);
  }

  obterPorId(id: number): Observable<VisitaListItem> {
    return this.http.get<VisitaListItem>(`${this.url}/${id}`);
  }

  criar(dto: CriarVisitaRequest): Observable<VisitaListItem> {
    return this.http.post<VisitaListItem>(this.url, dto);
  }

  atualizar(id: number, dto: AtualizarVisitaRequest): Observable<VisitaListItem> {
    return this.http.put<VisitaListItem>(`${this.url}/${id}`, dto);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
