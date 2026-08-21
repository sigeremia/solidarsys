import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import {
  LoginRequest,
  LoginResponse,
  UsuarioInfo,
  EsqueceuSenhaRequest,
  RedefinirSenhaRequest,
} from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  // Access token em memória (nunca no localStorage — proteção contra XSS)
  private _accessToken = signal<string | null>(null);
  private _usuario = signal<UsuarioInfo | null>(null);

  readonly usuario = this._usuario.asReadonly();
  readonly isAuthenticated = computed(() => !!this._accessToken());
  readonly accessToken = this._accessToken.asReadonly();

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login`, request, { withCredentials: true })
      .pipe(
        tap((res) => {
          this._accessToken.set(res.accessToken);
          this._usuario.set(res.usuario);
        })
      );
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/logout`, {}, { withCredentials: true })
      .pipe(
        tap(() => {
          this._accessToken.set(null);
          this._usuario.set(null);
          this.router.navigate(['/login']);
        })
      );
  }

  refresh(): Observable<{ accessToken: string }> {
    return this.http
      .post<{ accessToken: string }>(`${this.apiUrl}/refresh`, {}, { withCredentials: true })
      .pipe(tap((res) => this._accessToken.set(res.accessToken)));
  }

  esqueceuSenha(request: EsqueceuSenhaRequest): Observable<{ mensagem: string }> {
    return this.http.post<{ mensagem: string }>(`${this.apiUrl}/esqueceu-senha`, request);
  }

  redefinirSenha(request: RedefinirSenhaRequest): Observable<{ mensagem: string }> {
    return this.http.post<{ mensagem: string }>(`${this.apiUrl}/redefinir-senha`, request);
  }

  carregarUsuarioAtual(): Observable<UsuarioInfo> {
    return this.http
      .get<UsuarioInfo>(`${this.apiUrl}/me`, { withCredentials: true })
      .pipe(tap((u) => this._usuario.set(u)));
  }

  setAccessToken(token: string): void {
    this._accessToken.set(token);
  }

  limparSessao(): void {
    this._accessToken.set(null);
    this._usuario.set(null);
  }
}
